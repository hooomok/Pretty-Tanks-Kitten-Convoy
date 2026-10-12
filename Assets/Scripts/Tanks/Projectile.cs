using UnityEngine;

namespace PrettyTanks
{
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField]
        private float skin = 0.025f;
        private TankController owner;
        private Vector2 direction;
        private WeaponData data;
        private float expires;
        private float damage;
        private int used;
        private bool infinite;
        private bool playerOwner;

        public int Remaining => infinite ? -1 : Mathf.Max(0, data.Bounces - used);

        public void Init(TankController source, Vector2 vector, WeaponData config, bool unlimited)
        {
            damage = config.Damage * source.OutgoingDamageMultiplier;
            owner = source;
            playerOwner = source.Player;
            direction = vector.normalized;
            data = config;
            infinite = unlimited;
            expires = Time.time + data.Lifetime;
            var s = gameObject.AddComponent<SpriteRenderer>();
            s.sprite = Art.Sprite("heart");
            s.color = source.Player ? new Color(1, 0.3f, 0.6f) : new Color(0.55f, 0.3f, 0.85f);
            s.sortingOrder = 20;
            var mat = Resources.Load<Material>("ConfettiMaterial");
            if (mat)
            {
                s.sharedMaterial = mat;
            }
            // Размер сердечка-снаряда не зависит от разрешения PNG.
            transform.localScale = Art.ScaleToSize(s.sprite, Vector2.one * 0.23f);
        }

        private void FixedUpdate()
        {
            if (!GameManager.I.Playing)
            {
                return;
            }
            if (Time.time >= expires)
            {
                Destroy(gameObject);
                return;
            }
            float travel = data.Speed * Time.fixedDeltaTime;
            Vector2 point = transform.position;
            // Несколько столкновений за физический кадр исключают пролёт сквозь углы.
            for (int step = 0; step < 12 && travel > 0.0001f; step++)
            {
                RaycastHit2D chosen = default;
                foreach (var hit in Physics2D.RaycastAll(point, direction, travel))
                {
                    var tank = hit.collider.GetComponentInParent<TankController>();
                    if (tank)
                    {
                        if (tank.Phased) continue;
                        if (tank == owner)
                        {
                            // Своя пуля опасна игроку после первого отражения.
                            // Враги сохраняют прежнюю защиту от собственных пуль.
                            if (!playerOwner || used == 0) continue;
                        }
                        else if (tank.Player == playerOwner)
                        {
                            continue;
                        }
                    }
                    if (hit.collider.isTrigger && hit.collider.GetComponent<ModuleHitbox>() == null)
                    {
                        continue;
                    }
                    if (playerOwner && hit.collider.GetComponent<EscortTruck>())
                    {
                        continue;
                    }
                    if (!tank && hit.distance < skin * 0.2f)
                    {
                        continue;
                    }
                    chosen = hit;
                    break;
                }
                if (!chosen.collider)
                {
                    point += direction * travel;
                    break;
                }
                travel -= chosen.distance;
                var target = chosen.collider.GetComponentInParent<TankController>();
                if (target)
                {
                    var module = chosen.collider.GetComponent<ModuleHitbox>();
                    if (!module)
                    {
                        foreach (var candidate in Physics2D.RaycastAll(chosen.point + direction * 0.01f, direction,
                                                                       0.7f))
                        {
                            var box = candidate.collider.GetComponent<ModuleHitbox>();
                            if (box && box.Owner == target)
                            {
                                module = box;
                                break;
                            }
                        }
                    }
                    target.TakeHit(damage, direction, module, used);
                    Destroy(gameObject);
                    return;
                }
                var truck = chosen.collider.GetComponent<EscortTruck>();
                if (truck)
                {
                    truck.Health.Damage(damage * 0.75f);
                    Destroy(gameObject);
                    return;
                }
                bool reflect =
                    GameManager.I.Level.Hit(chosen.point - direction * 0.001f, chosen.point + direction * 0.04f);
                if (!reflect)
                {
                    EventBus.FX(chosen.point, "hit");
                    Destroy(gameObject);
                    return;
                }
                if (!infinite && used >= data.Bounces)
                {
                    Destroy(gameObject);
                    return;
                }
                used++;
                direction = Vector2.Reflect(direction, chosen.normal).normalized;
                point = chosen.point + chosen.normal * skin;
                travel = Mathf.Max(0, travel - skin);
                EventBus.FX(point, "bounce");
            }
            transform.position = point;
        }
    }
}
