using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace PrettyTanks.Editor {
    [InitializeOnLoad]
    public static class ProjectSetup {
        static ProjectSetup() {
            EditorApplication.delayCall += Auto;
        }

        private static void Auto()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Auto;
                return;
            }

            if (!File.Exists("Assets/Settings/PrettyURP.asset"))
            {
                Configure();
            }

            const string mainScenePath = "Assets/Scenes/Main.unity";

            var mainScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(mainScenePath);

            if (!mainScene)
            {
                Debug.LogError("Не найдена стартовая сцена: " + mainScenePath);
                return;
            }

            EditorSceneManager.playModeStartScene = mainScene;

            const string sessionKey = "PrettyTanks.MainSceneChecked";

            if (SessionState.GetBool(sessionKey, false))
            {
                return;
            }

            SessionState.SetBool(sessionKey, true);
            SetGameViewAspect();

            var currentScene = SceneManager.GetActiveScene();

            if (SceneManager.sceneCount == 1 &&
                string.IsNullOrEmpty(currentScene.path) &&
                !currentScene.isDirty)
            {
                EditorSceneManager.OpenScene(
                    mainScenePath,
                    OpenSceneMode.Single);
            }
        }

        [MenuItem("Pretty Tanks/1. Configure project")]
        public static void Configure() {
            Directory.CreateDirectory("Assets/Settings");
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PrettyURP.asset");
            if (!pipeline) {
                var renderer = ScriptableObject.CreateInstance<Renderer2DData>();
                AssetDatabase.CreateAsset(renderer, "Assets/Settings/PrettyRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, "Assets/Settings/PrettyURP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            if (!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/ConfettiMaterial.mat")) {
                var mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
                AssetDatabase.CreateAsset(mat, "Assets/Resources/ConfettiMaterial.mat");
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Art" })) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != 128) {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 128;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
            }
            PlayerSettings.companyName = "Pikmi";
            PlayerSettings.productName = "Pretty Tanks";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler");
            if (input != null) {
                input.intValue = 1;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Pretty Tanks: настройка завершена. Откройте Assets/Scenes/Main.unity и нажмите Play.");
        }

        [MenuItem("Pretty Tanks/2. Open game scene")]
        public static void Open() {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
            }
        }

        [MenuItem("Pretty Tanks/3. Validate assets")]
        public static void Validate() {
            string[] names = { "PlayerTank",    "EnemyTank",    "Modules",        "Cannon",       "Difficulty",
                               "Enemy_Scooter", "Enemy_Sniper", "Enemy_Ricochet", "Enemy_Mother", "Enemy_Ghost",
                               "UIAppearance", "Truck", "PowerUps", "ShieldVisual", "ShieldBubble", "Music" };
            foreach (var name in names)
                if (!Resources.Load<Object>(name)) {
                    throw new System.Exception("Не найден ресурс: " + name);
                }
            foreach (var name in new[] { "hull",  "track", "engine", "barrel", "bow",  "ears",  "van",
                                         "cat",   "brick", "steel",  "wood",   "ice",  "floor", "debris",
                                         "heart", "star",  "round",  "panel",  "gift", "home",
                                         "menu-hero", "route-previews", "blossom-tree" })
                if (!Art.Sprite(name)) {
                    throw new System.Exception("Не найден спрайт: " + name);
                }
            if (!GraphicsSettings.defaultRenderPipeline) {
                throw new System.Exception("URP не настроен");
            }
            Debug.Log("Pretty Tanks: ресурсы и URP найдены. Запуск и механику проверьте по Docs/TEST_PLAN_RU.md.");
        }

        [MenuItem("Pretty Tanks/Debug/Unlock all missions")]
        public static void UnlockAllMissions() {
            for (int i = 0; i < GameManager.Names.Length; ++i) {
                SaveSystem.Complete(i);
            }
            Debug.Log("Все миссии открыты для проверки. Прогресс сохранён.");
        }

        [MenuItem("Pretty Tanks/4. Build Windows x64")]
        public static void Build() {
            Configure();
            Validate();
            Directory.CreateDirectory("Build/Windows");
            var report = BuildPipeline.BuildPlayer(
                new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/Main.unity" },
                                         locationPathName = "Build/Windows/PrettyTanks.exe",
                                         target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception("Сборка не завершена: " + report.summary.result);
            }
        }

        [MenuItem("Pretty Tanks/Debug/Reset intro")]
        public static void ResetIntro()
        {
            PlayerPrefs.DeleteKey(WelcomeGuide.SeenKey);
            PlayerPrefs.Save();

            Debug.Log("Вступление будет показано перед следующим запуском уровня.");
        }

        private static void SetGameViewAspect()
        {
            try
            {
                var assembly = typeof(UnityEditor.Editor).Assembly;
                var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
                var groupType = assembly.GetType("UnityEditor.GameViewSizeGroupType");
                var viewType = assembly.GetType("UnityEditor.GameView");

                const BindingFlags flags =
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.FlattenHierarchy;

                var instance = sizesType.BaseType
                    .GetProperty("instance", flags)
                    .GetValue(null);

                var standalone = System.Enum.Parse(groupType, "Standalone");

                var group = sizesType.GetMethod("GetGroup", flags)
                    .Invoke(instance, new object[] { standalone });

                var labels = (string[])group.GetType()
                    .GetMethod("GetDisplayTexts", flags)
                    .Invoke(group, null);

                int index = System.Array.FindIndex(
                    labels,
                    label => label.StartsWith(
                        "16:9",
                        System.StringComparison.Ordinal));

                if (index < 0)
                {
                    Debug.LogWarning(
                        "Формат 16:9 не найден. Выберите его во вкладке Game.");
                    return;
                }

                var gameView = EditorWindow.GetWindow(viewType);

                viewType.GetProperty("selectedSizeIndex", flags)
                    .SetValue(gameView, index);

                gameView.Repaint();
            }
            catch (System.Exception)
            {
                Debug.LogWarning(
                    "Не удалось автоматически выбрать 16:9. " +
                    "Во вкладке Game выберите Free Aspect → 16:9.");
            }
        }
    }
}
