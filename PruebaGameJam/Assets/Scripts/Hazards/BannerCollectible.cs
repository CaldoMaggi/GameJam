using System.Collections;
using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Player;
using UnityEngine;

namespace CastleAssault.Hazards
{
    /// <summary>
    /// Estandarte coleccionable: da +50 metros de distancia bonus al recogerlo.
    /// Tiene un efecto de bob (flotación) y brilla para ser llamativo.
    /// </summary>
    public class BannerCollectible : MonoBehaviour
    {
        private const float BonusDistance = 50f;
        private bool _collected = false;
        private SpriteRenderer _sr;
        private float _bobTimer;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = PixelSpriteFactory.BannerSprite;
            _sr.sortingOrder = 8;
            transform.localScale = Vector3.one * 1.0f;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
        }

        private void Update()
        {
            if (_collected) return;

            // Animación de flotación suave
            _bobTimer += Time.deltaTime;
            float bob = Mathf.Sin(_bobTimer * 3.5f) * 0.12f;
            transform.localPosition = new Vector3(0f, bob, 0f);

            // Rotación suave izquierda-derecha
            float tilt = Mathf.Sin(_bobTimer * 2f) * 5f;
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            // Brillo intermitente dorado
            float glow = Mathf.PingPong(_bobTimer * 3f, 1f);
            _sr.color = Color.Lerp(new Color(1f, 0.9f, 0.3f), Color.white, glow * 0.5f);

            // Destruir si queda muy atrás de la cámara
            if (Camera.main != null && transform.position.y < Camera.main.transform.position.y - 8f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || player.IsDead) return;

            _collected = true;

            // Otorgar bonus
            if (GameManager.Instance != null)
                GameManager.Instance.AddBonusDistance(BonusDistance);

            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayCollectible();

            // Efecto de recogida: destello blanco que escala y desaparece
            StartCoroutine(CollectEffect());
        }

        private IEnumerator CollectEffect()
        {
            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float s = 1f + t * 5f;
                transform.localScale = startScale * s;
                float a = Mathf.Lerp(1f, 0f, t / 0.35f);
                _sr.color = new Color(1f, 1f, 1f, a);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
