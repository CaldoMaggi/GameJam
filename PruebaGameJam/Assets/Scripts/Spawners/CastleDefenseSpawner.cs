using System.Collections;
using CastleAssault.Core;
using CastleAssault.Hazards;
using CastleAssault.Player;
using UnityEngine;

namespace CastleAssault.Spawners
{
    /// <summary>
    /// Controlador de oleadas de peligros según la fase de distancia.
    /// Fase 1 (0-100m): Flechas simples
    /// Fase 2 (100-300m): Flechas en parejas + Rocas
    /// Fase 3 (300m+): Lluvia de flechas + Rocas + Charcos de aceite + Estandartes
    /// </summary>
    public class CastleDefenseSpawner : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private Camera _mainCamera;

        [Header("Cooldowns por Fase")]
        [SerializeField] private float _phase1ArrowCooldown = 2.5f;
        [SerializeField] private float _phase2ArrowCooldown = 1.6f;
        [SerializeField] private float _phase3ArrowCooldown = 0.7f;
        [SerializeField] private float _rockCooldown = 4.0f;
        [SerializeField] private float _oilCooldown = 5.5f;
        [SerializeField] private float _bannerCooldown = 8.0f;

        [Header("Rango horizontal de spawn (eje X)")]
        [SerializeField] private float _spawnXRange = 2.2f;

        private float _arrowTimer;
        private float _rockTimer;
        private float _oilTimer;
        private float _bannerTimer;

        private void Start()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            // Escalonear inicialmente los cooldowns para no tener todo al mismo tiempo
            _arrowTimer = 1.5f;
            _rockTimer = 5.0f;
            _oilTimer = 7.0f;
            _bannerTimer = 3.0f;
        }

        private void Update()
        {
            if (_player == null || _player.IsDead) return;
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            int phase = GameManager.Instance.Phase;
            float dt = Time.deltaTime;

            _arrowTimer -= dt;
            _rockTimer -= dt;
            _oilTimer -= dt;
            _bannerTimer -= dt;

            float arrowCooldown = GetArrowCooldown(phase);

            // ── Flechas ──────────────────────────────────────────────────
            if (_arrowTimer <= 0f)
            {
                _arrowTimer = arrowCooldown;
                SpawnArrow();

                if (phase >= 2)
                {
                    // Flechas en par en fase 2+
                    StartCoroutine(SpawnArrowDelayed(0.35f));
                }

                if (phase >= 3 && Random.value < 0.4f)
                {
                    // Ráfaga de 3 flechas en fase 3
                    StartCoroutine(SpawnArrowDelayed(0.22f));
                    StartCoroutine(SpawnArrowDelayed(0.44f));
                }
            }

            // ── Rocas ─────────────────────────────────────────────────────
            if (phase >= 2 && _rockTimer <= 0f)
            {
                _rockTimer = _rockCooldown;
                // Ajustar cooldown con la velocidad de la fase 3
                if (phase == 3) _rockTimer *= 0.65f;
                SpawnRock();
            }

            // ── Aceite ────────────────────────────────────────────────────
            if (phase >= 3 && _oilTimer <= 0f)
            {
                _oilTimer = _oilCooldown;
                SpawnOil();
            }

            // ── Estandartes Coleccionables ─────────────────────────────────
            if (phase >= 1 && _bannerTimer <= 0f)
            {
                _bannerTimer = _bannerCooldown;
                SpawnBanner();
            }
        }

        private float GetArrowCooldown(int phase)
        {
            switch (phase)
            {
                case 1: return _phase1ArrowCooldown;
                case 2: return _phase2ArrowCooldown;
                default: return _phase3ArrowCooldown;
            }
        }

        #region Spawn Helpers
        private Vector3 GetRandomXInLane()
        {
            float x = Random.Range(-_spawnXRange, _spawnXRange);
            // Snapping opcional a 3 carriles
            // float[] lanes = { -1.5f, 0f, 1.5f };
            // float x = lanes[Random.Range(0, lanes.Length)];
            return new Vector3(x, 0f, 0f);
        }

        private void SpawnArrow()
        {
            if (_player == null) return;

            // Posición en el suelo donde va a caer la flecha
            Vector3 xOffset = GetRandomXInLane();
            Vector3 groundPos = new Vector3(
                _player.transform.position.x + xOffset.x,
                _player.transform.position.y + Random.Range(2f, 5f),
                0f);

            GameObject arrowGo = new GameObject("Arrow");
            arrowGo.transform.position = groundPos + Vector3.up * 8f;
            ArrowHazard arrow = arrowGo.AddComponent<ArrowHazard>();
            CircleCollider2D col = arrowGo.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.18f;
            arrow.Initialize(groundPos);
        }

        private IEnumerator SpawnArrowDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnArrow();
        }

        private void SpawnRock()
        {
            if (_player == null) return;

            Vector3 xOffset = GetRandomXInLane();
            Vector3 groundPos = new Vector3(
                _player.transform.position.x + xOffset.x,
                _player.transform.position.y + Random.Range(3f, 6f),
                0f);
            Vector3 spawnPos = new Vector3(groundPos.x, groundPos.y + 12f, 0f);

            GameObject rockGo = new GameObject("Rock");
            rockGo.AddComponent<CircleCollider2D>().isTrigger = true;
            RockHazard rock = rockGo.AddComponent<RockHazard>();
            rock.Initialize(spawnPos, groundPos);
        }

        private void SpawnOil()
        {
            if (_player == null) return;

            Vector3 xOffset = GetRandomXInLane();
            Vector3 oilPos = new Vector3(
                _player.transform.position.x + xOffset.x,
                _player.transform.position.y + Random.Range(3f, 7f),
                0f);

            GameObject oilGo = new GameObject("OilPuddle");
            oilGo.AddComponent<BoxCollider2D>().isTrigger = true;
            OilHazard oil = oilGo.AddComponent<OilHazard>();
            oil.Initialize(oilPos);
        }

        private void SpawnBanner()
        {
            if (_player == null) return;

            Vector3 xOffset = GetRandomXInLane();
            Vector3 bannerPos = new Vector3(
                _player.transform.position.x + xOffset.x,
                _player.transform.position.y + Random.Range(4f, 8f),
                0f);

            GameObject bannerGo = new GameObject("Banner");
            BannerCollectible banner = bannerGo.AddComponent<BannerCollectible>();
            bannerGo.transform.position = bannerPos;
        }
        #endregion
    }
}
