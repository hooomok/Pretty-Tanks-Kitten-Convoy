using System;
using UnityEngine;

namespace PrettyTanks
{
    public sealed class HealthSystem : MonoBehaviour
    {
        [SerializeField]
        private float maximum = 100;

        public float Current { get; private set; }

        public float Maximum => maximum;

        public bool Dead => Current <= 0;

        public bool Invulnerable { get; set; }

        public event Action<float, float> Changed;

        public event Action Died;

        public void Init(float hp)
        {
            maximum = hp;
            Current = hp;
            Changed?.Invoke(Current, maximum);
        }

        public void Damage(float amount)
        {
            if (Dead || Invulnerable || amount <= 0)
            {
                return;
            }
            Current = Mathf.Max(0, Current - amount);
            Changed?.Invoke(Current, maximum);
            if (Dead)
            {
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (Dead)
            {
                return;
            }
            Current = Mathf.Min(maximum, Current + Mathf.Max(0, amount));
            Changed?.Invoke(Current, maximum);
        }
    }
}
