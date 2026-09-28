using System.Collections;
using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Hazards;
using CastleAssault.Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CastleAssault.Core
{
    /// <summary>
    /// Gestiona el tutorial de introducción:
    /// 1. Juego pausado con textos de instrucción flotantes
    /// 2. Roca de prueba cae muy despacio
    /// 3. Al esquivar o presionar [Espacio], inicia el juego
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private Canvas _tutorialCanvas;

        private GameObject _panel;
        private TextMeshProUGUI _line1;
        private TextMeshProUGUI _line2;
        private TextMeshProUGUI _startMsg;
        private GameObject _tutorialRock;

        private bool _tutorialComplete = false;
        private float _rockSpawnTimer = 1.5f; // Segundos antes de que caiga la roca de prueba

        private void Awake()
        {
            BuildTutorialUI();
        }

        private void Start()
        {
            if (_tutorialCanvas != null) _tutorialCanvas.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_tutorialComplete) return;

            _rockSpawnTimer -= Time.deltaTime;
            if (_rockSpawnTimer <= 0f && _tutorialRock == null)
            {
                SpawnTutorialRock();
            }

            // Detectar si el jugador se movió o presionó espacio
            bool playerMoved = false;
            bool spacePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kbd = UnityEngine.InputSystem.Keyboard.current;
                playerMoved = kbd.aKey.isPressed || kbd.dKey.isPressed ||
                              kbd.leftArrowKey.isPressed || kbd.rightArrowKey.isPressed;
                spacePressed = kbd.spaceKey.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            playerMoved |= Input.GetAxisRaw("Horizontal") != 0f;
            spacePressed |= Input.GetKeyDown(KeyCode.Space);
#endif

            if (playerMoved || spacePressed)
            {
                CompleteTutorial();
            }
        }

        private void SpawnTutorialRock()
        {
            if (_player == null) return;

            Vector3 rockPos = _player.transform.position + new Vector3(0f, 5f, 0f);

            _tutorialRock = new GameObject("TutorialRock");
            _tutorialRock.transform.position = rockPos;

            SpriteRenderer sr = _tutorialRock.AddComponent<SpriteRenderer>();
            sr.sprite = PixelSpriteFactory.RockSprite;
            sr.sortingOrder = 8;
            sr.transform.localScale = Vector3.one * 1.2f;

            CircleCollider2D col = _tutorialRock.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            // La roca de tutorial cae muy despacio sin matar
            TutorialRockDrifter drifter = _tutorialRock.AddComponent<TutorialRockDrifter>();
            drifter.Speed = 1.2f; // Muy lenta para dar tiempo de esquivar
        }

        private void CompleteTutorial()
        {
            if (_tutorialComplete) return;
            _tutorialComplete = true;

            if (_tutorialRock != null)
                Destroy(_tutorialRock);

            // Mensaje de arranque
            if (_line1 != null) _line1.gameObject.SetActive(false);
            if (_line2 != null) _line2.gameObject.SetActive(false);

            if (_startMsg != null)
            {
                _startMsg.gameObject.SetActive(true);
                _startMsg.text = "¡Llega lo más lejos posible!";
            }

            StartCoroutine(DelayedGameStart());
        }

        private IEnumerator DelayedGameStart()
        {
            yield return new WaitForSeconds(1.5f);

            if (_panel != null) _panel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void BuildTutorialUI()
        {
            if (_tutorialCanvas == null)
            {
                GameObject canvasGo = new GameObject("TutorialCanvas");
                _tutorialCanvas = canvasGo.AddComponent<Canvas>();
                _tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _tutorialCanvas.sortingOrder = 100;
                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Panel semitransparente de fondo
            _panel = new GameObject("TutorialPanel");
            _panel.transform.SetParent(_tutorialCanvas.transform, false);
            Image bg = _panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            RectTransform panelRect = _panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0.3f);
            panelRect.anchorMax = new Vector2(1f, 0.75f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            _line1 = CreateText(_panel.transform, "Usa [A][D] o [Flechas] para moverte a los lados",
                new Vector2(0.5f, 0.7f), 42);
            _line2 = CreateText(_panel.transform, "Usa [Espacio] para cubrirte con el escudo / esquivar",
                new Vector2(0.5f, 0.45f), 38);

            _startMsg = CreateText(_panel.transform, "",
                new Vector2(0.5f, 0.5f), 52);
            _startMsg.color = new Color(1f, 0.95f, 0.3f);
            _startMsg.fontStyle = FontStyles.Bold;
            _startMsg.gameObject.SetActive(false);
        }

        private TextMeshProUGUI CreateText(Transform parent, string text, Vector2 anchorPos, int fontSize)
        {
            GameObject go = new GameObject("TutorialText");
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            // Sombra para legibilidad
            var shadow = go.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.8f);
            shadow.effectDistance = new Vector2(2, -2);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, anchorPos.y - 0.15f);
            rt.anchorMax = new Vector2(0.95f, anchorPos.y + 0.15f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return tmp;
        }
    }

    /// <summary>Hace caer la roca de tutorial muy lentamente</summary>
    public class TutorialRockDrifter : MonoBehaviour
    {
        public float Speed = 1.2f;
        private void Update()
        {
            transform.Translate(Vector3.down * Speed * Time.deltaTime);
        }
    }
}
