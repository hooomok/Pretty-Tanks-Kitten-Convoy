using UnityEngine;

namespace PrettyTanks
{
    [CreateAssetMenu(menuName = "Pretty Tanks/Power Ups")]
    public sealed class PowerUpConfig : ScriptableObject
    {
        [Header("Появление подарков")]
        [SerializeField, Min(1f)] private float crateInterval = 15f;
        [SerializeField, Min(0f)] private float firstCrateDelay = 8f;
        [SerializeField] private bool startingRepair = true;
        [SerializeField] private PowerKind[] allowedTypes = {
            PowerKind.Triple, PowerKind.Laser, PowerKind.Bomb, PowerKind.Speed,
            PowerKind.Shield, PowerKind.Regeneration, PowerKind.Repair
        };
        [Header("Длительность и эффект")]
        [SerializeField, Min(0.1f)] private float duration = 12f;
        [SerializeField, Min(0.1f)] private float shieldDuration = 5f;
        [SerializeField, Min(0f)] private float regenPerSecond = 3f;
        [SerializeField, Min(0.1f)] private float bombRadius = 4f;
        [SerializeField, Min(0f)] private float bombDamage = 80f;
        public bool StartingRepair => startingRepair;
        public float CrateInterval => Mathf.Max(1f, crateInterval);
        public float FirstCrateDelay => Mathf.Max(0f, firstCrateDelay);
        public float Duration => Mathf.Max(0.1f, duration);
        public float ShieldDuration => Mathf.Max(0.1f, shieldDuration);
        public float RegenPerSecond => Mathf.Max(0f, regenPerSecond);
        public float BombRadius => Mathf.Max(0.1f, bombRadius);
        public float BombDamage => Mathf.Max(0f, bombDamage);
        public bool TryPick(out PowerKind kind)
        {
            kind = PowerKind.Repair;
            if (allowedTypes == null || allowedTypes.Length == 0) return false;
            kind = allowedTypes[Random.Range(0, allowedTypes.Length)];
            return true;
        }
    }
}
