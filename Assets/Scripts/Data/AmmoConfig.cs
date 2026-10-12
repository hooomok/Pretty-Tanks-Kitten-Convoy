using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Постоянные настройки боезапаса игрока. Счётчик хранится в WeaponSystem.</summary>
    [CreateAssetMenu(menuName = "Pretty Tanks/Ammo Config")]
    public sealed class AmmoConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int maxAmmo = 30;
        [SerializeField, Min(0)] private int startingAmmo = 30;
        [Tooltip("Секунды игрового времени между пополнениями.")]
        [SerializeField, Min(0.1f)] private float refillInterval = 1.5f;
        [SerializeField, Min(1)] private int refillAmount = 1;
        public int MaxAmmo => Mathf.Max(1, maxAmmo);
        public int StartingAmmo => Mathf.Clamp(startingAmmo, 0, MaxAmmo);
        public float RefillInterval => Mathf.Max(0.1f, refillInterval);
        public int RefillAmount => Mathf.Max(1, refillAmount);
    }
}
