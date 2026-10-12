using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Хранит повреждения пяти модулей и отдельные таймеры временного отключения.</summary>
    public sealed class ModuleSystem : MonoBehaviour
    {
        private const int ModuleCount = 5;
        private static readonly string[] Labels = { "Левая гусеница", "Правая гусеница", "Пушка", "Башня",
                                                    "Двигатель" };

        [SerializeField]
        private ModuleData data;
        private readonly float[] hp = new float[ModuleCount];
        private readonly float[] disabledUntil = new float[ModuleCount];
        private readonly SpriteRenderer[] parts = new SpriteRenderer[ModuleCount];
        private readonly Color[] colors = new Color[ModuleCount];
        private bool engineSeized;
        private float nextSmoke;

        public event Action Changed;

        public ModuleData Data => data;

        public float EngineFactor
        {
            get {
                if (Time.time < disabledUntil[(int)ModuleKind.Engine] || engineSeized)
                {
                    return 0f;
                }
                return Broken(ModuleKind.Engine) ? 0.3f : 1f;
            }
        }

        public void Init(ModuleData config)
        {
            data = config;
            for (int i = 0; i < ModuleCount; i++)
            {
                hp[i] = data.HP((ModuleKind)i);
            }
        }

        public void Register(ModuleKind kind, SpriteRenderer part)
        {
            parts[(int)kind] = part;
            colors[(int)kind] = part.color;
        }

        public float Fraction(ModuleKind kind) => hp[(int)kind] / data.HP(kind);

        public bool Broken(ModuleKind kind) => hp[(int)kind] <= 0f;

        public bool Offline(ModuleKind kind) => Broken(kind) || Time.time < disabledUntil[(int)kind];

        public static string Label(ModuleKind kind) => Labels[(int)kind];

        public void Hit(ModuleKind kind, float amount)
        {
            // Повторное попадание в уже сломанный двигатель полностью блокирует движение.
            if (kind == ModuleKind.Engine && Broken(kind))
            {
                engineSeized = true;
            }
            hp[(int)kind] = Mathf.Max(0f, hp[(int)kind] - amount);
            Refresh(kind);
            Changed?.Invoke();
        }

        public void DisableRandom()
        {
            ModuleKind kind = RandomSystem();
            disabledUntil[(int)kind] = Time.time + data.DisableTime;
            EventBus.Say("Ремонт завершён. Временно отключено: " + Label(kind));
            Changed?.Invoke();
        }

        private static ModuleKind RandomSystem()
        {
            // Сначала выбирается система: две гусеницы не удваивают её вероятность.
            switch (UnityEngine.Random.Range(0, 4))
            {
            case 0:
                return ModuleKind.Cannon;
            case 1:
                return ModuleKind.Engine;
            case 2:
                return UnityEngine.Random.value < 0.5f ? ModuleKind.LeftTrack : ModuleKind.RightTrack;
            default:
                return ModuleKind.Turret;
            }
        }

        public bool RepairRandom()
        {
            List<int> candidates = FindDamagedModules(true);
            if (candidates.Count == 0)
            {
                candidates = FindDamagedModules(false);
            }
            if (candidates.Count == 0)
            {
                return false;
            }

            int index = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            ModuleKind kind = (ModuleKind)index;
            hp[index] = data.HP(kind);
            disabledUntil[index] = 0f;
            if (kind == ModuleKind.Engine)
            {
                engineSeized = false;
            }
            Refresh(kind);
            Changed?.Invoke();
            return true;
        }

        private List<int> FindDamagedModules(bool onlyBroken)
        {
            var candidates = new List<int>();
            for (int i = 0; i < ModuleCount; i++)
            {
                bool suitable = onlyBroken ? hp[i] <= 0f : hp[i] < data.HP((ModuleKind)i);
                if (suitable)
                {
                    candidates.Add(i);
                }
            }
            return candidates;
        }

        private void Refresh(ModuleKind kind)
        {
            int index = (int)kind;
            if (parts[index])
            {
                parts[index].color = Color.Lerp(new Color(0.23f, 0.18f, 0.28f), colors[index], Fraction(kind));
            }
        }

        private void Update()
        {
            if (Time.time < nextSmoke)
            {
                return;
            }
            nextSmoke = Time.time + 0.65f;
            for (int i = 0; i < ModuleCount; i++)
            {
                if (hp[i] < data.HP((ModuleKind)i) * 0.5f && parts[i])
                {
                    EventBus.FX(parts[i].transform.position, "smoke");
                }
            }
        }
    }
}
