using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Player;
using UnityEngine;

namespace CastleAssault.Hazards
{
    /// <summary>
    /// Roca/pedrusco pesado: cae más lento pero tiene radio de impacto grande.
    /// NO puede bloquearse con el escudo. Muerte instantánea al contacto.
    /// Muestra una sombra creciente en el suelo que indica el área de caída.
    /// </summary>
    public class RockHazard : MonoBehaviour
    {
        [SerializeField] private float _speed = 5.5f;
        [SerializeField] private float _shadowGrowTime = 1.5f; // tiempo antes de llegar al jugador

        private bool _hit = false;
        private SpriteRenderer _shadowSr;
        private GameObject _shadowObj;
        private float _shadowTimer;
        private Vector3 _targetGroundPos;

        private void Awake()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSpriteFactory.RockSprite;
            sr.sortingOrder = 9;
            transform.localScale = Vector3.one * 1.6f;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;
        }

        public void Initialize(Vector3 spawnPos, Vector3 groundTarget)
        {
            transform.position = spawnPos;
            _targetGroundPos = groundTarget;

            // Calcular velocidad para llegar en _shadowGrowTime segundos
            float dist = Vector3.Distance(spawnPos, groundTarget);
            _speed = dist / _shadowGrowTime;

            CreateShadow(groundTarget);
        }

        private void CreateShadow(Vector3 pos)
        {
            _shadowObj = new GameObject("RockShadow");
            _shadowObj.transform.position = new Vector3(pos.x, pos.y, 0f);

            _shadowSr = _shadowObj.AddComponent<SpriteRenderer>();
            _shadowSr.sprite = PixelSpriteFactory.WhitePixel;
            _shadowSr.color = new Color(0f, 0f, 0f, 0f);
            _shadowSr.sortingOrder = 2;
            _shadowObj.transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (_hit) return;

            // Mover la roca hacia el objetivo
            transform.position = Vector3.MoveTowards(
                transform.position, _targetGroundPos, _speed * Time.deltaTime);

            // Rotar la roca mientras cae
            transform.Rotate(0f, 0f, -180f * Time.deltaTime);

            // Animar la sombra creciente en el suelo
            if (_shadowSr != null && _shadowObj != null)
            {
                _shadowTimer += Time.deltaTime;
                float t = Mathf.Clamp01(_shadowTimer / _shadowGrowTime);
                float shadowScale = Mathf.Lerp(0.1f, 1.4f, t);
                _shadowObj.transform.localScale = new Vector3(shadowScale, shadowScale * 0.45f, 1f);
                float alpha = Mathf.Lerp(0f, 0.55f, t);
                _shadowSr.color = new Color(0f, 0f, 0f, alpha);
            }

            // Destruir si pasa muy abajo (misó al jugador)
            if (transform.position.y < Camera.main.transform.position.y - 8f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hit) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            _hit = true;

            // Efecto visual: destello de impacto
            if (ProceduralAudio.Instance != null)
                ProceduralAudio.Instance.PlayRockSmash();

            // La roca mata instantáneamente (isRock = true ignora escudo)
            player.TakeDamage(999, isRock: true);

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_shadowObj != null)
                Destroy(_shadowObj);
        }
    }
}
