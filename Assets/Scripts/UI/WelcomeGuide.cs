using UnityEngine;

namespace PrettyTanks
{
    public sealed class WelcomeGuide
    {
        public const string SeenKey = "PrettyTanks.IntroSeen.v1";

        private enum Page
        {
            Closed,
            Story,
            Help
        }

        private readonly UITheme theme;
        private readonly Texture2D comic;
        private Page page;
        private int pendingLevel = -1;

        public bool IsOpen => page != Page.Closed;

        public WelcomeGuide(UITheme theme)
        {
            this.theme = theme;
            comic = Resources.Load<Texture2D>("Intro/kitten-comic");

            if (!comic)
            {
                Debug.LogWarning(
                    "Не найдена картинка Resources/Intro/kitten-comic. " +
                    "Вступление покажет текст без изображения.");
            }
        }

        public void RequestStart(int level)
        {
            // Не запускаем отсутствующий или закрытый уровень.
            if (level < 0 || level >= GameManager.Names.Length ||
                level > SaveSystem.Unlocked)
            {
                return;
            }

            if (PlayerPrefs.GetInt(SeenKey, 0) == 1)
            {
                GameManager.I.StartLevel(level);
                return;
            }

            // Мир игры пока не создаётся: враги не появляются за комиксом.
            pendingLevel = level;
            page = Page.Story;
        }

        public void OpenHelp()
        {
            // Обычная памятка из меню не должна сама запускать бой.
            pendingLevel = -1;
            page = Page.Help;
        }

        public void Draw()
        {
            if (!IsOpen)
            {
                return;
            }

            theme.Panel(
                new Rect(0, 0, 1280, 800),
                new Color(1f, 0.94f, 0.97f));

            theme.ShadowPanel(
                new Rect(64, 48, 1152, 704),
                UITheme.Paper);

            if (page == Page.Story)
            {
                DrawStory();
            }
            else
            {
                DrawHelp();
            }
        }

        private void DrawStory()
        {
            // Большая область для комикса из двух рядов.
            // Заголовок и отдельные подписи убраны, чтобы картинка была крупнее.
            if (comic)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    // Белый цвет сохраняет исходные цвета картинки.
                    Color previousColor = GUI.color;
                    GUI.color = Color.white;

                    GUI.DrawTexture(
                        new Rect(88, 64, 1104, 600),
                        comic,
                        ScaleMode.ScaleToFit,
                        true);

                    GUI.color = previousColor;
                }
            }
            else
            {
                // Если картинка отсутствует, история всё равно будет понятна.
                theme.Text(
                    new Rect(160, 220, 960, 240),
                    "Троих котят нашли под дождём.\n\n" +
                    "Теперь розовый фургон везёт их в приют, " +
                    "но похитители на танках преграждают дорогу.\n\n" +
                    "Защити фургон и довези котят до дома.",
                    theme.Body);
            }

            if (theme.Button(
                new Rect(390, 682, 500, 48),
                "Как спасти котят →",
                true))
            {
                page = Page.Help;

                // Этот клик не должен нажать кнопку на следующем экране.
                GUIUtility.ExitGUI();
            }
        }

        private void DrawHelp()
        {
            theme.Text(
                new Rect(92, 72, 1096, 44),
                "Как спасти котят",
                theme.Title);

            theme.Text(
                new Rect(92, 140, 440, 96),
                "Довези фургон до дома и сохрани свой танк.\n" +
                "Фургон едет сам. Защищай его от врагов " +
                "и следи за двумя шкалами здоровья.",
                theme.Body);

            string[] keys =
            {
                "WASD / СТРЕЛКИ",
                "МЫШЬ + ЛКМ",
                "E РЯДОМ С ФУРГОНОМ",
                "ПРОБЕЛ",
                "ESC"
            };

            string[] hints =
            {
                "Двигайся и занимай позицию для защиты фургона.",
                "Наводи башню мышью и удерживай ЛКМ для стрельбы.",
                "Во время ремонта танк не едет и не стреляет. " +
                "После ремонта временно отключится один модуль.",
                "Взорви подобранную бомбу до окончания её таймера.",
                "Пауза и продолжение игры."
            };

            for (int i = 0; i < keys.Length; i++)
            {
                float y = 250 + i * 70;

                theme.Text(
                    new Rect(92, y, 440, 24),
                    keys[i],
                    theme.Heading);

                theme.Text(
                    new Rect(92, y + 26, 440, 46),
                    hints[i],
                    theme.Small);
            }

            theme.Text(
                new Rect(590, 130, 570, 28),
                "Подарки — подбирай своим танком",
                theme.Heading);

            for (int i = 0; i < 7; i++)
            {
                PowerKind kind = (PowerKind)i;
                float y = 171 + i * 65;

                // Подложка помогает увидеть даже белый подарок.
                theme.Panel(
                    new Rect(590, y + 2, 48, 48),
                    new Color(0.83f, 0.78f, 0.88f));

                theme.Sprite(
                    new Rect(596, y + 8, 36, 36),
                    "gift",
                    PowerUpLook.ColorFor(kind));

                theme.Text(
                    new Rect(652, y, 510, 25),
                    PowerUpLook.Title(kind),
                    theme.Heading);

                theme.Text(
                    new Rect(652, y + 27, 510, 37),
                    PowerUpLook.Hint(kind),
                    theme.Small);
            }

            theme.Text(
                new Rect(92, 626, 1096, 34),
                "Бонусы включаются при подборе. Для бомбы нужен Пробел. " +
                "Следи за временем действия усилений.",
                theme.Small);

            string button = pendingLevel >= 0
                ? "Понятно — спасать котят!"
                : "Вернуться в меню";

            if (theme.Button(
                new Rect(390, 682, 500, 48),
                button,
                true))
            {
                FinishHelp();
                GUIUtility.ExitGUI();
            }
        }

        private void FinishHelp()
        {
            int level = pendingLevel;
            pendingLevel = -1;
            page = Page.Closed;

            if (level < 0)
            {
                return;
            }

            GameManager game = GameManager.I;
            game.StartLevel(level);

            // Запоминаем просмотр только после успешного запуска уровня.
            if (game.Started && game.LevelIndex == level)
            {
                PlayerPrefs.SetInt(SeenKey, 1);
                PlayerPrefs.Save();
            }
        }
    }
}