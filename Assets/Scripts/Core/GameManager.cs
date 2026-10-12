using UnityEngine;
using UnityEngine.InputSystem;

namespace PrettyTanks
{
    /// <summary>Создаёт сессию и переключает меню, бой, паузу и результат.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        private bool started;
        private bool finished;
        private bool paused;
        private InputAction pause;
        private float acceptInputAt;

        public static GameManager I { get; private set; }

        public Transform World { get; private set; }

        public TankController Player { get; private set; }

        public EscortTruck Truck { get; private set; }

        public LevelGenerator Level { get; private set; }

        public WaveManager Waves { get; private set; }

        public bool Playing => started && !finished && !paused;

        public bool AcceptsPlayerInput => Playing && Time.unscaledTime >= acceptInputAt;

        public bool Started => started;

        public bool Finished => finished;

        public bool Paused => paused;

        public bool Won { get; private set; }

        public string Result { get; private set; }

        public int LevelIndex { get; private set; }

        public static readonly string[] Names = { "Розовый пригород", "Лавандовый лес", "Ледяная набережная",
                                                  "Красивый сад", "Возвращение в пригород", "Дорога к кошачьему дому" };

        public static int MapForLevel(int level) => level % 4;

        private void Awake()
        {
            I = this;
            Time.timeScale = 1f;
            Physics2D.queriesHitTriggers = true;
            CreateCamera();
            gameObject.AddComponent<UIManager>();
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<Vfx>();
            gameObject.AddComponent<DevTools>();
            pause = new InputAction("Pause", binding: "<Keyboard>/escape");
            pause.Enable();
        }

        private static void CreateCamera()
        {
            if (Camera.main)
            {
                return;
            }
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 11.3f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(0.98f, 0.93f, 0.96f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
        }

        private void Update()
        {
            if (pause.WasPressedThisFrame() && started && !finished)
            {
                TogglePause();
            }
            FramePlayfield();
        }

        private static void FramePlayfield()
        {
            Camera camera = Camera.main;
            if (!camera)
            {
                return;
            }

            float availableHeight = (UITheme.PlayfieldBottom - UITheme.PlayfieldTop) / UITheme.Height;
            float verticalSize = 9.5f / availableHeight;
            float horizontalSize = 17f / (camera.aspect * 0.96f);
            camera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
            float screenCenter = (UITheme.PlayfieldTop + UITheme.PlayfieldBottom) / (2f * UITheme.Height);
            float cameraY = (2f * screenCenter - 1f) * camera.orthographicSize;
            camera.transform.position = new Vector3(0f, cameraY, -10f);
        }

        public void StartLevel(int index)
        {
            if (index < 0 || index >= Names.Length || index > SaveSystem.Unlocked)
            {
                return;
            }

            ClearWorld();
            Time.timeScale = 1f;
            paused = false;
            finished = false;
            started = false;
            Won = false;
            Result = string.Empty;
            LevelIndex = index;

            World = new GameObject("Level " + (LevelIndex + 1)).transform;
            var gridObject = new GameObject("Tilemap grid");
            gridObject.transform.SetParent(World);
            Level = gridObject.AddComponent<LevelGenerator>();
            Level.Generate(MapForLevel(LevelIndex));
            Truck = TankFactory.CreateTruck(World, Level.Route, LevelIndex);
            Player = TankFactory.Create(true, Level.Route[0] + new Vector2(1.7f, 0f), World);
            Waves = World.gameObject.AddComponent<WaveManager>();
            Waves.Init(LevelIndex);
            var powerConfig = Resources.Load<PowerUpConfig>("PowerUps");
            if (!powerConfig || powerConfig.StartingRepair)
            {
                Waves.CreateCrate(PowerKind.Repair, Player.transform.position + Vector3.up * 2f);
            }

            started = true;
            DelayPlayerInput();
            EventBus.State();
            EventBus.Say("Осмотрись: WASD — ехать, мышь — прицел, ЛКМ — огонь, E — ремонт рядом с фургоном.");
        }

        public void Finish(bool win, string text)
        {
            if (finished)
            {
                return;
            }
            finished = true;
            Won = win;
            Result = text;
            if (win)
            {
                SaveSystem.Complete(LevelIndex);
            }
            Time.timeScale = 0f;
            EventBus.State();
        }

        public void TogglePause()
        {
            if (!started || finished)
            {
                return;
            }
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
            DelayPlayerInput();
            EventBus.State();
        }

        public void Menu()
        {
            Time.timeScale = 1f;
            started = false;
            finished = false;
            paused = false;
            ClearWorld();
            EventBus.State();
        }

        private void DelayPlayerInput()
        {
            // Нажатие на кнопку меню не должно одновременно выпускать снаряд.
            acceptInputAt = Time.unscaledTime + 0.2f;
        }

        private void ClearWorld()
        {
            if (!World)
            {
                return;
            }
            World.gameObject.SetActive(false);
            Destroy(World.gameObject);
        }

        private void OnDestroy()
        {
            pause?.Dispose();
            Time.timeScale = 1f;
            if (I == this)
            {
                I = null;
            }
        }
    }
}
