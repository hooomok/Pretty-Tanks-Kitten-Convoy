using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Общие цвета, размеры и состояния интерфейса.</summary>
    public sealed class UITheme : System.IDisposable
    {
        public const float Width = 1280f;
        public const float Height = 800f;
        // Свободная область между HUD-панелями, в координатах интерфейса.
        public const float PlayfieldTop = 112f;
        public const float PlayfieldBottom = 648f;
        public static readonly Color Ink = new Color(0.25f, 0.13f, 0.32f);
        public static readonly Color Muted = new Color(0.51f, 0.42f, 0.54f);
        public static readonly Color Pink = new Color(0.94f, 0.24f, 0.53f);
        public static readonly Color Mint = new Color(0.40f, 0.70f, 0.60f);
        public static readonly Color Paper = new Color(1f, 0.985f, 0.99f);

        private readonly Font font;
        private readonly Texture2D panel;
        private readonly GUIStyle panelStyle;
        private readonly GUIStyle buttonStyle;
        private readonly UIAppearanceConfig appearance;
        private readonly Texture2D[] routeTextures = new Texture2D[4];

        public GUIStyle Hero { get; }

        public GUIStyle Title { get; }

        public GUIStyle CenteredTitle { get; }

        public GUIStyle Heading { get; }

        public GUIStyle Body { get; }

        public GUIStyle Small { get; }

        public GUIStyle Caption { get; }

        public GUIStyle WhiteButton { get; }

        public GUIStyle Centered { get; }

        public UITheme()
        {
            appearance = Resources.Load<UIAppearanceConfig>("UIAppearance");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            panel = Art.Sprite("panel").texture;
            panelStyle = new GUIStyle { border = new RectOffset(24, 24, 24, 24) };
            SetBackgrounds(panelStyle, panel);

            Hero = Label(52, Ink, FontStyle.Bold);
            Title = Label(30, Ink, FontStyle.Bold);
            CenteredTitle = Label(30, Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            Heading = Label(20, Ink, FontStyle.Bold);
            Body = Label(18, Ink);
            Small = Label(15, Muted);
            Caption = Label(12, Muted, FontStyle.Bold);
            Centered = Label(18, Ink, FontStyle.Bold, TextAnchor.MiddleCenter);
            WhiteButton = Label(20, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Кнопка только обрабатывает ввод. Фон и подпись рисуются отдельно.
            buttonStyle = new GUIStyle { border = new RectOffset(24, 24, 24, 24) };
            SetBackgrounds(buttonStyle, panel);
            SetTextColors(buttonStyle, Color.clear);
        }

        private GUIStyle Label(int size, Color color, FontStyle weight = FontStyle.Normal,
                               TextAnchor alignment = TextAnchor.UpperLeft)
        {
            var style = new GUIStyle { font = font,
                                       fontSize = size,
                                       fontStyle = weight,
                                       alignment = alignment,
                                       wordWrap = true,
                                       clipping = TextClipping.Clip,
                                       padding = new RectOffset(0, 0, 0, 0) };

            // У обычной подписи все восемь состояний одинаковы.
            SetTextColors(style, color);
            return style;
        }

        private static void SetTextColors(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
            style.onNormal.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.onFocused.textColor = color;
        }

        private static void SetBackgrounds(GUIStyle style, Texture2D texture)
        {
            style.normal.background = texture;
            style.hover.background = texture;
            style.active.background = texture;
            style.focused.background = texture;
            style.onNormal.background = texture;
            style.onHover.background = texture;
            style.onActive.background = texture;
            style.onFocused.background = texture;
        }

        public void Panel(Rect bounds, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.Label(bounds, GUIContent.none, panelStyle);
            GUI.color = previous;
        }

        public void ShadowPanel(Rect bounds, Color color)
        {
            Panel(new Rect(bounds.x, bounds.y + 5f, bounds.width, bounds.height),
                  new Color(0.43f, 0.25f, 0.36f, 0.08f));
            Panel(bounds, Color.white);
            Panel(new Rect(bounds.x + 2, bounds.y + 2, bounds.width - 4, bounds.height - 4), color);
        }

        public bool Button(Rect bounds, string label, bool primary = false, bool enabled = true)
        {
            enabled = enabled && GUI.enabled;
            bool hovered = enabled && bounds.Contains(Event.current.mousePosition);
            Color background = primary ? Pink : Paper;
            if (hovered)
            {
                background = primary ? new Color(0.78f, 0.24f, 0.45f) : new Color(1f, 0.91f, 0.95f);
            }
            if (!enabled)
            {
                background = new Color(0.90f, 0.86f, 0.89f);
            }

            bool oldEnabled = GUI.enabled;
            Color oldColor = GUI.color;
            GUI.enabled = enabled;
            if (primary)
            {
                Panel(new Rect(bounds.x - 3, bounds.y - 3, bounds.width + 6, bounds.height + 6), Color.white);
                Panel(new Rect(bounds.x, bounds.y + 5, bounds.width, bounds.height), new Color(0.68f, 0.24f, 0.43f, 0.25f));
            }
            GUI.color = background;
            bool clicked = GUI.Button(bounds, GUIContent.none, buttonStyle);
            GUI.enabled = oldEnabled;
            GUI.color = oldColor;
            GUI.Label(bounds, label, primary && enabled ? WhiteButton : Centered);
            return clicked;
        }

        public void Text(Rect bounds, string value, GUIStyle style = null)
        {
            GUI.Label(bounds, value, style ?? Body);
        }

        public Vector2 WorldHealthSize => appearance ? appearance.WorldHealthSize : new Vector2(36f, 4f);

        public void HealthBar(Rect bounds, float fraction, Color color, bool worldSpace = false)
        {
            if (!worldSpace)
            {
                Vector2 scale = appearance ? appearance.HudHealthScale : new Vector2(0.6f, 0.5f);
                bounds = new Rect(bounds.x, bounds.center.y - bounds.height * scale.y * 0.5f,
                                  bounds.width * scale.x, bounds.height * scale.y);
            }
            if (Event.current.type != EventType.Repaint) return;
            float radius = bounds.height * 0.5f;
            GUI.DrawTexture(bounds, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0f,
                            new Color(0.76f, 0.66f, 0.75f, 0.65f), 0f, radius);
            float fill = bounds.width * Mathf.Clamp01(fraction);
            if (fill > 0f)
            {
                GUI.DrawTexture(new Rect(bounds.x, bounds.y, fill, bounds.height), Texture2D.whiteTexture,
                                ScaleMode.StretchToFill, true, 0f, color, 0f, Mathf.Min(radius, fill * 0.5f));
            }
            float icon = appearance ? appearance.HealthIconSize : 10f;
            Sprite(new Rect(bounds.x - icon - 3f, bounds.center.y - icon * 0.5f, icon, icon), "heart", color);
        }

        public void Bar(Rect bounds, float value, Color color)
        {
            Panel(bounds, new Color(0.91f, 0.86f, 0.90f));
            if (value > 0f)
            {
                Panel(new Rect(bounds.x, bounds.y, bounds.width * Mathf.Clamp01(value), bounds.height), color);
            }
        }

        // Изображения остаются отдельными ресурсами: текст и кнопки всегда живые.
        public void Picture(Rect bounds, string resource, Color tint, float cornerRadius = 0f)
        {
            var sprite = Art.Sprite(resource);
            if (!sprite) return;
            Color previous = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(bounds, sprite.texture, ScaleMode.ScaleAndCrop, true,
                            0f, tint, 0f, cornerRadius);
            GUI.color = previous;
        }

        public void RoutePreview(Rect bounds, int index, Color tint)
        {
            // Выделение текстур нужно только при первой фактической отрисовке.
            if (Event.current.type != EventType.Repaint || index < 0 || index >= routeTextures.Length)
            {
                return;
            }

            Texture2D texture = GetRouteTexture(index);
            if (!texture)
            {
                return;
            }

            float requestedRadius = appearance ? appearance.CornerRadius : 14f;
            float radius = Mathf.Clamp(requestedRadius, 0f, Mathf.Min(bounds.width, bounds.height) * 0.5f);
            Color previous = GUI.color;
            try
            {
                GUI.color = Color.white;
                // Встроенное скругление Unity; цвет закрытого уровня сохраняется.
                GUI.DrawTexture(bounds, texture, ScaleMode.ScaleAndCrop, true, 0f, tint, 0f, radius);
            }
            finally
            {
                GUI.color = previous;
            }
        }

        private Texture2D GetRouteTexture(int index)
        {
            if (routeTextures[index])
            {
                return routeTextures[index];
            }

            var atlasSprite = Art.Sprite("route-previews");
            if (!atlasSprite)
            {
                return null;
            }

            Texture2D atlas = atlasSprite.texture;
            // Нулевая карточка — верхняя полоса. ReadPixels отсчитывает Y снизу.
            int bottom = Mathf.FloorToInt((3 - index) * atlas.height / 4f);
            int top = Mathf.FloorToInt((4 - index) * atlas.height / 4f);
            int height = top - bottom;
            RenderTexture previous = RenderTexture.active;
            RenderTexture buffer = RenderTexture.GetTemporary(atlas.width, atlas.height, 0);
            Texture2D preview = null;
            try
            {
                // GPU-копия позволяет оставить Read/Write у исходного PNG выключенным.
                Graphics.Blit(atlas, buffer);
                RenderTexture.active = buffer;
                preview = new Texture2D(atlas.width, height, TextureFormat.RGBA32, false)
                {
                    name = "Route preview " + index,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                preview.ReadPixels(new Rect(0, bottom, atlas.width, height), 0, 0, false);
                preview.Apply(false, true);
                routeTextures[index] = preview;
                return preview;
            }
            catch
            {
                if (preview)
                {
                    UnityEngine.Object.Destroy(preview);
                }
                throw;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(buffer);
            }
        }

        public void Dispose()
        {
            // Уничтожаем только созданные копии, не исходные Resources.
            for (int i = 0; i < routeTextures.Length; i++)
            {
                if (routeTextures[i])
                {
                    UnityEngine.Object.Destroy(routeTextures[i]);
                    routeTextures[i] = null;
                }
            }
        }

        public void Sprite(Rect bounds, string name, Color color, float rotation = 0f)
        {
            UnityEngine.Sprite sprite = Art.Sprite(name);
            if (!sprite)
            {
                return;
            }

            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            GUI.color = color;
            GUIUtility.RotateAroundPivot(rotation, bounds.center);
            GUI.DrawTexture(bounds, sprite.texture, ScaleMode.ScaleToFit, true);
            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }
    }
}
