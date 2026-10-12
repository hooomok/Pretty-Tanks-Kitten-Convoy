using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace PrettyTanks
{
    // Инструменты проверки доступны только в редакторе, в релизную сборку горячие клавиши не попадут.
    public sealed class DevTools : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Update()
        {
            var g = GameManager.I;
            var k = Keyboard.current;
            if (k == null || !g || !g.Playing || !g.Player)
            {
                return;
            }
            if (k.f1Key.wasPressedThisFrame)
            {
                g.Truck.Health.Damage(40);
            }
            if (k.f2Key.wasPressedThisFrame)
            {
                g.Player.Modules.Hit(ModuleKind.LeftTrack, 100);
            }
            if (k.f3Key.wasPressedThisFrame)
            {
                g.Player.Modules.Hit(ModuleKind.Engine, 100);
            }
            if (k.f4Key.wasPressedThisFrame)
            {
                g.Player.Modules.Hit(ModuleKind.Cannon, 100);
            }
            if (k.f5Key.wasPressedThisFrame)
            {
                g.Player.Modules.Hit(ModuleKind.Turret, 100);
            }
            if (k.f6Key.wasPressedThisFrame)
            {
                g.Player.Modules.Hit(ModuleKind.RightTrack, 100);
            }
            if (k.f7Key.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Repair);
            }
            if (k.f8Key.wasPressedThisFrame)
            {
                for (int i = 0; i < 6; i++)
                {
                    g.Player.Power.Give((PowerKind)i);
                }
            }
            if (k.f9Key.wasPressedThisFrame)
            {
                g.Finish(true, "Тестовый финиш");
            }
            if (k.f10Key.wasPressedThisFrame)
            {
                g.Truck.Health.Damage(10000);
            }
            if (k.digit6Key.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Triple);
            }
            if (k.digit7Key.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Laser);
            }
            if (k.digit8Key.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Bomb);
            }
            if (k.digit9Key.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Speed);
            }
            if (k.digit0Key.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Shield);
            }
            if (k.minusKey.wasPressedThisFrame)
            {
                g.Player.Power.Give(PowerKind.Regeneration);
            }
            if (k.digit1Key.wasPressedThisFrame)
            {
                g.Waves.Spawn(EnemyKind.Scooter, g.Level.FreePoint());
            }
            if (k.digit2Key.wasPressedThisFrame)
            {
                g.Waves.Spawn(EnemyKind.Sniper, g.Level.FreePoint());
            }
            if (k.digit3Key.wasPressedThisFrame)
            {
                g.Waves.Spawn(EnemyKind.Ricochet, g.Level.FreePoint());
            }
            if (k.digit4Key.wasPressedThisFrame)
            {
                g.Waves.Spawn(EnemyKind.Mother, g.Level.FreePoint());
            }
            if (k.digit5Key.wasPressedThisFrame)
            {
                g.Waves.Spawn(EnemyKind.Ghost, g.Level.FreePoint());
            }
        }
#endif
    }
}
