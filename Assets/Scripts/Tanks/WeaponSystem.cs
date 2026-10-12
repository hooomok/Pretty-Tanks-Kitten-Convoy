using System.Collections.Generic;
using UnityEngine;

namespace PrettyTanks
{
    public sealed class WeaponSystem : MonoBehaviour
    {
        [SerializeField]
        private WeaponData data;
        private TankController tank;
        private float nextShot;

        private int ammo;
        private int maxAmmo = 30;
        private float refillInterval = 1.5f;
        private int refillAmount = 1;
        private float refillTimer;
        private float nextEmptyMessage;

        public int Ammo => ammo;
        public int MaxAmmo => maxAmmo;

        public Projectile Latest { get; private set; }

        public float Cooldown => data.Cooldown;

        private float ShotDamage => data.Damage * tank.OutgoingDamageMultiplier;

        public bool Ready => Time.time >= nextShot;

        public void Init(WeaponData config)
        {
            data = config;
            tank = GetComponent<TankController>();
            // Ассет не меняется во время боя: параметры копируются при старте.
            var ammoConfig = Resources.Load<AmmoConfig>("Ammo");
            maxAmmo = ammoConfig ? ammoConfig.MaxAmmo : 30;
            ammo = ammoConfig ? ammoConfig.StartingAmmo : maxAmmo;
            refillInterval = ammoConfig ? ammoConfig.RefillInterval : 1.5f;
            refillAmount = ammoConfig ? ammoConfig.RefillAmount : 1;
            refillTimer = 0f;
            nextShot = 0f;
            nextEmptyMessage = 0f;
        }

        private void Update()
        {
            // Пополнение действует только у игрока и только во время активного боя.
            var game = GameManager.I;
            if (!tank || !tank.Player || !game || !game.Playing) return;
            if (ammo >= maxAmmo)
            {
                refillTimer = 0f;
                return;
            }
            refillTimer += Time.deltaTime;
            while (refillTimer >= refillInterval && ammo < maxAmmo)
            {
                refillTimer -= refillInterval;
                ammo += Mathf.Min(refillAmount, maxAmmo - ammo);
            }
            // При полном запасе нельзя накопить пополнения на будущее.
            if (ammo >= maxAmmo) refillTimer = 0f;
        }

        public bool TryFire(float cooldownMultiplier = 1)
        {
            if (!Ready || !tank || tank.Repairing || tank.Modules.Offline(ModuleKind.Cannon) ||
                !GameManager.I || !GameManager.I.Playing)
            {
                return false;
            }
            bool laser = tank.Power.Active(PowerKind.Laser);
            bool triple = !laser && tank.Power.Active(PowerKind.Triple);
            int cost = triple ? 3 : 1;
            if (tank.Player)
            {
                if (ammo < cost)
                {
                    if (Time.time >= nextEmptyMessage)
                    {
                        EventBus.Say(triple
                            ? "Для тройного выстрела нужно 3 патрона. Дождись пополнения."
                            : "Патроны закончились. Дождись пополнения.");
                        nextEmptyMessage = Time.time + 2f;
                    }
                    return false;
                }
                // Тратим патроны только после всех проверок возможности выстрелить.
                ammo -= cost;
            }
            nextShot = Time.time + data.Cooldown * cooldownMultiplier;
            Vector2 origin = tank.Turret.position + tank.Turret.up * 0.1f;
            if (laser)
            {
                Laser(origin, tank.Turret.up);
                return true;
            }
            Spawn(origin, tank.Turret.up);
            if (triple)
            {
                Spawn(origin, Quaternion.Euler(0, 0, -13) * tank.Turret.up);
                Spawn(origin, Quaternion.Euler(0, 0, 13) * tank.Turret.up);
            }
            EventBus.FX(origin, "shot");
            return true;
        }

        private void Spawn(Vector2 origin, Vector2 direction)
        {
            var go = new GameObject("Heart shell");
            go.transform.position = origin;
            go.transform.SetParent(GameManager.I.World);
            Latest = go.AddComponent<Projectile>();
            Latest.Init(tank, direction, data, tank.Power.Active(PowerKind.Speed));
        }

        private void Laser(Vector2 origin, Vector2 direction)
        {
            var hitTanks = new HashSet<TankController>();
            bool truckHit = false;
            foreach (var hit in Physics2D.RaycastAll(origin, direction, 24))
            {
                var other = hit.collider.GetComponentInParent<TankController>();
                if (other && other != tank && other.Player != tank.Player && hitTanks.Add(other))
                {
                    other.TakeHit(ShotDamage, direction, hit.collider.GetComponent<ModuleHitbox>(), 0);
                }
                var truck = hit.collider.GetComponent<EscortTruck>();
                if (truck && !tank.Player && !truckHit)
                {
                    truck.Health.Damage(ShotDamage);
                    truckHit = true;
                }
            }
            // Луч пробивает препятствия, повреждая разрушаемые клетки один раз за выстрел.
            var cells = new HashSet<Vector3Int>();
            for (float d = 0; d < 24; d += 0.35f)
            {
                Vector2 p = origin + direction * d;
                var cell = GameManager.I.Level.Cell(p);
                if (cells.Add(cell))
                {
                    GameManager.I.Level.HitCell(cell);
                }
            }
            Vfx.Beam(origin, origin + direction * 24, new Color(1, 0.4f, 0.8f));
            EventBus.FX(origin, "shot");
        }
    }
}
