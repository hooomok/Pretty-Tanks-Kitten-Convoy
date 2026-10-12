using UnityEngine;

namespace PrettyTanks
{
    public enum ModuleKind
    {
        LeftTrack,
        RightTrack,
        Cannon,
        Turret,
        Engine
    }

    [CreateAssetMenu(menuName = "Pretty Tanks/Modules")]
    public sealed class ModuleData : ScriptableObject
    {
        [SerializeField]
        private float trackHP = 36;
        [SerializeField]
        private float cannonHP = 30;
        [SerializeField]
        private float turretHP = 40;
        [SerializeField]
        private float engineHP = 35;
        [SerializeField]
        private float disableTime = 5;

        public float DisableTime => disableTime;

        public float HP(ModuleKind k) => k == ModuleKind.LeftTrack || k == ModuleKind.RightTrack ? trackHP
                                         : k == ModuleKind.Cannon                                ? cannonHP
                                         : k == ModuleKind.Turret                                ? turretHP
                                                                                                 : engineHP;
    }
}
