using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Выбор маршрута и обучение. Не управляет состоянием боя.</summary>
    public sealed class MainMenuView
    {
        private static readonly string[] Descriptions = {
            "Первые встречи с врагами. Держись рядом с котятами и береги фургон.",
            "Враги идут чаще. Выбирай позицию перед фургоном и не забывай про ремонт.",
            "Появляются снайперы и рикошетники. Используй отражения и прикрытия.",
            "Знакомые противники наступают плотнее. Чередуй бой и ремонт.",
            "Повторный рейс через пригород. Более сильные враги и короткие паузы между волнами.",
            "Финальная поездка через лес: танк-босс, призраки и самые частые волны."
        };

        private static readonly Color[] Accents = { new Color(0.98f, 0.76f, 0.84f), new Color(0.82f, 0.77f, 0.95f),
                                                    new Color(0.68f, 0.85f, 0.94f),
                                                    new Color(0.69f, 0.85f, 0.75f) };

        private readonly UITheme theme;
        private readonly AudioManager audio;
        private int selectedLevel;
        private int page;
        private readonly WelcomeGuide guide;
        private bool wasVisible;

        public MainMenuView(UITheme theme, AudioManager audio)
        {
            this.theme = theme;
            this.audio = audio;
            guide = new WelcomeGuide(theme);
        }

        public void Hide()
        {
            wasVisible = false;
        }

        public void Draw()
        {
            if (!wasVisible)
            {
                selectedLevel = SaveSystem.Unlocked;
                page = selectedLevel / 4;
                wasVisible = true;
            }

            if (guide.IsOpen)
            {
                guide.Draw();
                return;
            }

            DrawBackground();
            DrawHeader();
            DrawIllustrationCard();
            DrawRouteList();
            DrawFooter();
        }

        private void DrawBackground()
        {
            theme.Panel(new Rect(-30, -30, 1340, 860), new Color(1f, 0.94f, 0.97f, 1f));
            for (int i = 0; i < 18; i++)
            {
                float x = 22f + (i * 107f) % 1240f;
                float y = (i * 173f + Time.unscaledTime * (3f + i % 3)) % 800f;
                theme.Sprite(new Rect(x, y, 12 + i % 3 * 4, 12 + i % 3 * 4),
                             i % 2 == 0 ? "star" : "heart", new Color(1f, 0.64f, 0.77f, 0.42f));
            }
        }

        private void DrawHeader()
        {
            theme.Sprite(new Rect(50, 30, 38, 38), "bow", UITheme.Pink);
            theme.Text(new Rect(102, 35, 350, 28), "PRETTY TANKS", theme.Heading);
            if (theme.Button(
                new Rect(625, 30, 225, 46),
                "Выйти из игры"))
            {
                // Записываем текущие сохранения на диск.
                PlayerPrefs.Save();

            #if UNITY_EDITOR
                // В редакторе останавливаем Play, сам Unity остаётся открытым.
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                // В собранной игре закрываем приложение.
                Application.Quit();
            #endif

                // Завершаем обработку текущего события интерфейса.
                GUIUtility.ExitGUI();
            }
            if (theme.Button(new Rect(865, 30, 160, 46), "Как играть"))
            {
                guide.OpenHelp();
                GUIUtility.ExitGUI();
            }
            if (theme.Button(new Rect(1040, 30, 190, 46),
                             AudioListener.volume > 0f ? "Звук: вкл." : "Звук: выкл."))
            {
                audio.ToggleMute();
            }
        }

        private void DrawIllustrationCard()
        {
            theme.ShadowPanel(new Rect(50, 108, 550, 530), new Color(1f, 0.91f, 0.94f));
            theme.ShadowPanel(new Rect(123, 271, 404, 272), Color.white);
            DrawGameConvoy();
            theme.Text(new Rect(78, 133, 490, 20), "ПУТЕШЕСТВИЕ С КОТЯТАМИ", theme.Caption);
            theme.Text(new Rect(76, 166, 490, 65), "Pretty Tanks", theme.Hero);
            theme.Text(new Rect(80, 231, 480, 28), "Доставь котят домой", theme.Heading);
            theme.Sprite(new Rect(519, 179, 36, 36), "heart", UITheme.Pink, -12f);
            theme.ShadowPanel(new Rect(64, 549, 522, 77), UITheme.Paper);
            theme.Sprite(new Rect(80, 568, 37, 37), "heart", UITheme.Pink);
            theme.Text(new Rect(131, 562, 440, 26), "Три котёнка рассчитывают на тебя", theme.Heading);
            theme.Text(new Rect(132, 592, 430, 24), "Оберегай от врагов и делись запчастями", theme.Small);
        }

        private void DrawGameConvoy()
        {
            theme.Panel(new Rect(127, 275, 396, 264), new Color(1f, 0.96f, 0.98f));
            theme.Sprite(new Rect(153, 294, 23, 23), "star", new Color(1f, 0.78f, 0.48f), -12f);
            theme.Sprite(new Rect(474, 475, 22, 22), "heart", new Color(1f, 0.57f, 0.73f), 12f);
            DrawMenuTruck(new Vector2(225f, 407f), 95f);
            DrawMenuTank(new Vector2(389f, 417f), 105f);
        }

        private void DrawMenuTruck(Vector2 center, float scale)
        {
            Color tail = new Color(1f, 0.89f, 0.76f);
            DrawMenuPart(center, scale, "van", 0f, 0f, 1.35f, 2f, new Color(1f, 0.62f, 0.77f));
            for (int i = 0; i < 3; i++)
            {
                Color color = i == 1 ? new Color(1f, 0.9f, 0.65f) : new Color(0.97f, 0.95f, 1f);
                DrawMenuPart(center, scale, "cat", (i - 1) * 0.32f, -0.2f, 0.56f, 0.56f, color);
            }
        }

        private void DrawMenuTank(Vector2 center, float scale)
        {
            TankData data = Resources.Load<TankData>("PlayerTank");
            Color color = data ? data.Color : new Color(1f, 0.42f, 0.69f);
            Color track = new Color(0.42f, 0.32f, 0.49f);
            DrawMenuPart(center, scale, "hull", 0f, 0f, 0.83f, 1.12f, color);
            DrawMenuPart(center, scale, "track", -0.49f, 0f, 0.27f, 1.22f, track);
            DrawMenuPart(center, scale, "track", 0.49f, 0f, 0.27f, 1.22f, track);
            DrawMenuPart(center, scale, "engine", 0f, -0.48f, 0.43f, 0.28f, new Color(0.85f, 0.7f, 0.8f));
            DrawMenuPart(center, scale, "round", 0f, 0.12f, 0.6f, 0.6f, color * new Color(0.95f, 0.9f, 0.95f));
            DrawMenuPart(center, scale, "barrel", 0f, 0.59f, 0.22f, 0.75f, color);
            DrawMenuPart(center, scale, "bow", 0f, 0.1f, 0.63f, 0.35f, new Color(1f, 0.35f, 0.61f));
        }

        private void DrawMenuPart(Vector2 center, float scale, string spriteName,
                                  float x, float y, float width, float height, Color tint, float rotation = 0f)
        {
            if (Event.current.type != EventType.Repaint) return;
            Sprite sprite = Art.Sprite(spriteName);
            if (!sprite) return;
            Rect bounds = new Rect(center.x + (x - width * 0.5f) * scale, center.y - (y + height * 0.5f) * scale, width * scale, height * scale);
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            try
            {
                GUI.color = tint;
                GUIUtility.RotateAroundPivot(rotation, bounds.center);
                GUI.DrawTexture(bounds, sprite.texture, ScaleMode.StretchToFill, true);
            }
            finally
            {
                GUI.matrix = previousMatrix;
                GUI.color = previousColor;
            }
        }

        private void DrawRouteList()
        {
            theme.ShadowPanel(new Rect(620, 108, 610, 530), UITheme.Paper);
            theme.Text(new Rect(660, 131, 300, 28), "Выбери уровень", theme.Heading);
            theme.Text(new Rect(946, 136, 164, 22), "ОТКРЫТО " + (SaveSystem.Unlocked + 1) + " / " + GameManager.Names.Length,
                       theme.Caption);
            int lastPage = (GameManager.Names.Length - 1) / 4;
            if (theme.Button(new Rect(1110, 128, 40, 32), "←", false, page > 0)) page--;
            if (theme.Button(new Rect(1160, 128, 40, 32), "→", false, page < lastPage)) page++;
            for (int i = 0; i < 4; i++)
            {
                int index = page * 4 + i;
                if (index < GameManager.Names.Length)
                {
                    DrawRouteCard(index, new Rect(645, 181 + i * 92, 560, 82));
                }
            }

            theme.Panel(new Rect(642, 557, 565, 72), new Color(1f, 0.91f, 0.95f));
            theme.Text(new Rect(659, 565, 530, 22),
                       "МИССИЯ " + (selectedLevel + 1) + "  ·  около минуты", theme.Caption);
            theme.Text(new Rect(659, 589, 530, 38), Descriptions[selectedLevel], theme.Small);
            if (theme.Button(new Rect(645, 655, 560, 62), "Отправиться в путь  →", true))
            {
                guide.RequestStart(selectedLevel);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawRouteCard(int index, Rect bounds)
        {
            bool unlocked = index <= SaveSystem.Unlocked;
            bool selected = index == selectedLevel;
            if (selected)
            {
                theme.Panel(new Rect(bounds.x - 2, bounds.y - 2, bounds.width + 4, bounds.height + 4),
                            UITheme.Pink);
            }
            if (theme.Button(bounds, string.Empty, false, unlocked))
            {
                selectedLevel = index;
            }

            theme.Panel(new Rect(bounds.x + 13, bounds.y + 15, 49, 49),
                        unlocked ? Accents[GameManager.MapForLevel(index)] : new Color(0.84f, 0.80f, 0.84f));
            theme.Text(new Rect(bounds.x + 13, bounds.y + 15, 49, 49), (index + 1).ToString("00"), theme.Centered);
            theme.Text(new Rect(bounds.x + 74, bounds.y + 10, 300, 43), GameManager.Names[index], theme.Heading);
            theme.Text(new Rect(bounds.x + 75, bounds.y + 52, 300, 26),
                       unlocked ? CardDetail(index) : "Пройди предыдущий уровень", theme.Caption);
            theme.RoutePreview(new Rect(bounds.x + 383, bounds.y + 9, 122, 64), GameManager.MapForLevel(index),
                               unlocked ? Color.white : new Color(0.8f, 0.76f, 0.82f, 0.65f));
            theme.Text(new Rect(bounds.x + 518, bounds.y + 12, 26, 28),
                       selected   ? "♥"
                       : unlocked ? "→"
                                  : "•",
                       theme.Heading);
        }

        private static string CardDetail(int index)
        {
            var config = Resources.Load<DifficultyData>("Difficulty");
            var profile = config.ForLevel(index);
            return $"Волна: {profile.CountForWave(1)} врага · интервал {profile.SpawnSpacing:F1} с";
        }

        private void DrawFooter()
        {
            theme.Text(new Rect(70, 661, 520, 28), "WASD — ехать     Мышь — прицел     ЛКМ — огонь", theme.Small);
            theme.Text(new Rect(70, 692, 520, 28), "E — ремонт фургона     Пробел — бомба", theme.Small);
        }
    }
}
