using System.Text;
using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Отрисовывает HUD и экраны паузы. Главное меню вынесено в MainMenuView.</summary>
    public sealed class UIManager : MonoBehaviour
    {
        private UITheme theme;
        private MainMenuView mainMenu;
        private string message = string.Empty;
        private float messageUntil;
        private readonly StringBuilder powerDescription = new StringBuilder();

        private void OnEnable()
        {
            EventBus.Message += ShowMessage;
        }

        private void OnDisable()
        {
            EventBus.Message -= ShowMessage;
        }

        private void OnDestroy()
        {
            // Освобождаем кеш превью при остановке игры или смене сцены.
            theme?.Dispose();
        }

        private void ShowMessage(string value)
        {
            message = value;
            messageUntil = Time.unscaledTime + 5f;
        }

        private void EnsureStyles()
        {
            if (theme != null)
            {
                return;
            }

            // GUI-стили создаются только внутри OnGUI, когда уже доступен шрифт.
            theme = new UITheme();
            mainMenu = new MainMenuView(theme, GetComponent<AudioManager>());
        }

        private void OnGUI()
        {
            EnsureStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix =
                Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                              new Vector3(Screen.width / UITheme.Width, Screen.height / UITheme.Height, 1f));

            try
            {
                GameManager game = GameManager.I;
                if (!game.Started)
                {
                    mainMenu.Draw();
                    return;
                }

                mainMenu.Hide();
                bool previousEnabled = GUI.enabled;
                GUI.enabled = !game.Paused && !game.Finished;
                DrawConvoyHUD(game);
                if (game.Player)
                {
                    DrawPlayerHUD(game.Player);
                    DrawHealthInWorld(game.Truck.transform.position, game.Truck.Health);
                    foreach (TankController tank in game.World.GetComponentsInChildren<TankController>())
                    {
                        if (!tank.Player)
                        {
                            DrawHealthInWorld(tank.transform.position, tank.Health);
                        }
                    }
                }
                GUI.enabled = previousEnabled;

                DrawNotification(game);
                if (game.Finished || game.Paused)
                {
                    DrawOverlay(game);
                }
            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void DrawConvoyHUD(GameManager game)
        {
            theme.ShadowPanel(new Rect(15, 12, 1250, 92), UITheme.Paper);
            theme.Text(new Rect(35, 24, 430, 24), GameManager.Names[game.LevelIndex], theme.Heading);
            if (game.Player)
            {
                DrawTopHealth("ТАНК", game.Player.Health, 35f, UITheme.Pink);
            }

            if (game.Truck)
            {
                DrawTopHealth("ФУРГОН", game.Truck.Health, 260f, UITheme.Mint);
            }

            theme.Text(new Rect(490, 26, 270, 24), "ДОРОГА ДОМОЙ  " + Mathf.RoundToInt(game.Truck.Progress * 100f) + "%",
                       theme.Heading);
            theme.Bar(new Rect(490, 64, 246, 12), game.Truck.Progress, UITheme.Mint);
            theme.Text(new Rect(788, 25, 255, 24), "ВОЛНА " + game.Waves.Wave, theme.Heading);
            theme.Text(new Rect(788, 58, 255, 25),
                       "Врагов: " + game.Waves.AliveCount +
                           "  ·  Следующая: " + Mathf.CeilToInt(game.Waves.SecondsUntilWave) + " с",
                       theme.Small);
            if (theme.Button(new Rect(1070, 30, 166, 46), "Пауза  [Esc]"))
            {
                game.TogglePause();
                GUIUtility.ExitGUI();
            }
        }

        private void DrawTopHealth(string label, HealthSystem health, float x, Color color) {
            if (!health)
            {
                return;
            }

            string value =
                Mathf.CeilToInt(health.Current) + " / " +
                Mathf.CeilToInt(health.Maximum);

            theme.Text(
                new Rect(x, 54, 210, 25),
                label + "  " + value,
                theme.Heading);

            float fraction = health.Maximum > 0f
                ? health.Current / health.Maximum
                : 0f;

            theme.HealthBar(
                new Rect(x + 14, 84, 190, 16),
                fraction,
                color);
        }

        private void DrawPlayerHUD(TankController player)
        {
            theme.ShadowPanel(new Rect(15, 656, 1250, 130), UITheme.Paper);
            theme.Text(new Rect(33, 672, 210, 25), "ТВОЙ ТАНК", theme.Heading);
            DrawModuleBars(player.Modules);
            theme.Text(
                new Rect(33, 723, 210, 22),
                $"ПАТРОНЫ: {player.Weapon.Ammo}/{player.Weapon.MaxAmmo}",
                theme.Small
            );
            DrawWeaponAndRepair(player);
            DrawPowerUps(player.Power);
        }

        private void DrawModuleBars(ModuleSystem modules)
        {
            for (int i = 0; i < 5; i++)
            {
                ModuleKind kind = (ModuleKind)i;
                float x = 250 + i * 133;
                bool offline = modules.Offline(kind);
                theme.Text(new Rect(x, 674, 130, 23), ModuleSystem.Label(kind), theme.Small);
                theme.Bar(new Rect(x, 707, 110, 10), modules.Fraction(kind), offline ? Color.gray : UITheme.Mint);
                if (offline)
                {
                    theme.Text(new Rect(x, 722, 130, 18), "ОТКЛЮЧЕНО", theme.Caption);
                }
            }
        }

        private void DrawWeaponAndRepair(TankController player)
        {
            TruckRepair repair = player.GetComponent<TruckRepair>();
            string repairText = "E — ремонт фургона";
            if (repair.Busy)
            {
                repairText = "Ремонт: " + Mathf.RoundToInt(repair.Progress * 100f) + "%";
            }
            else if (repair.Cooldown > 0f)
            {
                repairText = "Ремонт через " + Mathf.CeilToInt(repair.Cooldown) + " с";
            }
            theme.Text(new Rect(960, 708, 280, 28), repairText, theme.Small);
        }

        private void DrawPowerUps(PowerUpSystem power)
        {
            powerDescription.Clear();
            for (int i = 0; i < 6; i++)
            {
                PowerKind kind = (PowerKind)i;
                if (!power.Active(kind))
                {
                    continue;
                }
                powerDescription.Append(PowerUpSystem.Label(kind));
                powerDescription.Append(' ');
                powerDescription.Append(Mathf.CeilToInt(power.Seconds(kind)));
                powerDescription.Append(" с   ");
            }

            string line =
                powerDescription.Length > 0
                    ? powerDescription.ToString()
                    : "WASD — движение     ЛКМ — огонь     E — ремонт     Пробел — бомба     Esc — пауза";
            theme.Text(new Rect(33, 752, 1210, 27), line, theme.Small);
        }

        private void DrawHealthInWorld(Vector3 position, HealthSystem health)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(position + Vector3.up * 1.2f);
            if (screen.z < 0f)
            {
                return;
            }

            Vector2 size = theme.WorldHealthSize;
            var bounds = new Rect(screen.x / Screen.width * UITheme.Width - size.x * 0.5f,
                                  (1f - screen.y / Screen.height) * UITheme.Height, size.x, size.y);
            theme.HealthBar(bounds, health.Current / health.Maximum, UITheme.Pink, true);
        }

        private void DrawNotification(GameManager game)
        {
            if (Time.unscaledTime >= messageUntil || game.Finished)
            {
                return;
            }
            theme.Panel(new Rect(245, 116, 790, 52), UITheme.Paper);
            theme.Text(new Rect(263, 128, 754, 32), message, theme.Small);
        }

        private void DrawOverlay(GameManager game)
        {
            theme.Panel(new Rect(-30, -30, 1340, 860), new Color(0.24f, 0.16f, 0.29f, 0.72f));
            theme.ShadowPanel(new Rect(360, 192, 560, 420), UITheme.Paper);
            theme.Sprite(new Rect(614, 217, 50, 46), game.Finished && !game.Won ? "bow" : "heart", UITheme.Pink);
            theme.Text(new Rect(395, 284, 490, 70), game.Finished ? game.Result : "Котята немного отдыхают",
                       theme.CenteredTitle);

            if (game.Finished && game.Won && game.LevelIndex < GameManager.Names.Length - 1)
            {
                if (theme.Button(new Rect(418, 374, 445, 50), "Следующий маршрут", true))
                {
                    game.StartLevel(game.LevelIndex + 1);
                    GUIUtility.ExitGUI();
                }
            }
            else if (!game.Finished && theme.Button(new Rect(418, 374, 445, 50), "Продолжить путь", true))
            {
                game.TogglePause();
                GUIUtility.ExitGUI();
            }

            if (game.Finished && theme.Button(new Rect(418, 436, 445, 50), "Проехать ещё раз", !game.Won))
            {
                game.StartLevel(game.LevelIndex);
                GUIUtility.ExitGUI();
            }
            if (theme.Button(new Rect(418, 508, 445, 50), "К выбору маршрута"))
            {
                game.Menu();
                GUIUtility.ExitGUI();
            }
        }
    }
}
