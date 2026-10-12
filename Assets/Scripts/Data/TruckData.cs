using UnityEngine;

namespace PrettyTanks
{
    [CreateAssetMenu(menuName = "Pretty Tanks/Truck")]
    public sealed class TruckData : ScriptableObject
    {
        [SerializeField, Min(1f)] private float health = 240f;
        [SerializeField, Min(0.1f)] private float speed = 0.64f;
        [SerializeField, Min(0f)] private float speedGrowthPerLevel = 0.035f;
        public float Health => Mathf.Max(1f, health);
        public float SpeedForLevel(int level) => Mathf.Max(0.1f, speed + speedGrowthPerLevel * level);
    }
}
