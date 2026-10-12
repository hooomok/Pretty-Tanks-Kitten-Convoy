using UnityEngine;

namespace PrettyTanks
{
    public enum EnemyKind
    {
        Scooter,
        Sniper,
        Ricochet,
        Mother,
        Ghost
    }

    [CreateAssetMenu(menuName = "Pretty Tanks/Enemy")]
    public sealed class EnemyData : ScriptableObject
    {
        [SerializeField]
        private EnemyKind kind;
        [SerializeField]
        private float health = 65;
        [SerializeField]
        private float speed = 2;
        [SerializeField]
        private float range = 9;
        [SerializeField]
        private float shotDelay = 1.7f;
        [SerializeField]
        private float spawnDelay = 9;
        [SerializeField]
        private float ghostInterval = 7;
        [SerializeField]
        private Color color = new Color(0.65f, 0.55f, 0.88f);

        [SerializeField, Min(0f)] private float damageMultiplier = 1.15f;
        [SerializeField, Min(0f)] private float ramDamage = 25f;
        [SerializeField, Min(1f)] private float minionHealth = 28.8f;
        [SerializeField, Min(1f)]
        [Tooltip("Максимальная суммарная длина пути с одним отражением, которую рассматривает ИИ.")]
        private float ricochetSearchDistance = 23f;
        public float RicochetSearchDistance => Mathf.Max(1f, ricochetSearchDistance);
        public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);
        public float RamDamage => Mathf.Max(0f, ramDamage);
        public float MinionHealth => Mathf.Max(1f, minionHealth);

        public EnemyKind Kind => kind;

        public float Health => health;

        public float Speed => speed;

        public float Range => range;

        public float ShotDelay => shotDelay;

        public float SpawnDelay => spawnDelay;

        public float GhostInterval => ghostInterval;

        public Color Color => color;
    }
}
