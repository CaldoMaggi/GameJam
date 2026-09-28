using System;
using CastleAssault.Player;
using UnityEngine;

namespace CastleAssault.Core
{
    /// <summary>
    /// Singleton central del juego. Controla:
    /// - Fases de dificultad (Fase 1 / 2 / 3) basadas en metros recorridos
    /// - Velocidad de avance progresiva
    /// - Conteo de metros (distancia)
    /// - Estadísticas (flechas bloqueadas, esquivadas)
    /// - Récord persistente en PlayerPrefs
    /// - Flujo: Tutorial → Juego → Game Over
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // ── Velocidades ──
        private const float BaseForwardSpeed = 5.0f;
        private const float Phase2SpeedMultiplier = 1.15f;      // +15% en fase 2
        private const float Phase3SpeedIncrement = 0.0025f;     // Aceleración progresiva en fase 3

        // ── Fases ──
        private const float Phase2StartDistance = 100f;
        private const float Phase3StartDistance = 300f;

        // ── PlayerPrefs Keys ──
        private const string PrefBestScore = "BestScore";

        // ── Estado ──
        private GameState _state = GameState.Tutorial;
        private float _distanceRaw = 0f;
        private float _bonusDistance = 0f;
        private float _forwardSpeed = 0f;
        private int _arrowsBlocked = 0;
        private int _arrowsDodged = 0;

        public float CurrentForwardSpeed => _forwardSpeed;
        public float TotalDistance => _distanceRaw + _bonusDistance;
        public int ArrowsBlocked => _arrowsBlocked;
        public int ArrowsDodged => _arrowsDodged;
        public GameState CurrentState => _state;
        public int BestScore => PlayerPrefs.GetInt(PrefBestScore, 0);
        public int Phase => GetCurrentPhase();

        // Eventos
        public event Action<GameState> OnStateChanged;
        public event Action<int> OnPhaseChanged;
        public event Action<float> OnDistanceUpdated;
        public event Action OnGameOver;
        public event Action<bool> OnGameOverWithRecord;

        private int _lastPhase = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (_state != GameState.Playing) return;

            UpdateDistanceAndSpeed();

            int currentPhase = GetCurrentPhase();
            if (currentPhase != _lastPhase)
            {
                _lastPhase = currentPhase;
                OnPhaseChanged?.Invoke(currentPhase);
            }

            OnDistanceUpdated?.Invoke(TotalDistance);
        }

        private void UpdateDistanceAndSpeed()
        {
            int phase = GetCurrentPhase();

            switch (phase)
            {
                case 1:
                    _forwardSpeed = BaseForwardSpeed;
                    break;
                case 2:
                    _forwardSpeed = BaseForwardSpeed * Phase2SpeedMultiplier;
                    break;
                case 3:
                    _forwardSpeed += Phase3SpeedIncrement * Time.deltaTime * 60f;
                    _forwardSpeed = Mathf.Min(_forwardSpeed, BaseForwardSpeed * 2.5f); // Cap de seguridad
                    break;
            }

            // Distancia = velocidad de avance en metros por segundo
            _distanceRaw += _forwardSpeed * Time.deltaTime;
        }

        private int GetCurrentPhase()
        {
            float dist = TotalDistance;
            if (dist >= Phase3StartDistance) return 3;
            if (dist >= Phase2StartDistance) return 2;
            return 1;
        }

        public void StartGame()
        {
            _state = GameState.Playing;
            _forwardSpeed = BaseForwardSpeed;
            _lastPhase = 1;
            OnStateChanged?.Invoke(_state);
            OnPhaseChanged?.Invoke(1);
        }

        public void OnPlayerDied()
        {
            if (_state == GameState.GameOver) return;
            _state = GameState.GameOver;

            int finalScore = Mathf.RoundToInt(TotalDistance);
            int best = BestScore;
            bool isNewRecord = finalScore > best;

            if (isNewRecord)
            {
                PlayerPrefs.SetInt(PrefBestScore, finalScore);
                PlayerPrefs.Save();
            }

            OnStateChanged?.Invoke(_state);
            OnGameOver?.Invoke();
            OnGameOverWithRecord?.Invoke(isNewRecord);
        }

        public void AddBonusDistance(float bonus)
        {
            _bonusDistance += bonus;
        }

        public void RegisterArrowBlocked()
        {
            _arrowsBlocked++;
        }

        public void RegisterArrowDodged()
        {
            _arrowsDodged++;
        }

        public void RestartGame()
        {
            _distanceRaw = 0f;
            _bonusDistance = 0f;
            _forwardSpeed = 0f;
            _arrowsBlocked = 0;
            _arrowsDodged = 0;
            _lastPhase = 0;
            _state = GameState.Tutorial;
        }

        public GameStats GetFinalStats()
        {
            return new GameStats
            {
                Distance = Mathf.RoundToInt(TotalDistance),
                ArrowsBlocked = _arrowsBlocked,
                ArrowsDodged = _arrowsDodged,
                IsNewRecord = Mathf.RoundToInt(TotalDistance) > PlayerPrefs.GetInt(PrefBestScore, 0),
                BestScore = BestScore
            };
        }
    }

    public enum GameState
    {
        Tutorial,
        Playing,
        GameOver
    }

    [Serializable]
    public struct GameStats
    {
        public int Distance;
        public int ArrowsBlocked;
        public int ArrowsDodged;
        public bool IsNewRecord;
        public int BestScore;
    }
}
