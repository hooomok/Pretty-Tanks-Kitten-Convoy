using UnityEngine;

namespace PrettyTanks
{
    public sealed class PowerUpCrate : MonoBehaviour
    {
        [SerializeField]
        private PowerKind kind;
        private float expires;
        private bool taken;

        public void Init(PowerKind value)
        {
            kind = value;
            expires = Time.time + 35;
        }

        private void Update()
        {
            if (Time.time > expires)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (taken || !GameManager.I.Playing)
            {
                return;
            }
            var tank = other.GetComponentInParent<TankController>();
            if (!tank || !tank.Player)
            {
                return;
            }
            taken = true;
            tank.Power.Give(kind);
            Destroy(gameObject);
        }
    }
}
