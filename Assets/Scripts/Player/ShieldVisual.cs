using CastleAssault.Art;
using UnityEngine;

namespace CastleAssault.Player
{
    /// <summary>
    /// Maneja el efecto visual y aura defensiva del escudo alrededor del personaje.
    /// </summary>
    public class ShieldVisual : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private float _pulseTimer;
        private bool _isActive;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (_spriteRenderer.sprite == null)
            {
                _spriteRenderer.sprite = PixelSpriteFactory.ShieldAuraSprite;
            }

            _spriteRenderer.sortingOrder = 5;
            _spriteRenderer.enabled = false;
        }

        public void SetShieldActive(bool active)
        {
            _isActive = active;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = active;
                transform.localScale = Vector3.one * 1.35f;
            }
        }

        private void Update()
        {
            if (!_isActive || _spriteRenderer == null) return;

            _pulseTimer += Time.deltaTime * 6f;
            float pulse = 1.35f + Mathf.Sin(_pulseTimer) * 0.08f;
            transform.localScale = new Vector3(pulse, pulse, 1f);

            // Rotación suave del aura protectora
            transform.Rotate(0, 0, 45f * Time.deltaTime);
        }

        public void TriggerDeflectEffect()
        {
            // Flash rápido de bloqueo exitoso
            if (_spriteRenderer != null)
            {
                transform.localScale = Vector3.one * 1.7f;
            }
        }
    }
}
