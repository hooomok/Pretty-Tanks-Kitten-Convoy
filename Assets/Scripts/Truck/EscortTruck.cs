using UnityEngine;

namespace PrettyTanks
{
    [RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem))]
    public sealed class EscortTruck : MonoBehaviour
    {
        [SerializeField]
        private float speed = 0.64f;
        [SerializeField]
        private float maximumHP = 240;
        [SerializeField]
        private Vector2[] waypoints;
        private Rigidbody2D body;
        private int index;
        private float total;
        private float travelled;

        public HealthSystem Health { get; private set; }

        public float Progress => total > 0? Mathf.Clamp01(travelled / total) : 0;

        public void Init(Vector2[] path, int level)
        {
            waypoints = path;
            var config = Resources.Load<TruckData>("Truck");
            if (config)
            {
                speed = config.SpeedForLevel(level);
                maximumHP = config.Health;
            }
            else
            {
                speed += level * 0.035f;
            }
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Health = GetComponent<HealthSystem>();
            Health.Init(maximumHP);
            Health.Died += () => GameManager.I.Finish(false, "Котята украдены");
            float old = maximumHP;
            Health.Changed += (hp, max) =>
            {
                if (hp < old)
                {
                    EventBus.FX(transform.position, "meow");
                }
                old = hp;
            };
            transform.position = path[0];
            index = 1;
            for (int i = 1; i < path.Length; i++)
            {
                total += Vector2.Distance(path[i - 1], path[i]);
            }
        }

        private void FixedUpdate()
        {
            if (!GameManager.I.Playing || Health.Dead || index >= waypoints.Length)
            {
                return;
            }
            Vector2 before = body.position,
                    after = Vector2.MoveTowards(before, waypoints[index], speed * Time.fixedDeltaTime);
            body.MovePosition(after);
            Vector2 d = after - before;
            if (d.sqrMagnitude > 0)
            {
                body.MoveRotation(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90);
                travelled += d.magnitude;
            }
            if (Vector2.Distance(after, waypoints[index]) < 0.03f)
            {
                index++;
                if (index >= waypoints.Length)
                {
                    GameManager.I.Finish(true, "Все котята дома!");
                }
            }
        }
    }
}
