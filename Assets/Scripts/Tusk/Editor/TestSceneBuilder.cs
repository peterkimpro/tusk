using Tusk.CameraRig;
using Tusk.Player;
using Tusk.UI;
using Tusk.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tusk.EditorTools
{
    /// <summary>
    /// Programmatic test-scene builder. Runs via Tusk → Build Test Scene menu.
    /// Constructs the playable scene from scratch every time — no Inspector
    /// wiring required, no stale references possible. Explicit lesson from
    /// the previous project (Daily Heist) where hand-wiring caused cascading
    /// reference bugs.
    /// </summary>
    public static class TestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/TuskTest.unity";
        private const string MeshyPath = "Assets/Art/Meshy/";

        [MenuItem("Tusk/Build Test Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.playModeStateChanged += OnPlayModeExit;
                EditorApplication.isPlaying = false;
                return;
            }
            DoBuild();
        }

        private static void OnPlayModeExit(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            EditorApplication.playModeStateChanged -= OnPlayModeExit;
            EditorApplication.delayCall += DoBuild;
        }

        private static void DoBuild()
        {
            EnsureFolder("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            BuildLighting();
            var island   = BuildIsland();
            var (player, playerCtrl, playerStats) = BuildPlayer(island);
            var cam      = BuildCamera(player);
            BuildHud(playerCtrl, playerStats);

            // Hook camera reference into player controller AFTER cam exists
            var so = new SerializedObject(playerCtrl);
            so.FindProperty("cameraRig").objectReferenceValue = cam.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.OpenScene(ScenePath);
            EnsureSceneInBuildSettings(ScenePath);
            Debug.Log("Tusk: Test scene built at " + ScenePath);
        }

        private static void BuildLighting()
        {
            var sun = new GameObject("Sun");
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.4f;
            l.color = new Color(1.00f, 0.95f, 0.85f);
            l.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor    = new Color(0.55f, 0.65f, 0.82f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.40f, 0.42f);
            RenderSettings.ambientGroundColor  = new Color(0.20f, 0.22f, 0.18f);
        }

        private static IslandGenerator BuildIsland()
        {
            var go = new GameObject("Island");
            var gen = go.AddComponent<IslandGenerator>();

            // Wire Meshy tree + rock prefabs if they exist (skip pine — flagged for regen)
            var trees = LoadMeshyArray("tree_oak");
            var rocks = LoadMeshyArray("rock_boulder", "rock_cluster");
            if (trees.Length > 0 || rocks.Length > 0)
            {
                var so = new SerializedObject(gen);
                if (trees.Length > 0)
                {
                    var arr = so.FindProperty("treePrefabs");
                    arr.arraySize = trees.Length;
                    for (int i = 0; i < trees.Length; i++)
                        arr.GetArrayElementAtIndex(i).objectReferenceValue = trees[i];
                }
                if (rocks.Length > 0)
                {
                    var arr = so.FindProperty("rockPrefabs");
                    arr.arraySize = rocks.Length;
                    for (int i = 0; i < rocks.Length; i++)
                        arr.GetArrayElementAtIndex(i).objectReferenceValue = rocks[i];
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            gen.Generate();
            return gen;
        }

        private static GameObject[] LoadMeshyArray(params string[] names)
        {
            var list = new System.Collections.Generic.List<GameObject>();
            foreach (var n in names)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(MeshyPath + n + ".fbx");
                if (go != null) list.Add(go);
                else Debug.LogWarning($"Tusk: missing Meshy asset {MeshyPath}{n}.fbx");
            }
            return list.ToArray();
        }

        private static (GameObject playerGO, PlayerController ctrl, PlayerStats stats) BuildPlayer(IslandGenerator island)
        {
            // Place player at island center, on top of the dome
            float startY = 7f; // above dome peak; gravity will settle
            var go = new GameObject("Player");
            go.transform.position = new Vector3(0f, startY, 0f);

            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.slopeLimit = 50f;
            cc.stepOffset = 0.4f;

            var ctrl = go.AddComponent<PlayerController>();
            var stats = go.AddComponent<PlayerStats>();

            // Visual: prefer Meshy character if imported, fallback to capsule
            var meshyChar = AssetDatabase.LoadAssetAtPath<GameObject>(MeshyPath + "player_character.fbx");
            GameObject visual;
            if (meshyChar != null)
            {
                visual = (GameObject)PrefabUtility.InstantiatePrefab(meshyChar);
                visual.name = "Visual";
                visual.transform.SetParent(go.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                // Meshy default scale is variable; auto-fit to ~1.8m tall
                AutoScaleToHeight(visual, 1.8f);
            }
            else
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visual.name = "Visual";
                visual.transform.SetParent(go.transform);
                visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                visual.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);
                var col = visual.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
                TintRenderer(visual, new Color(0.85f, 0.35f, 0.20f));
            }

            return (go, ctrl, stats);
        }

        /// <summary>Scale a model so its bounding-box height equals targetHeight (meters).</summary>
        private static void AutoScaleToHeight(GameObject go, float targetHeight)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            float currentHeight = b.size.y;
            if (currentHeight < 0.01f) return;
            float scale = targetHeight / currentHeight;
            go.transform.localScale *= scale;

            // After scaling, re-measure and place bottom at parent origin
            renderers = go.GetComponentsInChildren<Renderer>();
            b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            float yOffset = go.transform.position.y - b.min.y;
            go.transform.position += new Vector3(0f, yOffset, 0f);
        }

        private static Camera BuildCamera(GameObject player)
        {
            var go = GameObject.Find("Main Camera") ?? new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.GetComponent<Camera>() ?? go.AddComponent<Camera>();
            if (go.GetComponent<AudioListener>() == null) go.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 70f;

            var rig = go.GetComponent<ThirdPersonCamera>() ?? go.AddComponent<ThirdPersonCamera>();
            var so = new SerializedObject(rig);
            so.FindProperty("target").objectReferenceValue = player.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return cam;
        }

        private static void BuildHud(PlayerController ctrl, PlayerStats stats)
        {
            var canvasGO = new GameObject("HUD");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (GameObject.Find("EventSystem") == null)
            {
                var ev = new GameObject("EventSystem");
                ev.AddComponent<UnityEngine.EventSystems.EventSystem>();
                ev.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var hpFill = AddBar(canvasGO.transform, "HPBar", new Vector2(40, -40), new Color(0.85f, 0.20f, 0.20f));
            var staminaFill = AddBar(canvasGO.transform, "StaminaBar", new Vector2(40, -84), new Color(0.30f, 0.85f, 0.40f));

            var hint = new GameObject("Hint");
            hint.transform.SetParent(canvasGO.transform, false);
            var t = hint.AddComponent<Text>();
            t.font = font;
            t.fontSize = 22;
            t.color = new Color(1f, 1f, 1f, 0.85f);
            t.alignment = TextAnchor.UpperCenter;
            t.raycastTarget = false;
            var hrt = hint.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1);
            hrt.anchorMax = new Vector2(1, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.anchoredPosition = new Vector2(0, -20);
            hrt.sizeDelta = new Vector2(0, 40);

            var hud = canvasGO.AddComponent<PlayerHud>();
            var so = new SerializedObject(hud);
            so.FindProperty("stats").objectReferenceValue = stats;
            so.FindProperty("controller").objectReferenceValue = ctrl;
            so.FindProperty("hpFill").objectReferenceValue = hpFill;
            so.FindProperty("staminaFill").objectReferenceValue = staminaFill;
            so.FindProperty("hintText").objectReferenceValue = t;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image AddBar(Transform parent, string name, Vector2 anchoredPos, Color fillColor)
        {
            var bgGO = new GameObject(name + "_BG");
            bgGO.transform.SetParent(parent, false);
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.45f);
            bgImg.raycastTarget = false;
            var brt = bgGO.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 1);
            brt.anchorMax = new Vector2(0, 1);
            brt.pivot = new Vector2(0, 1);
            brt.anchoredPosition = anchoredPos;
            brt.sizeDelta = new Vector2(320, 28);

            var fillGO = new GameObject(name + "_Fill");
            fillGO.transform.SetParent(bgGO.transform, false);
            var fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = 1f;
            fillImg.raycastTarget = false;
            var frt = fillGO.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = new Vector2(3, 3);
            frt.offsetMax = new Vector2(-3, -3);
            return fillImg;
        }

        private static void TintRenderer(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            else m.color = color;
            r.sharedMaterial = m;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            AssetDatabase.CreateFolder("Assets", path.Substring("Assets/".Length));
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes) if (s.path == scenePath) return;
            var newList = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(newList, 0);
            newList[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newList;
        }
    }
}
