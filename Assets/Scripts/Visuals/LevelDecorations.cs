using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Декорации не создают коллайдеров и не меняют правила боя.</summary>
    public static class LevelDecorations
    {
        public static readonly Color[] GroundColors = {
            new Color(0.91f, 0.94f, 0.85f), new Color(0.86f, 0.85f, 0.94f),
            new Color(0.85f, 0.94f, 0.98f), new Color(0.85f, 0.94f, 0.86f)
        };
        public static readonly Color[] RoadColors = {
            new Color(1f, 0.87f, 0.85f), new Color(0.97f, 0.88f, 0.94f),
            new Color(0.95f, 0.97f, 1f), new Color(1f, 0.92f, 0.80f)
        };
        private static readonly Color[] TreeColors = {
            Color.white, new Color(0.78f, 0.76f, 1f),
            new Color(0.84f, 0.95f, 1f), new Color(1f, 0.91f, 0.74f)
        };

        public static void Build(LevelGenerator level, int index)
        {
            var root = new GameObject("Scenery — decorative only").transform;
            root.SetParent(level.transform, false);
            // Деревья только слева и справа; каменная рамка остаётся в LevelGenerator.
            for (int y = -8; y <= 8; y += 2)
            {
                Tree(root, new Vector2(-16.2f, y), index, 1.8f);
                Tree(root, new Vector2(16.1f, y), index, 1.8f);
            }
            for (int x = -13; x <= 12; x++)
            {
                for (int y = -7; y <= 6; y++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    int pattern = Mathf.Abs(x * 29 + y * 43 + index * 17);
                    if (level.Blocked(p) || level.OnRoute(p, 1.9f) || pattern % 9 != 0) continue;
                    Flower(root, p, index);
                }
            }
            // Маленькие указатели показывают направление конвоя вдоль дороги.
            for (int i = 1; i < level.Route.Length; i++)
            {
                Vector2 a = level.Route[i - 1], b = level.Route[i];
                Vector2 direction = (b - a).normalized;
                for (float distance = 2f; distance < Vector2.Distance(a, b) - 1f; distance += 3f)
                {
                    var mark = Art.Part("Route heart", root, "heart", new Color(1f, 1f, 1f, 0.6f),
                                        a + direction * distance, Vector2.one * 0.20f, -7);
                    mark.transform.localRotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, direction));
                }
            }
            Vector2 finish = level.Route[level.Route.Length - 1];
            Art.Part("Welcome garden", root, "round", new Color(0.65f, 0.83f, 0.71f, 0.5f), finish,
                     new Vector2(2.5f, 2f), -8);
            Art.Part("Home ribbon", root, "bow", UITheme.Pink, finish + Vector2.up * 0.75f,
                     new Vector2(0.6f, 0.35f), -1);
        }

        private static void Tree(Transform root, Vector2 position, int index, float size)
        {
            var sprite = Art.Sprite("blossom-tree");
            if (!sprite) return;
            // Размер в мире не зависит от разрешения иллюстрации.
            Vector2 scale = new Vector2(size / sprite.bounds.size.x, size / sprite.bounds.size.y);
            Art.Part("Blossom canopy", root, "blossom-tree", TreeColors[index], position, scale, -3);
        }

        private static void Flower(Transform root, Vector2 position, int index)
        {
            Art.Part("Leaves", root, "round", new Color(0.56f, 0.75f, 0.62f, 0.65f), position,
                     new Vector2(0.45f, 0.20f), -6);
            Color petal = index == 2 ? Color.white : index == 1 ? new Color(0.79f, 0.67f, 0.91f) : new Color(1f, 0.72f, 0.80f);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * Mathf.PI * 2f / 5f;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.11f;
                Art.Part("Petal", root, "round", petal, position + offset, Vector2.one * 0.18f, -5);
            }
            Art.Part("Flower centre", root, "round", new Color(1f, 0.91f, 0.60f), position, Vector2.one * 0.11f, -4);
        }
    }
}
