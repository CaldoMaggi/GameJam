using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CastleAssault.Audio
{
    /// <summary>
    /// Música de fondo: una canción para el menú y otra para el juego.
    /// Cambia sola según la escena, con fundido entre canciones.
    /// Archivos: Assets/Resources/Audio/MenuMusic y Assets/Resources/Audio/GameMusic
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        private static MusicPlayer _instance;

        [Header("Nombres de escena")]
        [SerializeField] private string _menuSceneName = "MainMenu";

        [Header("Volumen")]
        [SerializeField, Range(0f, 1f)] private float _menuVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _gameVolume = 0.4f;
        [SerializeField] private float _fadeTime = 0.6f;

        private AudioSource _source;
        private AudioClip _menuClip;
        private AudioClip _gameClip;
        private AudioClip _currentClip;
        private Coroutine _switchRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        // Se crea solo si no hay uno puesto en la escena
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            if (_instance != null) return;
            new GameObject("MusicPlayer").AddComponent<MusicPlayer>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.volume = 0f;

            _menuClip = Resources.Load<AudioClip>("Audio/MenuMusic");
            _gameClip = Resources.Load<AudioClip>("Audio/GameMusic");

            if (_menuClip == null)
                Debug.LogWarning("MusicPlayer: no encontré Resources/Audio/MenuMusic");
            if (_gameClip == null)
                Debug.LogWarning("MusicPlayer: no encontré Resources/Audio/GameMusic");

            SceneManager.sceneLoaded += OnSceneLoaded;
            PlayForScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (_instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayForScene(scene);
        }

        private void PlayForScene(Scene scene)
        {
            bool isMenu = scene.name == _menuSceneName;
            AudioClip target = isMenu ? _menuClip : _gameClip;
            float volume = isMenu ? _menuVolume : _gameVolume;

            // Si ya suena esa canción (por ejemplo al reintentar), no se reinicia
            if (target == _currentClip && _source.isPlaying)
            {
                _source.volume = volume;
                return;
            }

            if (_switchRoutine != null) StopCoroutine(_switchRoutine);
            _switchRoutine = StartCoroutine(SwitchTo(target, volume));
        }

        private IEnumerator SwitchTo(AudioClip clip, float volume)
        {
            // Baja el volumen de la canción actual
            float startVol = _source.volume;
            float t = 0f;
            while (t < _fadeTime && _source.isPlaying)
            {
                t += Time.unscaledDeltaTime;
                _source.volume = Mathf.Lerp(startVol, 0f, t / _fadeTime);
                yield return null;
            }

            _source.Stop();
            _currentClip = clip;
            _source.clip = clip;

            if (clip == null) yield break;

            _source.volume = 0f;
            _source.Play();

            // Sube el volumen de la nueva
            t = 0f;
            while (t < _fadeTime)
            {
                t += Time.unscaledDeltaTime;
                _source.volume = Mathf.Lerp(0f, volume, t / _fadeTime);
                yield return null;
            }
            _source.volume = volume;
        }
    }
}