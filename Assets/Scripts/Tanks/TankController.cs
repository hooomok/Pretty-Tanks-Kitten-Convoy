using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Движение корпуса и башни, применение брони и последствий повреждений.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem), typeof(ModuleSystem))]
    public sealed class TankController : MonoBehaviour
    {
        [SerializeField]
        private TankData data;
        [SerializeField]
        private Transform turret;
        [SerializeField]
        private bool player;

        private Rigidbody2D body;
        private Vector2 drive;
        private Vector2 aim;
        private bool dead;

        public HealthSystem Health { get; private set; }

        public ModuleSystem Modules { get; private set; }

        public WeaponSystem Weapon { get; private set; }

        public PowerUpSystem Power { get; private set; }

        public bool Player => player;

        public Transform Turret => turret;

        public bool Repairing { get; set; }

        public bool Phased { get; set; }

        public float SpeedOverride { get; set; } = -1f;

        public float ArmorOverride { get; set; } = -1f;

        public float OutgoingDamageMultiplier { get; set; } = 1f;

        public void Init(TankData config, ModuleData modules, bool isPlayer, Transform top)
        {
            data = config;
            player = isPlayer;
            turret = top;
            ConfigureBody();

            Health = GetComponent<HealthSystem>();
            Health.Init(data.Health);
            Health.Died += Die;
            Modules = GetComponent<ModuleSystem>();
            Modules.Init(modules);
            Power = gameObject.AddComponent<PowerUpSystem>();
            Weapon = gameObject.AddComponent<WeaponSystem>();
        }

        private void ConfigureBody()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void Drive(Vector2 movement, Vector2 target)
        {
            drive = Vector2.ClampMagnitude(movement, 1f);
            aim = target;
        }

        private void FixedUpdate()
        {
            if (dead || !GameManager.I.Playing)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            bool leftOffline = Modules.Offline(ModuleKind.LeftTrack);
            bool rightOffline = Modules.Offline(ModuleKind.RightTrack);
            RotateHull(leftOffline, rightOffline);
            MoveHull(leftOffline, rightOffline);
            AimTurret();
        }

        private void RotateHull(bool leftOffline, bool rightOffline)
        {
            float rotation;
            if (leftOffline != rightOffline)
            {
                // Одна повреждённая гусеница постоянно разворачивает корпус.
                float direction = leftOffline ? 1f : -1f;
                rotation = body.rotation + direction * 130f * Time.fixedDeltaTime;
            }
            else
            {
                float targetAngle = drive.sqrMagnitude > 0.01f
                                        ? Mathf.Atan2(drive.y, drive.x) * Mathf.Rad2Deg - 90f
                                        : body.rotation;
                rotation =
                    Mathf.MoveTowardsAngle(body.rotation, targetAngle, data.TurnSpeed * Time.fixedDeltaTime);
            }
            body.MoveRotation(rotation);
        }

        private void MoveHull(bool leftOffline, bool rightOffline)
        {
            float engineFactor = leftOffline && rightOffline ? 0f : Modules.EngineFactor;
            float baseSpeed = SpeedOverride >= 0f ? SpeedOverride : data.Speed;
            float bonus = Power.Active(PowerKind.Speed) ? 1.65f : 1f;
            float surfaceFactor = GameManager.I.Level.SlowAt(body.position);
            Vector2 direction = leftOffline != rightOffline ? (Vector2)transform.up * drive.magnitude : drive;
            Vector2 desiredVelocity = direction * baseSpeed * engineFactor * surfaceFactor * bonus;
            if (Repairing)
            {
                desiredVelocity = Vector2.zero;
            }

            bool onIce = GameManager.I.Level.IceAt(body.position);
            body.linearVelocity =
                onIce ? Vector2.Lerp(body.linearVelocity, desiredVelocity, Time.fixedDeltaTime * 2f)
                      : desiredVelocity;
        }

        private void AimTurret()
        {
            if (Modules.Offline(ModuleKind.Turret))
            {
                turret.localRotation = Quaternion.identity;
                return;
            }

            Vector2 direction = aim - (Vector2)turret.position;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            turret.rotation = Quaternion.RotateTowards(turret.rotation, Quaternion.Euler(0f, 0f, angle),
                                                       360f * Time.fixedDeltaTime);
        }

        public void TakeHit(float damage, Vector2 incoming, ModuleHitbox hit, int ricochets)
        {
            if (dead || Phased || Power.Active(PowerKind.Shield))
            {
                return;
            }

            // Даже после рикошета фронтальная броня действует, если снаряд пришёл спереди.
            bool frontalHit = Vector2.Dot(-incoming.normalized, transform.up) > 0.55f;
            float armorFactor = 1f;
            if (frontalHit && !player)
            {
                armorFactor = ArmorOverride >= 0f ? ArmorOverride : data.FrontArmor;
            }

            float actualDamage = damage * armorFactor;
            if (hit)
            {
                Modules.Hit(hit.Kind, actualDamage);
            }
            float hullFraction = hit != null && hit.Owner == this ? 0.65f : 1f;
            Health.Damage(actualDamage * hullFraction);
            EventBus.FX(transform.position, "hit");
        }

        private void Die()
        {
            if (dead)
            {
                return;
            }
            dead = true;
            EventBus.FX(transform.position, "burst");
            if (player)
            {
                GameManager.I.Finish(false, "Твой танк уничтожен");
            }
            Destroy(gameObject);
        }
    }
}
