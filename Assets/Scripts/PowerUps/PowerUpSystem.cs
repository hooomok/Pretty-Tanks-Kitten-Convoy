using System.Collections.Generic;
using UnityEngine;

namespace PrettyTanks
{
    public enum PowerKind
    {
        Triple,
        Laser,
        Bomb,
        Speed,
        Shield,
        Regeneration,
        Repair
    }

    public sealed class PowerUpSystem : MonoBehaviour
    {
        [SerializeField]
        private float duration = 12;
        [SerializeField]
        private float shieldDuration = 5;
        [SerializeField]
        private float regenPerSecond = 3;
        [SerializeField]
        private float bombRadius = 4;
        [SerializeField]
        private float bombDamage = 80;
        private readonly Dictionary<PowerKind, float> ends = new Dictionary<PowerKind, float>();
        private TankController tank;

        public bool Active(PowerKind kind) => ends.TryGetValue(kind, out float end) && Time.time < end;

        public float Seconds(PowerKind kind) => ends.TryGetValue(kind, out float end)
                                                    ? Mathf.Max(0, end - Time.time)
                                                    : 0;

        private void Awake()
        {
            tank = GetComponent<TankController>();
            var config = Resources.Load<PowerUpConfig>("PowerUps");
            if (config)
            {
                duration = config.Duration;
                shieldDuration = config.ShieldDuration;
                regenPerSecond = config.RegenPerSecond;
                bombRadius = config.BombRadius;
                bombDamage = config.BombDamage;
            }
        }

        public void Give(PowerKind kind)
        {
            if (kind == PowerKind.Repair)
            {
                if (!tank.Modules.RepairRandom())
                {
                    tank.Health.Heal(25);
                }
            }
            else
            {
                // Повторное усиление обновляет таймер, не сокращая уже оставшееся время.
                float end = Time.time + (kind == PowerKind.Shield ? shieldDuration : duration);
                ends[kind] = ends.TryGetValue(kind, out float current) ? Mathf.Max(current, end) : end;
            }
            EventBus.Say("Получено: " + Label(kind));
            EventBus.FX(transform.position, "repair");
        }

        private void Update()
        {
            if (tank && tank.Player && GameManager.I.Playing && Active(PowerKind.Regeneration))
            {
                GameManager.I.Truck.Health.Heal(regenPerSecond * Time.deltaTime);
            }
        }

        public void Bomb()
        {
            if (!Active(PowerKind.Bomb) || tank.Repairing)
            {
                return;
            }
            ends[PowerKind.Bomb] = 0;
            var seen = new HashSet<TankController>();
            foreach (var c in Physics2D.OverlapCircleAll(transform.position, bombRadius))
            {
                var other = c.GetComponentInParent<TankController>();
                if (other && other.Player != tank.Player && seen.Add(other))
                {
                    other.TakeHit(bombDamage, (other.transform.position - transform.position).normalized, null, 0);
                }
            }
            GameManager.I.Level.Explode(transform.position, bombRadius);
            EventBus.FX(transform.position, "bomb");
        }

        public static string Label(PowerKind k) => new[] {
            "тройной выстрел",     "лазер",        "бомба [ПРОБЕЛ]", "скорость + ∞ рикошетов", "щит",
            "регенерация фургона", "ремонт модуля"
        }[(int)k];
    }
}
