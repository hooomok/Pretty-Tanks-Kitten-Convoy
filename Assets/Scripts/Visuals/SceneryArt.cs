using System.Collections.Generic;
using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Лёгкие геометрические текстуры поверхностей. Создаются один раз без внешних пакетов.</summary>
    public static class SceneryArt
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Surface(string name)
        {
            if (Cache.TryGetValue(name, out Sprite cached)) return cached;
            const int size = 128;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float value = 1f;
                    if (name == "garden")
                    {
                        // Редкие травинки и точки, без контрастной сетки.
                        int pattern = (x * 37 + y * 71) % 991;
                        value = pattern < 6 ? 0.91f : 0.99f;
                    }
                    else if (name == "cobble")
                    {
                        int row = y / 32;
                        int u = (x + row % 2 * 32) % 64;
                        int v = y % 32;
                        value = u < 2 || v < 2 ? 0.83f : v < 5 ? 1f : v > 28 ? 0.91f : 0.97f;
                    }
                    else if (name == "brick-detail")
                    {
                        int u = (x + y / 32 % 2 * 32) % 64;
                        int v = y % 32;
                        value = u < 4 || v < 4 ? 0.69f : v < 8 ? 1f : 0.91f;
                    }
                    else if (name == "steel-detail")
                    {
                        int edge = Mathf.Min(Mathf.Min(x, 127 - x), Mathf.Min(y, 127 - y));
                        bool rivet = ((x - 14) * (x - 14) + (y - 14) * (y - 14) < 16) ||
                                     ((x - 114) * (x - 114) + (y - 114) * (y - 114) < 16);
                        value = edge < 4 ? 0.60f : edge < 9 ? 0.94f : rivet ? 0.64f : 0.83f + y / 128f * 0.13f;
                    }
                    else if (name == "wood-detail")
                    {
                        value = x % 32 < 3 || y < 4 || y > 123 ? 0.62f : x % 32 < 6 ? 0.96f : 0.86f;
                        if (y > 25 && y < 32 || y > 97 && y < 104) value *= 0.82f;
                    }
                    else if (name == "ice-detail")
                    {
                        int edge = Mathf.Min(Mathf.Min(x, 127 - x), Mathf.Min(y, 127 - y));
                        value = edge < 4 ? 0.76f : Mathf.Abs(x - y - 25) < 3 || Mathf.Abs(x - y + 40) < 2 ? 1f : 0.91f;
                    }
                    pixels[y * size + x] = new Color(value, value, value, 1f);
                }
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            Cache[name] = sprite;
            return sprite;
        }
    }
}
