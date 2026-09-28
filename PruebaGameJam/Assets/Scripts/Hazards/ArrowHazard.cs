using System.Collections;
using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Player;
using UnityEngine;

namespace CastleAssault.Hazards
{
    /// <summary>
    /// Proyectil de flecha: cae en línea recta hacia abajo.
    /// Muestra retícula roja en el suelo 0.5s antes del impacto.
    /// Al contactar al jugador: 1 HP de daño (o bloqueada por escudo).
    /// </summary>
    public class ArrowHazard : MonoBehaviour
    {
        [SerializeField] private float _speed = 14f;
        [SerializeField] private float _warningDuration = 0.5f;

        private bool _launched = false;
        private bool _hit = false;
        private SpriteRenderer _sr;
        private GameObject _reticleObj;

        private const float ArrowDespawnY = -5f;  // destruir si pasa muy abajo

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = PixelSpriteFactory.ArrowSprite;
            _sr.sortingOrder = 9;
            _sr.enabled = false; // Invisible hasta que sea lanzada
        }

        /// <summary>
        /// Inicializa la flecha: primero muestra retícula en el suelo _warningDuration segundos,
        /// luego lanza el proyectil desde la parte superior de la cámara.
        /// </summary>
        public void Initialize(Vector3 groundTargetPos, float customSpeed = 0f)
        {
            if (customSpeed > 0f) _speed = customSpeed;

            // 1. Crear retícula en el suelo
            _reticleObj = new GameObject("ArrowReticle");
            _reticleObj.transform.SetParent(null);
            _reticleObj.transform.position = new Vector3(groundTargetPos.x, groundTargetPos.y, 0f);

            SpriteRenderer reticleSr = _reticleObj.AddComponent<SpriteRenderer>();
            reticleSr.sprite = PixelSpriteFactory.ReticleSprite;
            reticleSr.sortingOrder = 3;
            reticleSr.transform.localScale = Vector3.one * 0.9f;

            // Parpadeo de la retícula
            StartCoroutine(BlinkReticle(reticleSr));

            // 2. Después de _warningDuration, posicionar y lanzar flecha
            StartCoroutine(LaunchAfterWarning(groundTargetPos));
        }

        private IEnumerator BlinkReticle(SpriteRenderer reticleSr)
        {
            float timer = 0f;
            while (timer < _warningDuration && reticleSr != null)
            {
                timer += Time.deltaTime;
                float t = Mathf.PingPong(timer * 8f, 1f);
                Color c = reticleSr.color;
                c.a = Mathf.Lerp(0.35f, 1f, t);
                reticleSr.color = c;
                yield return null;
            }
        }

        private IEnumerator LaunchAfterWarning(Vector3 groundTargetPos)
        {
            yield return new WaitForSeconds(_warningDuration);

            // Destruir retícula
            if (_reticleObj != null)
            {
                Destroy(_reticleObj);
                _reticleObj = null;
            }

            // Salir si el juego terminó mientras esperábamos
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
            {
                Destroy(gameObject);
                yield break;
            }

            // Posicionar la flecha encima del punto de impacto
            transform.position = new Vector3(groundTargetPos.x,
                groundTargetPos.y + 10f, groundTargetPos.z);
            _sr.enabled = true;
            _launched = true;

            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayArrowWhoosh();
        }

        private void Update()
        {
            if (!_launched || _hit) return;

            transform.Translate(Vector3.down * _speed * Time.deltaTime);

            // Limpiar si sale por abajo de la cámara
            if (transform.position.y < Camera.main.transform.position.y - 8f)
            {
                // La flecha pasó sin dar: registrar esquive
                if (GameManager.Instance != null) GameManager.Instance.RegisterArrowDodged();
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hit || !_launched) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            _hit = true;
            bool wasBlocked = player.TakeDamage(1, isRock: false);

            if (!wasBlocked && ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayArrowImpact();

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_reticleObj != null)
                Destroy(_reticleObj);
        }
    }
}
