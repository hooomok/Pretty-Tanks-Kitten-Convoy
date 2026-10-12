using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Пузырь читает существующий таймер щита. Сам не меняет защиту и урон.</summary>
    public sealed class ShieldVisual : MonoBehaviour
    {
        [SerializeField] private ShieldVisualConfig config;
        private PowerUpSystem power;
        private GameObject visual;
        private SpriteRenderer bubble;
        private SpriteRenderer[] sparkles;
        private Material material;
        private float visibility;

        public void Init(PowerUpSystem source)
        {
            power = source;
            config = Resources.Load<ShieldVisualConfig>("ShieldVisual");
            if (!config)
            {
                Debug.LogError("Не найден Assets/Resources/ShieldVisual.asset", this);
                enabled = false;
                return;
            }

            visual = new GameObject("ShieldVisual");
            visual.transform.SetParent(transform, false);
            bubble = Art.Part("Bubble", visual.transform, "round", Color.white, Vector2.zero, Vector2.one * 2f, 15);
            Shader shader = Resources.Load<Shader>("ShieldBubble");
            if (shader)
            {
                material = new Material(shader);
                bubble.sharedMaterial = material;
            }
            else
            {
                Debug.LogError("Не найден Assets/Resources/ShieldBubble.shader", this);
            }
            sparkles = new SpriteRenderer[3];
            for (int i = 0; i < sparkles.Length; i++)
            {
                sparkles[i] = Art.Part("Shield sparkle", visual.transform, "star", Color.white,
                                      Vector2.zero, Vector2.one * 0.13f, 16);
            }
            visual.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!visual || !power || !config) return;
            bool active = power.Active(PowerKind.Shield);
            // Повторный подбор не выключает объект и не перезапускает фазу пульсации.
            visibility = active ? 1f : Mathf.MoveTowards(visibility, 0f, Time.deltaTime / config.FadeOutDuration);
            if (visibility <= 0f)
            {
                visual.SetActive(false);
                return;
            }
            if (!visual.activeSelf) visual.SetActive(true);

            float pulse = 1f + 0.025f * (1f - Mathf.Cos(Time.time * Mathf.PI * 2f * config.PulseSpeed));
            visual.transform.localScale = Vector3.one * (config.Radius * pulse);
            visual.transform.rotation = Quaternion.identity;
            Color color = config.ShieldColor;
            color.a = config.ShieldAlpha * visibility;
            if (material) material.SetColor("_ShieldColor", color);
            else bubble.color = color;
            for (int i = 0; i < sparkles.Length; i++)
            {
                float angle = Time.time * 0.8f + i * Mathf.PI * 2f / sparkles.Length;
                sparkles[i].transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.94f;
                sparkles[i].color = new Color(1f, 0.88f, 0.97f, 0.8f * visibility);
            }
        }

        private void OnDisable()
        {
            if (visual) visual.SetActive(false);
        }

        private void OnDestroy()
        {
            if (material) Destroy(material);
        }
    }
}
