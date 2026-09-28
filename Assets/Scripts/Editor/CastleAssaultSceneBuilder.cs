#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using CastleAssault.Art;
using CastleAssault.Audio;
using CastleAssault.Core;
using CastleAssault.Environment;
using CastleAssault.Player;
using CastleAssault.Spawners;
using CastleAssault.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace CastleAssault.Editor
{
    /// <summary>
    /// Constructor automático de la escena "SampleScene" desde el Editor Unity.
    /// Menu: CastleAssault > Build Game Scene
    /// Configura todos los GameObjects, componentes, referencias y guarda la escena lista para jugar.
    /// </summary>
    public class CastleAssaultSceneBuilder : UnityEditor.Editor
    {
        [MenuItem("CastleAssault/Build Game Scene")]
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog("Castle Assault - Build Scene",
                "¿Construir la escena completa del juego? Esto limpiará los GameObjects existentes.",
                "Sí, construir", "Cancelar"))
            {
                return;
            }

            ClearExistingGameObjects();
            BuildFullScene();
            EditorUtility.DisplayDialog("Castle Assault", "✅ Escena construida exitosamente.\n\nPresiona Play para jugar.", "OK");
        }

        private static void ClearExistingGameObjects()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                // Mantener la cámara y luz global de la plantilla
                if (root.name == "Main Camera" || root.name == "Global Light 2D")
                    continue;
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildFullScene()
        {
            // ── 1. Cámara ───────────────────────────────────────────────────────
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 5.5f;
            mainCam.backgroundColor = new Color(0.12f, 0.12f, 0.15f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.gameObject.transform.position = new Vector3(0f, 0f, -10f);

            CameraController camCtrl = mainCam.gameObject.GetComponent<CameraController>();
            if (camCtrl == null) camCtrl = mainCam.gameObject.AddComponent<CameraController>();

            // ── 2. Audio Manager ────────────────────────────────────────────────
            GameObject audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioSource>();
            audioGo.AddComponent<ProceduralAudio>();

            // ── 3. Game Manager ─────────────────────────────────────────────────
            GameObject gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            // ── 4. Escenario infinito ────────────────────────────────────────────
            GameObject roadGo = new GameObject("RoadScroller");
            InfiniteRoadScroller scroller = roadGo.AddComponent<InfiniteRoadScroller>();

            // ── 5. Jugador ───────────────────────────────────────────────────────
            GameObject playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, -2.5f, 0f);

            SpriteRenderer playerSr = playerGo.AddComponent<SpriteRenderer>();
            playerSr.sprite = PixelSpriteFactory.KnightSprite;
            playerSr.sortingOrder = 10;

            Rigidbody2D rb = playerGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            BoxCollider2D playerCol = playerGo.AddComponent<BoxCollider2D>();
            playerCol.isTrigger = true;
            playerCol.size = new Vector2(0.8f, 1.2f);

            PlayerController playerCtrl = playerGo.AddComponent<PlayerController>();

            // Escudo visual (hijo del jugador)
            GameObject shieldGo = new GameObject("ShieldVisual");
            shieldGo.transform.SetParent(playerGo.transform, false);
            ShieldVisual shieldVisual = shieldGo.AddComponent<ShieldVisual>();

            // Enlazar campo privado ShieldVisual usando SerializedObject
            SerializedObject soPlayer = new SerializedObject(playerCtrl);
            soPlayer.FindProperty("_shieldVisual").objectReferenceValue = shieldVisual;
            soPlayer.ApplyModifiedProperties();

            // ── 6. Cámara sigue al jugador ──────────────────────────────────────
            SerializedObject soCam = new SerializedObject(camCtrl);
            soCam.FindProperty("_target").objectReferenceValue = playerGo.transform;
            soCam.ApplyModifiedProperties();

            // ── 7. Spawner de peligros ──────────────────────────────────────────
            GameObject spawnerGo = new GameObject("CastleDefenseSpawner");
            CastleDefenseSpawner spawner = spawnerGo.AddComponent<CastleDefenseSpawner>();
            SerializedObject soSpawner = new SerializedObject(spawner);
            soSpawner.FindProperty("_player").objectReferenceValue = playerCtrl;
            soSpawner.FindProperty("_mainCamera").objectReferenceValue = mainCam;
            soSpawner.ApplyModifiedProperties();

            // ── 8. Tutorial Manager ─────────────────────────────────────────────
            GameObject tutGo = new GameObject("TutorialManager");
            TutorialManager tutMgr = tutGo.AddComponent<TutorialManager>();
            SerializedObject soTut = new SerializedObject(tutMgr);
            soTut.FindProperty("_player").objectReferenceValue = playerCtrl;
            soTut.ApplyModifiedProperties();

            // ── 9. UI Manager ───────────────────────────────────────────────────
            GameObject uiMgrGo = new GameObject("UIManager");
            UIManager uiMgr = uiMgrGo.AddComponent<UIManager>();
            SerializedObject soUI = new SerializedObject(uiMgr);
            soUI.FindProperty("_player").objectReferenceValue = playerCtrl;
            soUI.ApplyModifiedProperties();

            // ── 10. EventSystem (necesario para botones UI) ─────────────────────
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            // ── 11. Luz 2D global (URP) ─────────────────────────────────────────
            if (Object.FindObjectOfType<Light2D>() == null)
            {
                GameObject lightGo = new GameObject("GlobalLight2D");
                Light2D globalLight = lightGo.AddComponent<Light2D>();
                globalLight.lightType = Light2D.LightType.Global;
                globalLight.intensity = 1f;
                globalLight.color = new Color(0.95f, 0.9f, 0.85f);
            }

            // ── 12. Guardar escena ──────────────────────────────────────────────
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
    }
}
#endif
