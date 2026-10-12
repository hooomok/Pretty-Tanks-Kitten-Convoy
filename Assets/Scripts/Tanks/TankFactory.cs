using UnityEngine;

namespace PrettyTanks
{
    public static class TankFactory
    {
        public static TankController Create(bool player, Vector2 position, Transform parent)
        {
            var go = new GameObject(player ? "Ribbon — player" : "Enemy tank");
            go.transform.SetParent(parent);
            go.transform.position = position;
            var config = Resources.Load<TankData>(player ? "PlayerTank" : "EnemyTank");
            var hull =
                Art.Part("Hull", go.transform, "hull", config.Color, Vector2.zero, new Vector2(0.83f, 1.12f), 5);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.64f, 0.87f);
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<HealthSystem>();
            go.AddComponent<ModuleSystem>();
            var tank = go.AddComponent<TankController>();
            var turret = Art.Part("Turret", go.transform, "round", config.Color * new Color(0.95f, 0.9f, 0.95f),
                                  new Vector2(0, 0.12f), Vector2.one * 0.6f, 8);
            // Масштаб башни остаётся единичным, масштаб графики вынесен в дочерний объект.
            var pivot = new GameObject("Turret pivot").transform;
            pivot.SetParent(go.transform, false);
            pivot.localPosition = new Vector3(0, 0.12f, 0);
            turret.transform.SetParent(pivot);
            turret.transform.localPosition = Vector3.zero;
            tank.Init(config, Resources.Load<ModuleData>("Modules"), player, pivot);
            tank.Weapon.Init(Resources.Load<WeaponData>("Cannon"));
            AddModule(tank, ModuleKind.LeftTrack, "track", new Vector2(-0.49f, 0), new Vector2(0.27f, 1.22f),
                      go.transform, new Color(0.42f, 0.32f, 0.49f), 6);
            AddModule(tank, ModuleKind.RightTrack, "track", new Vector2(0.49f, 0), new Vector2(0.27f, 1.22f),
                      go.transform, new Color(0.42f, 0.32f, 0.49f), 6);
            AddModule(tank, ModuleKind.Engine, "engine", new Vector2(0, -0.48f), new Vector2(0.43f, 0.28f),
                      go.transform, new Color(0.85f, 0.7f, 0.8f), 7);
            AddModule(tank, ModuleKind.Cannon, "barrel", new Vector2(0, 0.47f), new Vector2(0.22f, 0.75f), pivot,
                      config.Color, 9);
            var hit = turret.gameObject.AddComponent<BoxCollider2D>();
            hit.size = new Vector2(0.7f, 0.6f);
            hit.isTrigger = true;
            turret.gameObject.AddComponent<ModuleHitbox>().Init(tank, ModuleKind.Turret);
            tank.Modules.Register(ModuleKind.Turret, turret);
            if (player)
            {
                Art.Part("Bow", pivot, "bow", new Color(1, 0.35f, 0.61f), new Vector2(0, -0.02f),
                         new Vector2(0.63f, 0.35f), 10);
                go.AddComponent<ShieldVisual>().Init(tank.Power);
                go.AddComponent<PlayerInputDriver>();
                go.AddComponent<TruckRepair>();
            }
            else
            {
                Art.Part("Kitten ears", pivot, "ears", new Color(0.98f, 0.83f, 0.94f), new Vector2(0, 0.2f),
                         new Vector2(0.6f, 0.45f), 9);
            }
            return tank;
        }

        private static void AddModule(TankController tank, ModuleKind kind, string shape, Vector2 pos,
                                      Vector2 size, Transform parent, Color color, int order)
        {
            var s = Art.Part(kind.ToString(), parent, shape, color, pos, size, order);
            var c = s.gameObject.AddComponent<BoxCollider2D>();
            c.isTrigger = true;
            c.size = Vector2.one * 0.9f;
            s.gameObject.AddComponent<ModuleHitbox>().Init(tank, kind);
            tank.Modules.Register(kind, s);
        }

        public static EscortTruck CreateTruck(Transform parent, Vector2[] route, int level)
        {
            var go = new GameObject("Kitten convoy");
            go.transform.SetParent(parent);
            Art.Part("Van", go.transform, "van", new Color(1, 0.62f, 0.77f), Vector2.zero, new Vector2(1.35f, 2),
                     6);
            for (int i = 0; i < 3; i++)
            {
                Art.Part("Kitten " + i, go.transform, "cat",
                         i == 1 ? new Color(1, 0.9f, 0.65f) : new Color(0.97f, 0.95f, 1),
                         new Vector2((i - 1) * 0.32f, -0.2f), Vector2.one * 0.56f, 7);
            }
            var c = go.AddComponent<BoxCollider2D>();
            c.size = new Vector2(1.2f, 1.8f);
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<HealthSystem>();
            var truck = go.AddComponent<EscortTruck>();
            truck.Init(route, level);
            return truck;
        }
    }
}
