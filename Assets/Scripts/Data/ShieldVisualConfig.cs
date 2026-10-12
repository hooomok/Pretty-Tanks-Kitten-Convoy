using UnityEngine;

namespace PrettyTanks
{
    [CreateAssetMenu(menuName = "Pretty Tanks/Shield Visual")]
    public sealed class ShieldVisualConfig : ScriptableObject
    {
        [SerializeField, InspectorName("ShieldColor")] private Color shieldColor = new Color(1f, 0.55f, 0.82f);
        [SerializeField, Range(0f, 1f), InspectorName("ShieldAlpha")] private float shieldAlpha = 0.42f;
        [SerializeField, Min(0f), InspectorName("PulseSpeed")]
        [Tooltip("Число полных пульсаций за секунду.")]
        private float pulseSpeed = 1f;
        [SerializeField, Min(0.01f), InspectorName("FadeOutDuration")] private float fadeOutDuration = 0.3f;
        [SerializeField, Min(0.2f)] private float radius = 1.05f;
        public Color ShieldColor => shieldColor;
        public float ShieldAlpha => Mathf.Clamp01(shieldAlpha);
        public float PulseSpeed => Mathf.Max(0f, pulseSpeed);
        public float FadeOutDuration => Mathf.Max(0.01f, fadeOutDuration);
        public float Radius => Mathf.Max(0.2f, radius);
    }
}
