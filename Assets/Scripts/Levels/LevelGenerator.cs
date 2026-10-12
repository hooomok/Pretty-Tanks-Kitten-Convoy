using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PrettyTanks
{
    public enum WallKind
    {
        Brick,
        Steel,
        Wood,
        Ice
    }

    public sealed class LevelGenerator : MonoBehaviour
    {
        [SerializeField]
        private float burnSeconds = 3;
        [SerializeField]
        private float debrisSeconds = 5;
        private Tilemap walls;
        private Tilemap ground;
        private TilemapCollider2D wallCollider;
        private readonly Dictionary<Vector3Int, WallKind> kinds = new Dictionary<Vector3Int, WallKind>();
        private readonly Dictionary<Vector3Int, int> health = new Dictionary<Vector3Int, int>();
        private readonly Dictionary<Vector3Int, float> burning = new Dictionary<Vector3Int, float>();
        private readonly Dictionary<Vector3Int, float> debris = new Dictionary<Vector3Int, float>();
        private readonly HashSet<Vector3Int> slippery = new HashSet<Vector3Int>();
        private readonly Tile[] tiles = new Tile[4];
        private Tile floorTile;
        private Tile roadTile;
        private Tile iceTile;
        private Tile debrisTile;
        private readonly Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left,
                                                     Vector3Int.right };

        public Vector2[] Route { get; private set; }

        public List<Vector2> SpawnPoints { get; } = new List<Vector2>();

        public IEnumerable<Vector3Int> SteelCells
        {
            get {
                foreach (var pair in kinds)
                    if (pair.Value == WallKind.Steel)
                    {
                        yield return pair.Key;
                    }
            }
        }

        private Tile Make(string sprite, Color c, bool solid)
        {
            var t = ScriptableObject.CreateInstance<Tile>();
            t.sprite = sprite.EndsWith("-detail") || sprite == "garden" || sprite == "cobble"
                ? SceneryArt.Surface(sprite) : Art.Sprite(sprite);
            t.color = c;
            t.colliderType = solid ? Tile.ColliderType.Grid : Tile.ColliderType.None;
            return t;
        }

        private Tilemap Map(string label, int order, bool solid)
        {
            var go = new GameObject(label);
            go.transform.SetParent(transform, false);
            var m = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = order;
            var mat = Resources.Load<Material>("ConfettiMaterial");
            if (mat)
            {
                renderer.sharedMaterial = mat;
            }
            if (solid)
            {
                wallCollider = go.AddComponent<TilemapCollider2D>();
            }
            return m;
        }

        public void Generate(int level)
        {
            gameObject.AddComponent<Grid>();
            ground = Map("Pastel floor", -10, false);
            walls = Map("Destructible walls", 0, true);
            tiles[0] = Make("brick-detail", new Color(1, 0.65f, 0.73f), true);
            tiles[1] = Make("steel-detail", new Color(0.62f, 0.65f, 0.85f), true);
            tiles[2] = Make("wood-detail", new Color(0.86f, 0.72f, 0.54f), true);
            tiles[3] = Make("ice-detail", new Color(0.65f, 0.93f, 1), true);
            floorTile = Make("garden", LevelDecorations.GroundColors[level], false);
            roadTile = Make("cobble", LevelDecorations.RoadColors[level], false);
            iceTile = Make("ice-detail", new Color(0.77f, 0.94f, 1, 0.8f), false);
            debrisTile = Make("debris", new Color(0.68f, 0.54f, 0.6f), false);
            Route =
                level == 0   ? new[] { new Vector2(-12, -6), new Vector2(-5, -6), new Vector2(-5, 1),
                                       new Vector2(5, 1),    new Vector2(5, 6),   new Vector2(12, 6) }
                : level == 1 ? new[] { new Vector2(-12, 6), new Vector2(-5, 6), new Vector2(-5, -5),
                                       new Vector2(6, -5),  new Vector2(6, 5),  new Vector2(12, 5) }
                : level == 2
                    ? new[] { new Vector2(-12, -6), new Vector2(0, -6), new Vector2(0, 6), new Vector2(12, 6) }
                    : new[] { new Vector2(-12, 0), new Vector2(-7, 0), new Vector2(-7, 6), new Vector2(0, 6),
                              new Vector2(0, -5),  new Vector2(9, -5), new Vector2(9, 4),  new Vector2(12, 4) };
            for (int x = -15; x <= 14; x++)
            {
                for (int y = -9; y <= 8; y++)
                {
                    var c = new Vector3Int(x, y, 0);
                    Vector2 center = (Vector2Int) new Vector2Int(x, y) + Vector2.one * 0.5f;
                    bool road = NearRoute(center, 1.45f);
                    ground.SetTile(c, road ? roadTile : floorTile);
                    if (x == -15 || x == 14 || y == -9 || y == 8)
                    {
                        Set(c, WallKind.Steel);
                        continue;
                    }
                    // Детерминированные кластеры. Маршрут и зоны появления остаются свободны.
                    if (road || x < -11 || x > 11 || y < -6 || y > 6)
                    {
                        continue;
                    }
                    int pattern = Mathf.Abs(x * 17 + y * 31 + level * 13) % 11;
                    if (pattern < 4 && ((x + 18) % 5 < 3) && ((y + 12) % 4 < 3))
                    {
                        Set(c, (WallKind)(((x + 18) / 3 + (y + 12) / 2 + level) % 4));
                    }
                }
            }
            // Стальные отражатели по краям маршрута дают доступные углы для выстрелов.
            for (int x = -10; x <= 10; x += 5)
            {
                for (int y = -7; y <= 7; y += 14)
                {
                    var c = new Vector3Int(x, y, 0);
                    if (!NearRoute((Vector2Int) new Vector2Int(x, y) + Vector2.one * 0.5f, 1.7f))
                    {
                        Set(c, WallKind.Steel);
                    }
                }
            }
            // Гарантированная пара дерева позволяет проверить цепное распространение огня.
            bool grove = false;
            for (int x = -9; x <= 8 && !grove; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    Vector2 a = new Vector2(x + 0.5f, y + 0.5f), b = a + Vector2.right;
                    if (NearRoute(a, 1.8f) || NearRoute(b, 1.8f))
                    {
                        continue;
                    }
                    Set(new Vector3Int(x, y, 0), WallKind.Wood);
                    Set(new Vector3Int(x + 1, y, 0), WallKind.Wood);
                    grove = true;
                    break;
                }
            }
            SpawnPoints.Add(new Vector2(-12, 0));
            SpawnPoints.Add(new Vector2(12, -6));
            SpawnPoints.Add(new Vector2(12, 6));
            SpawnPoints.Add(new Vector2(-10, 6));
            walls.CompressBounds();
            wallCollider.ProcessTilemapChanges();
            var finish = Art.Part("Kitten home", transform, "home", Color.white, Vector2.zero,
                                  new Vector2(1.6f, 1.6f), -2);
            finish.transform.position = Route[Route.Length - 1];
            LevelDecorations.Build(this, level);
        }

        public bool OnRoute(Vector2 p, float range) => NearRoute(p, range);

        private bool NearRoute(Vector2 p, float range)
        {
            for (int i = 1; i < Route.Length; i++)
            {
                if (DistanceSegment(p, Route[i - 1], Route[i]) < range)
                {
                    return true;
                }
            }
            return false;
        }

        private static float DistanceSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 d = b - a;
            return Vector2.Distance(p, a + d * Mathf.Clamp01(Vector2.Dot(p - a, d) / d.sqrMagnitude));
        }

        private void Set(Vector3Int c, WallKind kind)
        {
            kinds[c] = kind;
            health[c] = kind == WallKind.Brick ? 2 : 1;
            walls.SetTile(c, tiles[(int)kind]);
        }

        public Vector3Int Cell(Vector2 p) => walls.WorldToCell(p);

        public bool Blocked(Vector2 p) => kinds.ContainsKey(Cell(p));

        public float SlowAt(Vector2 p) => debris.ContainsKey(Cell(p)) ? 0.55f : 1;

        public bool IceAt(Vector2 p) => slippery.Contains(Cell(p));

        public bool Hit(Vector2 before, Vector2 inside)
        {
            return HitCell(Cell(inside));
        }

        public bool HitCell(Vector3Int c)
        {
            if (!kinds.TryGetValue(c, out var k))
            {
                return false;
            }
            if (k == WallKind.Steel)
            {
                return true;
            }
            if (k == WallKind.Wood)
            {
                if (!burning.ContainsKey(c))
                {
                    burning[c] = Time.time;
                }
                return false;
            }
            if (k == WallKind.Ice)
            {
                Remove(c, true);
                return false;
            }
            health[c]--;
            if (health[c] <= 0)
            {
                Remove(c, false);
            }
            return false;
        }

        private void Remove(Vector3Int c, bool ice)
        {
            kinds.Remove(c);
            health.Remove(c);
            burning.Remove(c);
            walls.SetTile(c, null);
            if (ice)
            {
                slippery.Add(c);
                ground.SetTile(c, iceTile);
            }
            else
            {
                debris[c] = Time.time + debrisSeconds;
                ground.SetTile(c, debrisTile);
            }
            wallCollider.ProcessTilemapChanges();
            EventBus.FX(walls.GetCellCenterWorld(c), "hit");
        }

        private float nextTick;

        private void Update()
        {
            if (!GameManager.I.Playing || Time.time < nextTick)
            {
                return;
            }
            nextTick = Time.time + 0.25f;
            foreach (var c in new List<Vector3Int>(burning.Keys))
            {
                if (!burning.TryGetValue(c, out float start))
                {
                    continue;
                }
                EventBus.FX(walls.GetCellCenterWorld(c), "fire");
                if (Time.time - start >= 0.8f)
                {
                    foreach (var d in directions)
                    {
                        var n = c + d;
                        if (kinds.TryGetValue(n, out var k) && k == WallKind.Wood && !burning.ContainsKey(n))
                        {
                            burning[n] = Time.time;
                        }
                    }
                }
                if (Time.time - start >= burnSeconds)
                {
                    Remove(c, false);
                }
            }
            foreach (var c in new List<Vector3Int>(debris.Keys))
                if (Time.time >= debris[c])
                {
                    debris.Remove(c);
                    ground.SetTile(c, NearRoute(walls.GetCellCenterWorld(c), 1.45f) ? roadTile : floorTile);
                }
        }

        public void Explode(Vector2 p, float radius)
        {
            foreach (var c in new List<Vector3Int>(kinds.Keys))
                if (Vector2.Distance(p, walls.GetCellCenterWorld(c)) < radius)
                {
                    HitCell(c);
                    HitCell(c);
                }
        }

        public Vector2 Navigate(Vector2 from, Vector2 to)
        {
            Vector3Int start = Cell(from), goal = Cell(to);
            if (start == goal)
            {
                return (to - from).normalized;
            }
            var queue = new Queue<Vector3Int>();
            var previous = new Dictionary<Vector3Int, Vector3Int>();
            queue.Enqueue(start);
            previous[start] = start;
            while (queue.Count > 0 && previous.Count < 600)
            {
                var c = queue.Dequeue();
                if (c == goal)
                {
                    break;
                }
                foreach (var d in directions)
                {
                    var n = c + d;
                    if (n.x < -13 || n.x > 12 || n.y < -7 || n.y > 6 || kinds.ContainsKey(n) ||
                        previous.ContainsKey(n))
                    {
                        continue;
                    }
                    previous[n] = c;
                    queue.Enqueue(n);
                }
            }
            if (!previous.ContainsKey(goal))
            {
                return Vector2.zero;
            }
            var next = goal;
            while (previous[next] != start && next != start)
            {
                next = previous[next];
            }
            return ((Vector2)walls.GetCellCenterWorld(next) - from).normalized;
        }

        public Vector2 FreePoint()
        {
            for (int i = 0; i < 100; i++)
            {
                Vector2 p = new Vector2(Random.Range(-12, 13) + 0.5f, Random.Range(-7, 7) + 0.5f);
                if (!Blocked(p) && Physics2D.OverlapCircle(p, 0.65f) == null)
                {
                    return p;
                }
            }
            return Route[0] + Vector2.right * 2;
        }

        private void OnDestroy()
        {
            foreach (var t in tiles)
                if (t)
                {
                    Destroy(t);
                }
            if (floorTile)
            {
                Destroy(floorTile);
            }
            if (roadTile)
            {
                Destroy(roadTile);
            }
            if (iceTile)
            {
                Destroy(iceTile);
            }
            if (debrisTile)
            {
                Destroy(debrisTile);
            }
        }
    }
}
