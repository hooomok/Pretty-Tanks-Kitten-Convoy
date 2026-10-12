using UnityEngine;

namespace PrettyTanks
{
    public enum AIState
    {
        Patrol,
        Attack,
        Reload
    }

    /// <summary>Общее поведение танков с отдельными правилами для пяти типов врагов.</summary>
    public sealed class AIBrain : MonoBehaviour
    {
        private static readonly Vector2[] FaceNormals = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        [SerializeField]
        private EnemyData data;
        private TankController tank;
        private DifficultyProfile difficulty;
        private SpriteRenderer[] renderers;
        private Vector2 movement;
        private Vector2 aim;
        private float nextThink;
        private float nextMinion;
        private float phaseOffset;

        public AIState State { get; private set; }

        public void Init(EnemyData config, DifficultyProfile profile)
        {
            data = config;
            difficulty = profile;
            tank = GetComponent<TankController>();
            tank.SpeedOverride = data.Speed * difficulty.SpeedMultiplier;
            tank.Health.Init(data.Health * difficulty.HealthMultiplier);
            tank.OutgoingDamageMultiplier = data.DamageMultiplier * difficulty.DamageMultiplier;
            tank.ArmorOverride = data.Kind == EnemyKind.Scooter ? 0.7f : difficulty.FrontalDamageFraction;
            if (data.Kind == EnemyKind.Mother)
            {
                transform.localScale = Vector3.one * 1.25f;
            }

            renderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer part in renderers)
            {
                if (part.gameObject.name == "Hull")
                {
                    part.color = data.Color;
                }
            }

            nextMinion = Time.time + data.SpawnDelay * difficulty.ReloadMultiplier;
            phaseOffset = Random.Range(0f, 3f);
        }

        private void Update()
        {
            GameManager game = GameManager.I;
            if (!game.Playing || !tank || !game.Truck || !game.Player)
            {
                return;
            }

            UpdateGhostPhase();
            Vector2 target =
                data.Kind == EnemyKind.Sniper ? game.Player.transform.position : game.Truck.transform.position;
            if (Time.time >= nextThink)
            {
                nextThink = Time.time + 0.3f;
                UpdateDecision(target);
            }

            tank.Drive(movement, aim);
            TryAttack();
            TryCreateMinion();
        }

        private void UpdateGhostPhase()
        {
            if (data.Kind != EnemyKind.Ghost)
            {
                return;
            }

            tank.Phased = (Time.time + phaseOffset) % data.GhostInterval < 2f;
            foreach (SpriteRenderer part in renderers)
            {
                Color color = part.color;
                color.a = tank.Phased ? 0.3f : 1f;
                part.color = color;
            }
        }

        private void UpdateDecision(Vector2 target)
        {
            float distance = Vector2.Distance(transform.position, target);
            if (distance > data.Range)
            {
                State = AIState.Patrol;
            }
            else
            {
                State = tank.Weapon.Ready ? AIState.Attack : AIState.Reload;
            }

            movement = data.Kind == EnemyKind.Sniper ? Vector2.zero
                                                     : GameManager.I.Level.Navigate(transform.position, target);
            aim = target;
            if (data.Kind == EnemyKind.Ricochet && FindBounce(target, out Vector2 reflectionPoint))
            {
                aim = reflectionPoint;
            }
            if (data.Kind != EnemyKind.Scooter && distance < 4f && State != AIState.Patrol)
            {
                movement = Vector2.zero;
            }
        }

        private void TryAttack()
        {
            if (State != AIState.Attack || data.Kind == EnemyKind.Scooter || tank.Phased)
            {
                return;
            }
            Vector2 direction = aim - (Vector2)tank.Turret.position;
            if (Vector2.Angle(tank.Turret.up, direction) >= 9f)
            {
                return;
            }

            float interval = data.ShotDelay * difficulty.ReloadMultiplier;
            tank.Weapon.TryFire(interval / tank.Weapon.Cooldown);
        }

        private void TryCreateMinion()
        {
            if (data.Kind != EnemyKind.Mother || Time.time < nextMinion)
            {
                return;
            }
            nextMinion = Time.time + data.SpawnDelay * difficulty.ReloadMultiplier;
            GameManager.I.Waves.SpawnMinion(transform.position);
        }

        private bool FindBounce(Vector2 target, out Vector2 reflectionPoint)
        {
            Vector2 start = tank.Turret.position;
            reflectionPoint = target;
            float bestLength = float.MaxValue;

            // Зеркальная цель превращает два участка с рикошетом в одну прямую.
            foreach (Vector3Int cell in GameManager.I.Level.SteelCells)
            {
                Vector2 center = new Vector2(cell.x + 0.5f, cell.y + 0.5f);
                foreach (Vector2 normal in FaceNormals)
                {
                    Vector2 face = center + normal * 0.5f;
                    if (!TryReflection(start, target, face, normal, out Vector2 point))
                    {
                        continue;
                    }
                    float length = Vector2.Distance(start, point) + Vector2.Distance(point, target);
                    if (length > data.RicochetSearchDistance || length >= bestLength)
                    {
                        continue;
                    }
                    bestLength = length;
                    reflectionPoint = point;
                }
            }
            return bestLength < float.MaxValue;
        }

        private bool TryReflection(Vector2 start, Vector2 target, Vector2 face, Vector2 normal, out Vector2 point)
        {
            point = default;
            if (Vector2.Dot(start - face, normal) <= 0.1f || Vector2.Dot(target - face, normal) <= 0.1f)
            {
                return false;
            }

            Vector2 mirror = target - 2f * Vector2.Dot(target - face, normal) * normal;
            Vector2 ray = mirror - start;
            float denominator = Vector2.Dot(ray, normal);
            if (Mathf.Abs(denominator) < 0.001f)
            {
                return false;
            }
            float fraction = Vector2.Dot(face - start, normal) / denominator;
            if (fraction <= 0f || fraction >= 1f)
            {
                return false;
            }

            point = start + ray * fraction;
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            if (Mathf.Abs(Vector2.Dot(point - face, tangent)) > 0.47f)
            {
                return false;
            }
            return ClearPath(start, point + normal * 0.05f) && ClearPath(point + normal * 0.08f, target);
        }

        private static bool ClearPath(Vector2 start, Vector2 end)
        {
            float length = Vector2.Distance(start, end);
            for (float distance = 0.4f; distance < length - 0.2f; distance += 0.25f)
            {
                Vector2 sample = Vector2.Lerp(start, end, distance / length);
                if (GameManager.I.Level.Blocked(sample))
                {
                    return false;
                }
            }
            return true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (data.Kind != EnemyKind.Scooter || !GameManager.I.Playing)
            {
                return;
            }
            EscortTruck truck = collision.collider.GetComponent<EscortTruck>();
            if (!truck)
            {
                return;
            }
            truck.Health.Damage(data.RamDamage * tank.OutgoingDamageMultiplier);
            tank.Health.Damage(10000f);
        }
    }
}
