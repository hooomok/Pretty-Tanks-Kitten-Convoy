using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Сохраняемые настройки оформления меню.</summary>
    [CreateAssetMenu(menuName = "Pretty Tanks/UI Appearance")]
    public sealed class UIAppearanceConfig : ScriptableObject
    {
        [SerializeField, Range(0f, 32f)]
        [InspectorName("CornerRadius")]
        [Tooltip("Радиус углов превью маршрутов. 0 — прямые углы. Единицы интерфейса 1280×800.")]
        private float cornerRadius = 14f;

        [Header("Компактные полоски HP")]
        [SerializeField] private Vector2 worldHealthSize = new Vector2(36f, 4f);
        [SerializeField] private Vector2 hudHealthScale = new Vector2(0.6f, 0.5f);
        [SerializeField, Range(6f, 20f)] private float healthIconSize = 10f;
        public Vector2 WorldHealthSize => new Vector2(Mathf.Max(12f, worldHealthSize.x), Mathf.Max(2f, worldHealthSize.y));
        public Vector2 HudHealthScale => new Vector2(Mathf.Clamp(hudHealthScale.x, 0.2f, 1f), Mathf.Clamp(hudHealthScale.y, 0.2f, 1f));
        public float HealthIconSize => Mathf.Clamp(healthIconSize, 6f, 20f);

        public float CornerRadius => Mathf.Max(0f, cornerRadius);
    }
}
