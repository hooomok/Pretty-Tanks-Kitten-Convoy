using UnityEngine;

namespace PrettyTanks
{
    [CreateAssetMenu(menuName = "Pretty Tanks/Weapon")]
    public sealed class WeaponData : ScriptableObject
    {
        [SerializeField]
        private float damage = 24;
        [SerializeField]
        private float cooldown = 0.42f;
        [SerializeField]
        private float speed = 15;
        [SerializeField]
        private float lifetime = 5;
        [SerializeField, Range(0, 8)]
        private int bounces = 3;

        public float Damage => damage;

        public float Cooldown => cooldown;

        public float Speed => speed;

        public float Lifetime => lifetime;

        public int Bounces => bounces;
    }
}
