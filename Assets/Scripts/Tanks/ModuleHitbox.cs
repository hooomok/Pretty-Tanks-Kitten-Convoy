using UnityEngine;

namespace PrettyTanks
{
    public sealed class ModuleHitbox : MonoBehaviour
    {
        [SerializeField]
        private ModuleKind kind;

        public ModuleKind Kind => kind;

        public TankController Owner { get; private set; }

        public void Init(TankController owner, ModuleKind value)
        {
            Owner = owner;
            kind = value;
        }
    }
}
