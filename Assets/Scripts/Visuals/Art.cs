using System.Collections.Generic;
using UnityEngine;

namespace PrettyTanks
{
    public static class Art
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

        public static Sprite Sprite(string name)
        {
            if (!Sprites.TryGetValue(name, out Sprite sprite) || !sprite)
            {
                sprite = Resources.Load<Sprite>("Art/" + name);
                Sprites[name] = sprite;
            }
            return sprite;
        }

        private static bool IsIllustratedIcon(string shape)
        {
            return shape == "bow" || shape == "cat" || shape == "ears" ||
                   shape == "heart" || shape == "home" || shape == "star";
        }

        public static Vector3 ScaleToSize(Sprite sprite, Vector2 size)
        {
            if (!sprite)
            {
                return new Vector3(size.x, size.y, 1f);
            }
            Vector3 source = sprite.bounds.size;
            return new Vector3(size.x / source.x, size.y / source.y, 1f);
        }

        public static SpriteRenderer Part(string label, Transform parent, string shape, Color color,
                                          Vector2 position, Vector2 scale, int order)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1);
            var s = go.AddComponent<SpriteRenderer>();
            s.sprite = Sprite(shape);
            if (s.sprite && IsIllustratedIcon(shape))
            {
                // Новые PNG могут быть крупнее 128 px. Размер детали в мире
                // задаётся прежним scale, а не разрешением картинки.
                go.transform.localScale = ScaleToSize(s.sprite, scale);
            }
            s.color = color;
            s.sortingOrder = order;
            var mat = Resources.Load<Material>("ConfettiMaterial");
            if (mat)
            {
                s.sharedMaterial = mat;
            }
            return s;
        }
    }
}
