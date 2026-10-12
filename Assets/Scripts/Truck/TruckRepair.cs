using UnityEngine;

namespace PrettyTanks
{
    public sealed class TruckRepair : MonoBehaviour
    {
        [SerializeField]
        private float range = 2.1f;
        [SerializeField]
        private float duration = 2.5f;
        [SerializeField]
        private float cooldown = 10;
        [SerializeField]
        private float healFraction = 0.3f;
        private TankController tank;
        private float started;
        private float nextUse;
        private bool busy;

        public float Progress => busy ? Mathf.Clamp01((Time.time - started) / duration) : 0;

        public float Cooldown => Mathf.Max(0, nextUse - Time.time);

        public bool Busy => busy;

        private void Start()
        {
            tank = GetComponent<TankController>();
        }

        public void Begin()
        {
            if (busy || Time.time < nextUse)
            {
                return;
            }
            var truck = GameManager.I.Truck;
            if (Vector2.Distance(transform.position, truck.transform.position) > range)
            {
                EventBus.Say("Подъедь ближе к фургону и нажми E");
                return;
            }
            if (truck.Health.Current >= truck.Health.Maximum)
            {
                EventBus.Say("Фургон пока не требует ремонта");
                return;
            }
            busy = true;
            started = Time.time;
            tank.Repairing = true;
            EventBus.Say("Разбираем запчасти… Стрельба недоступна");
        }

        private void Update()
        {
            if (!busy || !GameManager.I.Playing)
            {
                return;
            }
            if (Vector2.Distance(transform.position, GameManager.I.Truck.transform.position) > range + 0.5f)
            {
                Cancel();
                EventBus.Say("Ремонт прерван: фургон слишком далеко");
                return;
            }
            if (Time.time - started >= duration)
            {
                GameManager.I.Truck.Health.Heal(GameManager.I.Truck.Health.Maximum * healFraction);
                tank.Modules.DisableRandom();
                nextUse = Time.time + cooldown;
                Cancel();
                EventBus.FX(transform.position, "repair");
            }
        }

        private void Cancel()
        {
            busy = false;
            if (tank)
            {
                tank.Repairing = false;
            }
        }

        private void OnDisable()
        {
            Cancel();
        }
    }
}
