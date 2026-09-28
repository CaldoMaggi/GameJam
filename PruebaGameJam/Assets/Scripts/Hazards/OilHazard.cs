using CastleAssault.Art;
using CastleAssault.Player;
using UnityEngine;

namespace CastleAssault.Hazards
{
    /// <summary>
    /// Charco de aceite hirviendo: se coloca en el suelo durante un tiempo determinado.
    /// Al pisarlo, ralentiza al jugador un 50% durante 2 segundos.
    /// El charco pulsa/vibra visualmente con burbujas animadas.
    /// </summary>
    public class OilHazard : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 5.0f;
        [SerializeField] private float _slowDuration = 2.0f;

        private float _timer;
        private SpriteRenderer _sr;
        private bool _active = true;
        private float _pulseTimer;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = PixelSpriteFactory.OilPuddleSprite;
            _sr.sortingOrder = 2;
            transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.4f, 0.7f);
        }

        public void Initialize(Vector3 position)
        {
            transform.position = position;
        }

        private void Update()
        {
            if (!_active) return;

            _timer += Time.deltaTime;
            _pulseTimer += Time.deltaTime;

            // Pulso visual: el charco "bulle"
            float pulse = 1f + Mathf.Sin(_pulseTimer * 4f) * 0.06f;
            transform.localScale = new Vector3(1.8f * pulse, 1.8f, 1f);

            // Fundido final antes de desaparecer
            if (_timer > _lifetime * 0.75f)
            {
                float alpha = Mathf.Lerp(1f, 0f, (_timer - _lifetime * 0.75f) / (_lifetime * 0.25f));
                Color c = _sr.color;
                c.a = alpha;
                _sr.color = c;
            }

            if (_timer >= _lifetime)
            {
                _active = false;
                Destroy(gameObject);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_active) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            // Aplicar ralentización continuamente mientras esté dentro
            player.ApplyOilSlow(_slowDuration);
        }
    }
}
