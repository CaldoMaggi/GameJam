using System.Collections;
using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CastleAssault.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // Resolución de referencia VERTICAL (juego en 720x1280)
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        [Header("Referencias Opcionales (se crean por código si están vacías)")]
        [SerializeField] private PlayerController _player;

        // ── HUD ──────────────────────────────────────────────
        private Canvas _hudCanvas;
        private TextMeshProUGUI _distanceText;
        private TextMeshProUGUI _phaseText;
        private Image[] _heartImages;
        private Image _shieldCooldownBar;
        private Image _shieldCooldownBg;
        private Image _shieldIcon;
        private TextMeshProUGUI _bonusPopupText;
        private float _bonusPopupTimer;

        // ── Game Over ─────────────────────────────────────────
        private Canvas _gameOverCanvas;
        private Image _gameOverDarken;
        private TextMeshProUGUI _gameOverTitle;
        private TextMeshProUGUI _statsDistance;
        private TextMeshProUGUI _statsBlocked;
        private TextMeshProUGUI _statsDodged;
        private TextMeshProUGUI _recordLabel;
        private Button _retryButton;
        private Button _menuButton;

        // ── Overlay rojo de daño ──────────────────────────────
        private Image _damageFlash;
        private float _damageFlashTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            BuildHUD();
            BuildGameOverScreen();

            // Suscribirse a eventos
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDistanceUpdated += OnDistanceUpdated;
                GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
                GameManager.Instance.OnGameOverWithRecord += ShowGameOver;
            }

            if (_player != null)
            {
                _player.OnHealthChanged += OnHealthChanged;
                _player.OnPlayerDied += OnPlayerDied;
            }
        }

        private void Update()
        {
            UpdateShieldCooldownBar();
            UpdateBonusPopup();
            UpdateDamageFlash();
        }

        #region HUD Updates
        private void UpdateShieldCooldownBar()
        {
            if (_player == null || _shieldCooldownBar == null) return;

            if (_player.IsShieldActive)
            {
                // Mostrar cuánto tiempo queda de escudo activo
                float fill = _player.ShieldTimeRemaining / _player.ShieldDuration;
                _shieldCooldownBar.fillAmount = fill;
                _shieldCooldownBar.color = new Color(0.3f, 0.7f, 1f, 0.9f);
            }
            else if (_player.ShieldCooldownRemaining > 0f)
            {
                // Recargando
                float fill = 1f - (_player.ShieldCooldownRemaining / _player.ShieldCooldown);
                _shieldCooldownBar.fillAmount = fill;
                _shieldCooldownBar.color = new Color(0.55f, 0.55f, 0.6f, 0.8f);
            }
            else
            {
                // Listo
                _shieldCooldownBar.fillAmount = 1f;
                float glow = Mathf.PingPong(Time.time * 2.5f, 1f);
                _shieldCooldownBar.color = Color.Lerp(new Color(0.4f, 0.9f, 0.4f), Color.white, glow * 0.3f);
            }
        }

        private void UpdateBonusPopup()
        {
            if (_bonusPopupText == null) return;
            if (_bonusPopupTimer > 0f)
            {
                _bonusPopupTimer -= Time.deltaTime;
                _bonusPopupText.gameObject.SetActive(true);
                float alpha = _bonusPopupTimer / 1.5f;
                Color c = _bonusPopupText.color;
                c.a = alpha;
                _bonusPopupText.color = c;
                _bonusPopupText.transform.localPosition += Vector3.up * 40f * Time.deltaTime;
            }
            else
            {
                _bonusPopupText.gameObject.SetActive(false);
            }
        }

        private void UpdateDamageFlash()
        {
            if (_damageFlash == null) return;
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(_damageFlashTimer / 0.4f) * 0.45f;
                _damageFlash.color = new Color(0.9f, 0.05f, 0.05f, alpha);
            }
            else
            {
                _damageFlash.color = Color.clear;
            }
        }

        private void OnDistanceUpdated(float distance)
        {
            if (_distanceText != null)
                _distanceText.text = Loc.Format("distance", Mathf.RoundToInt(distance));
        }

        private void OnPhaseChanged(int phase)
        {
            if (_phaseText == null) return;
            string[] labels = { "", Loc.Get("phase1"), Loc.Get("phase2"), Loc.Get("phase3") };
            if (phase < labels.Length)
            {
                _phaseText.text = labels[phase];
                StartCoroutine(FadeOutPhaseText());
            }
        }

        private IEnumerator FadeOutPhaseText()
        {
            if (_phaseText == null) yield break;
            _phaseText.gameObject.SetActive(true);
            Color c = _phaseText.color;
            c.a = 1f;
            _phaseText.color = c;
            yield return new WaitForSeconds(2.0f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 1.5f;
                c.a = Mathf.Lerp(1f, 0f, t);
                _phaseText.color = c;
                yield return null;
            }
            _phaseText.gameObject.SetActive(false);
        }

        private void OnHealthChanged(int hp)
        {
            if (_heartImages == null) return;
            for (int i = 0; i < _heartImages.Length; i++)
            {
                if (_heartImages[i] != null)
                {
                    bool active = i < hp;
                    _heartImages[i].color = active ? Color.white : new Color(0.25f, 0.25f, 0.25f, 0.6f);
                    _heartImages[i].transform.localScale = active ? Vector3.one : Vector3.one * 0.75f;
                }
            }

            // Flash rojo al recibir daño
            _damageFlashTimer = 0.4f;
        }

        private void OnPlayerDied()
        {
            // La pantalla de Game Over se muestra en ShowGameOver
        }

        public void ShowBonusPopup(int amount)
        {
            if (_bonusPopupText == null) return;
            _bonusPopupText.text = Loc.Format("bonus", amount);
            _bonusPopupText.gameObject.SetActive(true);
            _bonusPopupText.transform.localPosition = new Vector3(0f, -60f, 0f);
            _bonusPopupTimer = 1.5f;
            Color c = _bonusPopupText.color;
            c.a = 1f;
            _bonusPopupText.color = c;
        }
        #endregion

        #region Game Over
        private void ShowGameOver(bool isNewRecord)
        {
            if (_gameOverCanvas == null) return;

            StartCoroutine(ShowGameOverDelayed(isNewRecord));
        }

        private IEnumerator ShowGameOverDelayed(bool isNewRecord)
        {
            yield return new WaitForSeconds(0.6f); // Breve pausa para el shake

            _gameOverCanvas.gameObject.SetActive(true);

            // Animar el oscurecimiento
            if (_gameOverDarken != null)
            {
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * 2.5f;
                    _gameOverDarken.color = Color.Lerp(Color.clear,
                        new Color(0.08f, 0f, 0f, 0.78f), t);
                    yield return null;
                }
            }

            // Rellenar estadísticas
            if (GameManager.Instance != null)
            {
                GameStats stats = GameManager.Instance.GetFinalStats();

                if (_statsDistance != null)
                    _statsDistance.text = Loc.Format("stat_distance", stats.Distance);
                if (_statsBlocked != null)
                    _statsBlocked.text = Loc.Format("stat_blocked", stats.ArrowsBlocked);
                if (_statsDodged != null)
                    _statsDodged.text = Loc.Format("stat_dodged", stats.ArrowsDodged);
            }

            if (_recordLabel != null)
            {
                _recordLabel.gameObject.SetActive(isNewRecord);
                if (isNewRecord && ProceduralAudio.Instance != null)
                {
                    ProceduralAudio.Instance.PlayRecordFanfare();
                }
            }
        }
        #endregion

        #region UI Builder
        private void BuildHUD()
        {
            // Canvas HUD
            GameObject hudGo = new GameObject("HUDCanvas");
            _hudCanvas = hudGo.AddComponent<Canvas>();
            _hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _hudCanvas.sortingOrder = 50;
            CanvasScaler scaler = hudGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            hudGo.AddComponent<GraphicRaycaster>();

            // Flash de daño (cubre toda la pantalla)
            _damageFlash = CreateFullscreenImage(hudGo.transform, "DamageFlash", Color.clear, 90);

            // Texto de distancia (arriba al centro)
            _distanceText = CreateTMPLabel(hudGo.transform, Loc.Format("distance", 0),
                new Vector2(0.5f, 0.97f), new Vector2(0.5f, 0.97f), new Vector2(0f, -40f), 64);
            _distanceText.color = Color.white;
            _distanceText.fontStyle = FontStyles.Bold;
            _distanceText.alignment = TextAlignmentOptions.Center;
            AddTextShadow(_distanceText.gameObject);

            // Texto de fase (centro pantalla, aparece brevemente)
            _phaseText = CreateTMPLabel(hudGo.transform, "",
                new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), Vector2.zero, 64);
            _phaseText.color = new Color(1f, 0.85f, 0.2f, 1f);
            _phaseText.fontStyle = FontStyles.Bold;
            _phaseText.alignment = TextAlignmentOptions.Center;
            AddTextShadow(_phaseText.gameObject);
            _phaseText.gameObject.SetActive(false);

            // Corazones de vida (arriba a la izquierda)
            BuildHearts(hudGo.transform);

            // Icono y barra de escudo (arriba a la derecha)
            BuildShieldBar(hudGo.transform);

            // Popup de bonus (+50m!)
            _bonusPopupText = CreateTMPLabel(hudGo.transform, Loc.Format("bonus", 50),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 76);
            _bonusPopupText.color = new Color(1f, 0.9f, 0.2f, 1f);
            _bonusPopupText.fontStyle = FontStyles.Bold;
            _bonusPopupText.alignment = TextAlignmentOptions.Center;
            AddTextShadow(_bonusPopupText.gameObject);
            _bonusPopupText.gameObject.SetActive(false);
        }

        private void BuildHearts(Transform parent)
        {
            _heartImages = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject heartGo = new GameObject($"Heart{i}");
                heartGo.transform.SetParent(parent, false);

                Image img = heartGo.AddComponent<Image>();
                img.sprite = PixelSpriteFactory.HeartSprite;
                img.preserveAspect = true;

                RectTransform rt = heartGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.sizeDelta = new Vector2(80f, 80f);
                rt.anchoredPosition = new Vector2(25f + i * 92f, -25f);

                _heartImages[i] = img;
            }
        }

        private void BuildShieldBar(Transform parent)
        {
            // Icono del escudo
            GameObject iconGo = new GameObject("ShieldIcon");
            iconGo.transform.SetParent(parent, false);
            _shieldIcon = iconGo.AddComponent<Image>();
            _shieldIcon.sprite = PixelSpriteFactory.ShieldIconSprite;
            _shieldIcon.preserveAspect = true;
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(1f, 1f);
            iconRt.anchorMax = new Vector2(1f, 1f);
            iconRt.pivot = new Vector2(1f, 1f);
            iconRt.sizeDelta = new Vector2(76f, 76f);
            iconRt.anchoredPosition = new Vector2(-120f, -24f);

            // Barra de cooldown (fondo)
            GameObject bgGo = new GameObject("ShieldBarBG");
            bgGo.transform.SetParent(parent, false);
            _shieldCooldownBg = bgGo.AddComponent<Image>();
            _shieldCooldownBg.color = new Color(0.15f, 0.15f, 0.15f, 0.7f);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(1f, 1f);
            bgRt.anchorMax = new Vector2(1f, 1f);
            bgRt.pivot = new Vector2(1f, 1f);
            bgRt.sizeDelta = new Vector2(100f, 20f);
            bgRt.anchoredPosition = new Vector2(-25f, -90f);

            // Barra de cooldown (relleno)
            GameObject fillGo = new GameObject("ShieldBarFill");
            fillGo.transform.SetParent(parent, false);
            _shieldCooldownBar = fillGo.AddComponent<Image>();
            _shieldCooldownBar.fillMethod = Image.FillMethod.Horizontal;
            _shieldCooldownBar.type = Image.Type.Filled;
            _shieldCooldownBar.fillAmount = 1f;
            _shieldCooldownBar.color = new Color(0.4f, 0.9f, 0.4f, 0.9f);
            RectTransform fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(1f, 1f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.pivot = new Vector2(1f, 1f);
            fillRt.sizeDelta = new Vector2(96f, 16f);
            fillRt.anchoredPosition = new Vector2(-27f, -92f);

            // Label de la tecla del escudo
            TextMeshProUGUI spaceLabel = CreateTMPLabel(parent, Loc.Get("shield_key"),
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-125f, -130f), 32);
            spaceLabel.color = new Color(0.75f, 0.75f, 0.75f, 0.9f);
            spaceLabel.alignment = TextAlignmentOptions.Right;
        }

        private void BuildGameOverScreen()
        {
            GameObject goCanvasGo = new GameObject("GameOverCanvas");
            _gameOverCanvas = goCanvasGo.AddComponent<Canvas>();
            _gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _gameOverCanvas.sortingOrder = 200;
            CanvasScaler goScaler = goCanvasGo.AddComponent<CanvasScaler>();
            goScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            goScaler.referenceResolution = ReferenceResolution;
            goScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            goScaler.matchWidthOrHeight = 0.5f;
            goCanvasGo.AddComponent<GraphicRaycaster>();
            _gameOverCanvas.gameObject.SetActive(false);

            // Overlay oscuro rojizo
            _gameOverDarken = CreateFullscreenImage(goCanvasGo.transform, "Darken", Color.clear, 0);

            // Panel central (más ancho para pantalla vertical)
            GameObject panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(goCanvasGo.transform, false);
            Image panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.02f, 0.02f, 0.92f);
            RectTransform panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.05f, 0.15f);
            panelRt.anchorMax = new Vector2(0.95f, 0.88f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // Título
            _gameOverTitle = CreateTMPLabel(goCanvasGo.transform, Loc.Get("gameover"),
                new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), Vector2.zero, 68);
            _gameOverTitle.rectTransform.sizeDelta = new Vector2(1000f, 200f);
            _gameOverTitle.color = new Color(1f, 0.3f, 0.3f, 1f);
            _gameOverTitle.fontStyle = FontStyles.Bold;
            _gameOverTitle.alignment = TextAlignmentOptions.Center;
            AddTextShadow(_gameOverTitle.gameObject);

            // NUEVO RÉCORD
            _recordLabel = CreateTMPLabel(goCanvasGo.transform, Loc.Get("record"),
                new Vector2(0.5f, 0.67f), new Vector2(0.5f, 0.67f), Vector2.zero, 64);
            _recordLabel.color = new Color(1f, 0.88f, 0.1f, 1f);
            _recordLabel.fontStyle = FontStyles.Bold;
            _recordLabel.alignment = TextAlignmentOptions.Center;
            AddTextShadow(_recordLabel.gameObject);
            _recordLabel.gameObject.SetActive(false);

            // Estadísticas
            _statsDistance = CreateTMPLabel(goCanvasGo.transform, Loc.Format("stat_distance", 0),
                new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), Vector2.zero, 48);
            _statsDistance.color = Color.white;
            _statsDistance.alignment = TextAlignmentOptions.Center;

            _statsBlocked = CreateTMPLabel(goCanvasGo.transform, Loc.Format("stat_blocked", 0),
                new Vector2(0.5f, 0.49f), new Vector2(0.5f, 0.49f), Vector2.zero, 46);
            _statsBlocked.color = new Color(0.7f, 0.85f, 1f, 1f);
            _statsBlocked.alignment = TextAlignmentOptions.Center;

            _statsDodged = CreateTMPLabel(goCanvasGo.transform, Loc.Format("stat_dodged", 0),
                new Vector2(0.5f, 0.42f), new Vector2(0.5f, 0.42f), Vector2.zero, 46);
            _statsDodged.color = new Color(0.7f, 1f, 0.75f, 1f);
            _statsDodged.alignment = TextAlignmentOptions.Center;

            // Botón REINTENTAR
            _retryButton = CreateButton(goCanvasGo.transform, Loc.Get("retry"),
                new Vector2(0.5f, 0.3f), new Vector2(520f, 110f), new Color(0.15f, 0.55f, 0.18f));
            _retryButton.onClick.AddListener(OnRetryClicked);

            // Botón MENÚ PRINCIPAL
            _menuButton = CreateButton(goCanvasGo.transform, Loc.Get("menu"),
                new Vector2(0.5f, 0.19f), new Vector2(520f, 100f), new Color(0.35f, 0.15f, 0.12f));
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnRetryClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }

            _gameOverCanvas.gameObject.SetActive(false);
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void OnMenuClicked()
        {
            // Carga la escena 0 (el menú principal)
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        #region UI Helpers
        private TextMeshProUGUI CreateTMPLabel(Transform parent, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, int fontSize)
        {
            GameObject go = new GameObject("TMPLabel_" + text.Substring(0, Mathf.Min(text.Length, 12)));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(1000f, 100f);

            return tmp;
        }

        private Image CreateFullscreenImage(Transform parent, string name, Color color, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Canvas c = go.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = order;
            return img;
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchorPos,
            Vector2 size, Color bgColor)
        {
            GameObject go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);

            Image bg = go.AddComponent<Image>();
            bg.color = bgColor;

            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.3f);
            btn.colors = colors;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorPos;
            rt.anchorMax = anchorPos;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            // Texto del botón
            GameObject txtGo = new GameObject("BtnText");
            txtGo.transform.SetParent(go.transform, false);
            TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 44;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            return btn;
        }

        private void AddTextShadow(GameObject go)
        {
            var shadow = go.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(3f, -3f);
        }
        #endregion

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDistanceUpdated -= OnDistanceUpdated;
                GameManager.Instance.OnPhaseChanged -= OnPhaseChanged;
                GameManager.Instance.OnGameOverWithRecord -= ShowGameOver;
            }

            if (_player != null)
            {
                _player.OnHealthChanged -= OnHealthChanged;
                _player.OnPlayerDied -= OnPlayerDied;
            }
        }
        #endregion // UI Builder
    }
}