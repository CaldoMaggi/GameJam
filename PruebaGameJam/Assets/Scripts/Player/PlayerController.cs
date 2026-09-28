using System;
using System.Collections;
using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Environment;
using CastleAssault.UI;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CastleAssault.Player
{
    /// <summary>
    /// Controlador principal del jugador: avance vertical automático,
    /// movimiento lateral, mecánica de escudo (1.5s activo, 3s cooldown),
    /// ralentización por aceite y sistema de 3 vidas con invulnerabilidad temporal.
    /// Soporta teclado, gamepad y controles táctiles (MobileInput).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float _horizontalSpeed = 7.5f;
        [SerializeField] private float _roadHalfWidth = 2.4f;

        [Header("Escudo")]
        [SerializeField] private float _shieldDuration = 1.5f;
        [SerializeField] private float _shieldCooldown = 3.0f;
        [SerializeField] private float _shieldSpeedFactor = 0.75f; // -25% de velocidad con escudo alzado

        [Header("Vida y Daño")]
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _invulnerabilityDuration = 1.0f;

        [Header("Efectos")]
        [SerializeField] private ShieldVisual _shieldVisual;

        // Estado interno
        private int _currentHealth;
        private bool _isShieldActive;
        private float _shieldTimeRemaining;
        private float _shieldCooldownRemaining;
        private float _oilSlowTimeRemaining;
        private float _invulnerabilityTimeRemaining;
        private bool _isDead;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;

        // Propiedades públicas para HUD y Spawners
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsShieldActive => _isShieldActive;
        public bool IsDead => _isDead;
        public float ShieldDuration => _shieldDuration;
        public float ShieldCooldown => _shieldCooldown;
        public float ShieldCooldownRemaining => _shieldCooldownRemaining;
        public float ShieldTimeRemaining => _shieldTimeRemaining;
        public bool IsShieldReady => !_isShieldActive && _shieldCooldownRemaining <= 0f;
        public bool IsSlowedByOil => _oilSlowTimeRemaining > 0f;

        // Eventos
        public event Action<int> OnHealthChanged;
        public event Action<bool> OnShieldStateChanged;
        public event Action OnPlayerDied;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();

            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (_spriteRenderer.sprite == null)
            {
                _spriteRenderer.sprite = PixelSpriteFactory.KnightSprite;
            }
            _spriteRenderer.sortingOrder = 10;

            if (_shieldVisual == null)
            {
                Transform existingShield = transform.Find("ShieldVisual");
                if (existingShield != null)
                {
                    _shieldVisual = existingShield.GetComponent<ShieldVisual>();
                }
                else
                {
                    GameObject shieldObj = new GameObject("ShieldVisual");
                    shieldObj.transform.SetParent(transform, false);
                    _shieldVisual = shieldObj.AddComponent<ShieldVisual>();
                }
            }

            // Asegurar Collider2D como trigger para proyectiles y pickups
            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                boxCol.isTrigger = true;
                boxCol.size = new Vector2(0.8f, 1.2f);
            }
        }

        private void Start()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth);
        }

        private void Update()
        {
            if (_isDead) return;

            UpdateTimers();
            HandleShieldInput();
            HandleBlinkEffect();
        }

        private void FixedUpdate()
        {
            if (_isDead)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            MovePlayer();
        }

        private void UpdateTimers()
        {
            // Temporizador de escudo activo
            if (_isShieldActive)
            {
                _shieldTimeRemaining -= Time.deltaTime;
                if (_shieldTimeRemaining <= 0f)
                {
                    DeactivateShield();
                }
            }
            else if (_shieldCooldownRemaining > 0f)
            {
                _shieldCooldownRemaining -= Time.deltaTime;
                if (_shieldCooldownRemaining < 0f) _shieldCooldownRemaining = 0f;
            }

            // Temporizador de ralentización por aceite
            if (_oilSlowTimeRemaining > 0f)
            {
                _oilSlowTimeRemaining -= Time.deltaTime;
            }

            // Temporizador de invulnerabilidad
            if (_invulnerabilityTimeRemaining > 0f)
            {
                _invulnerabilityTimeRemaining -= Time.deltaTime;
            }
        }

        private void HandleShieldInput()
        {
            if (ReadShieldInput())
            {
                TryActivateShield();
            }
        }

        public bool TryActivateShield()
        {
            if (_isDead) return false;
            if (IsShieldReady)
            {
                _isShieldActive = true;
                _shieldTimeRemaining = _shieldDuration;
                _shieldCooldownRemaining = _shieldCooldown;

                if (_shieldVisual != null)
                {
                    _shieldVisual.SetShieldActive(true);
                }

                if (ProceduralAudio.Instance != null)
                {
                    ProceduralAudio.Instance.PlayShieldActivate();
                }

                OnShieldStateChanged?.Invoke(true);
                return true;
            }
            return false;
        }

        private void DeactivateShield()
        {
            _isShieldActive = false;
            _shieldTimeRemaining = 0f;

            if (_shieldVisual != null)
            {
                _shieldVisual.SetShieldActive(false);
            }

            OnShieldStateChanged?.Invoke(false);
        }

        private void MovePlayer()
        {
            // Velocidad de avance vertical dictada por el GameManager
            float forwardSpeed = GameManager.Instance != null ? GameManager.Instance.CurrentForwardSpeed : 7.0f;

            // Modificador por escudo (-25%)
            if (_isShieldActive)
            {
                forwardSpeed *= _shieldSpeedFactor;
            }

            // Modificador por aceite (-50%)
            if (_oilSlowTimeRemaining > 0f)
            {
                forwardSpeed *= 0.5f;
            }

            // Movimiento lateral del jugador
            float hInput = ReadHorizontalInput();
            float lateralSpeed = _horizontalSpeed;

            if (_isShieldActive) lateralSpeed *= _shieldSpeedFactor;
            if (_oilSlowTimeRemaining > 0f) lateralSpeed *= 0.5f;

            float newX = transform.position.x + hInput * lateralSpeed * Time.fixedDeltaTime;
            newX = Mathf.Clamp(newX, -_roadHalfWidth, _roadHalfWidth);

            float newY = transform.position.y + forwardSpeed * Time.fixedDeltaTime;

            transform.position = new Vector3(newX, newY, transform.position.z);
        }

        private float ReadHorizontalInput()
        {
            float h = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            }
            if (Gamepad.current != null)
            {
                float stick = Gamepad.current.leftStick.x.ReadValue();
                if (Mathf.Abs(stick) > 0.15f) h = stick;
                if (Gamepad.current.dpad.left.isPressed) h = -1f;
                if (Gamepad.current.dpad.right.isPressed) h = 1f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Mathf.Approximately(h, 0f))
            {
                h = Input.GetAxisRaw("Horizontal");
            }
#endif

            // Botones táctiles en pantalla (< y >)
            if (Mathf.Approximately(h, 0f))
            {
                h = MobileInput.Horizontal;
            }

            return Mathf.Clamp(h, -1f, 1f);
        }

        private bool ReadShieldInput()
        {
            // Botón táctil del escudo
            if (MobileInput.ShieldHeld || MobileInput.ShieldPressedThisFrame)
                return true;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.spaceKey.isPressed))
                return true;
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                                           Gamepad.current.rightTrigger.wasPressedThisFrame ||
                                           Gamepad.current.leftTrigger.wasPressedThisFrame))
                return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
                return true;
#endif

            return false;
        }

        #region Daño y Efectos de Estado
        /// <summary>
        /// Recibe impacto de proyectil.
        /// </summary>
        /// <param name="damage">Puntos de vida a restar (1 para flecha, 999 para roca)</param>
        /// <param name="isRock">Verdadero si el proyectil es una roca (ignora escudo y mata instantáneamente)</param>
        /// <returns>True si el impacto fue letal o causó daño, False si fue bloqueado por escudo</returns>
        public bool TakeDamage(int damage, bool isRock = false)
        {
            if (_isDead) return false;

            // Las rocas aplastan instantáneamente sin importar el escudo
            if (isRock)
            {
                Die();
                return true;
            }

            // Si es una flecha y el escudo está activo: ¡Bloqueado al 100%!
            if (_isShieldActive)
            {
                if (_shieldVisual != null) _shieldVisual.TriggerDeflectEffect();
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayShieldBlock();
                if (GameManager.Instance != null) GameManager.Instance.RegisterArrowBlocked();
                return false;
            }

            // Si está en tiempo de invulnerabilidad tras recibir golpe previo
            if (_invulnerabilityTimeRemaining > 0f) return false;

            // Impacto sin escudo
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            _invulnerabilityTimeRemaining = _invulnerabilityDuration;
            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPlayerHurt();
                if (CameraController.Instance != null) CameraController.Instance.Shake(0.2f, 0.25f);
            }

            return true;
        }

        public void ApplyOilSlow(float duration = 2.0f)
        {
            if (_isDead) return;
            _oilSlowTimeRemaining = duration;
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayOilSplash();
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;
            _currentHealth = 0;
            OnHealthChanged?.Invoke(_currentHealth);

            // Desactivar escudo
            if (_isShieldActive) DeactivateShield();

            // Animación de caída / rotación del sprite
            transform.rotation = Quaternion.Euler(0, 0, 90f);
            _spriteRenderer.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayPlayerDeath();
            if (CameraController.Instance != null) CameraController.Instance.Shake(0.5f, 0.5f);

            OnPlayerDied?.Invoke();
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
        }

        private void HandleBlinkEffect()
        {
            if (_isDead) return;

            if (_invulnerabilityTimeRemaining > 0f)
            {
                float alpha = Mathf.PingPong(Time.time * 15f, 1f) > 0.5f ? 0.35f : 1f;
                _spriteRenderer.color = new Color(1f, 0.4f, 0.4f, alpha);
            }
            else if (_oilSlowTimeRemaining > 0f)
            {
                _spriteRenderer.color = new Color(0.35f, 0.3f, 0.35f, 1f); // Tinte de aceite oscuro
            }
            else
            {
                _spriteRenderer.color = Color.white;
            }
        }
        #endregion
    }
}