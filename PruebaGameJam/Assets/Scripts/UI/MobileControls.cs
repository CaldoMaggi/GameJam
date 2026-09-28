using System;
using CastleAssault.Art;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CastleAssault.UI
{
    /// <summary>
    /// Estado de los controles táctiles. Cualquier script puede leerlo.
    /// </summary>
    public static class MobileInput
    {
        public static bool ForceShow;          // Para probar en PC
        public static bool LeftHeld;
        public static bool RightHeld;
        public static bool ShieldHeld;
        private static int _shieldPressFrame = -1;

        public static bool UseTouchUI => Application.isMobilePlatform || ForceShow;

        /// <summary>-1 izquierda, +1 derecha, 0 nada.</summary>
        public static float Horizontal => (RightHeld ? 1f : 0f) - (LeftHeld ? 1f : 0f);

        /// <summary>Equivale a "wasPressedThisFrame" de la barra espaciadora.</summary>
        public static bool ShieldPressedThisFrame => _shieldPressFrame == Time.frameCount;

        public static void MarkShieldPressed() { _shieldPressFrame = Time.frameCount; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ForceShow = false;
            LeftHeld = RightHeld = ShieldHeld = false;
            _shieldPressFrame = -1;
        }
    }

    /// <summary>
    /// Botones táctiles: izquierda, derecha y escudo.
    /// Ponlo en un GameObject vacío de la escena del juego.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class MobileControls : MonoBehaviour
    {
        [Tooltip("Muestra los botones también en PC (para probar). Desactívalo al exportar.")]
        [SerializeField] private bool _showOnDesktop = true;

        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private GameObject _canvasGo;

        private void Awake()
        {
            MobileInput.ForceShow = _showOnDesktop;
        }

        private void Start()
        {
            if (!MobileInput.UseTouchUI) return;

            EnsureEventSystem();
            BuildControls();
        }

        private void OnDestroy()
        {
            MobileInput.LeftHeld = false;
            MobileInput.RightHeld = false;
            MobileInput.ShieldHeld = false;
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildControls()
        {
            _canvasGo = new GameObject("MobileControlsCanvas");
            Canvas canvas = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            CanvasScaler scaler = _canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            _canvasGo.AddComponent<GraphicRaycaster>();

            // Flechas (abajo a la izquierda)
            CreateHoldButton("BtnLeft", "<", new Vector2(0.13f, 0.09f), new Vector2(230f, 230f),
                null, () => MobileInput.LeftHeld = true, () => MobileInput.LeftHeld = false);

            CreateHoldButton("BtnRight", ">", new Vector2(0.37f, 0.09f), new Vector2(230f, 230f),
                null, () => MobileInput.RightHeld = true, () => MobileInput.RightHeld = false);

            // Escudo (abajo a la derecha)
            CreateHoldButton("BtnShield", "", new Vector2(0.85f, 0.09f), new Vector2(260f, 260f),
                PixelSpriteFactory.ShieldIconSprite,
                () => { MobileInput.ShieldHeld = true; MobileInput.MarkShieldPressed(); },
                () => MobileInput.ShieldHeld = false);
        }

        private void CreateHoldButton(string name, string label, Vector2 anchor, Vector2 size,
            Sprite icon, Action onDown, Action onUp)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_canvasGo.transform, false);

            Image bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.28f);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;

            TouchHoldButton hold = go.AddComponent<TouchHoldButton>();
            hold.Init(bg, onDown, onUp);

            if (icon != null)
            {
                GameObject iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(go.transform, false);
                Image img = iconGo.AddComponent<Image>();
                img.sprite = icon;
                img.preserveAspect = true;
                img.raycastTarget = false;
                RectTransform irt = iconGo.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0.15f, 0.15f);
                irt.anchorMax = new Vector2(0.85f, 0.85f);
                irt.offsetMin = irt.offsetMax = Vector2.zero;
            }

            if (!string.IsNullOrEmpty(label))
            {
                GameObject txtGo = new GameObject("Label");
                txtGo.transform.SetParent(go.transform, false);
                TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
                tmp.text = label;
                tmp.fontSize = 150;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.raycastTarget = false;
                RectTransform trt = txtGo.GetComponent<RectTransform>();
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = trt.offsetMax = Vector2.zero;
            }
        }
    }

    /// <summary>Botón que avisa cuando se toca y cuando se suelta (soporta multitáctil).</summary>
    public class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private Image _bg;
        private Action _onDown;
        private Action _onUp;
        private bool _held;

        public void Init(Image bg, Action onDown, Action onUp)
        {
            _bg = bg;
            _onDown = onDown;
            _onUp = onUp;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_held) return;
            _held = true;
            _bg.color = new Color(1f, 1f, 1f, 0.55f);
            _onDown?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) { Release(); }
        public void OnPointerExit(PointerEventData eventData) { Release(); }

        private void OnDisable() { Release(); }

        private void Release()
        {
            if (!_held) return;
            _held = false;
            if (_bg != null) _bg.color = new Color(1f, 1f, 1f, 0.28f);
            _onUp?.Invoke();
        }
    }
}