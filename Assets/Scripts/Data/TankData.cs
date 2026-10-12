using UnityEngine;

namespace PrettyTanks
{
    [CreateAssetMenu(menuName = "Pretty Tanks/Tank")]
    public sealed class TankData : ScriptableObject
    {
        [SerializeField]
        private float health = 120;
        [SerializeField]
        private float speed = 4.3f;
        [SerializeField]
        private float turnSpeed = 230;
        [SerializeField]
        private float frontArmor = 0.18f;
        [SerializeField]
        private Color color = new Color(1, 0.42f, 0.69f);

        public float Health => health;

        public float Speed => speed;

        public float TurnSpeed => turnSpeed;

        public float FrontArmor => frontArmor;

        public Color Color => color;
    }
}
