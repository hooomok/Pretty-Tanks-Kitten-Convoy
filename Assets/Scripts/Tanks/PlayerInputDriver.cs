using UnityEngine;
using UnityEngine.InputSystem;

namespace PrettyTanks
{
    public sealed class PlayerInputDriver : MonoBehaviour
    {
        private InputAction move;
        private InputAction fire;
        private InputAction repair;
        private InputAction bomb;
        private TankController tank;

        private void Awake()
        {
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            fire = new InputAction("Fire", binding: "<Mouse>/leftButton");
            repair = new InputAction("Repair", binding: "<Keyboard>/e");
            bomb = new InputAction("Bomb", binding: "<Keyboard>/space");
        }

        private void OnEnable()
        {
            move.Enable();
            fire.Enable();
            repair.Enable();
            bomb.Enable();
        }

        private void Start()
        {
            tank = GetComponent<TankController>();
        }

        private void Update()
        {
            if (!tank || !GameManager.I.AcceptsPlayerInput)
            {
                return;
            }
            Vector2 target = tank.transform.position + Vector3.up;
            if (Mouse.current != null)
            {
                target = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
            tank.Drive(move.ReadValue<Vector2>(), target);
            if (fire.IsPressed())
            {
                tank.Weapon.TryFire();
            }
            if (repair.WasPressedThisFrame())
            {
                GetComponent<TruckRepair>().Begin();
            }
            if (bomb.WasPressedThisFrame())
            {
                tank.Power.Bomb();
            }
        }

        private void OnDisable()
        {
            move.Disable();
            fire.Disable();
            repair.Disable();
            bomb.Disable();
        }

        private void OnDestroy()
        {
            move.Dispose();
            fire.Dispose();
            repair.Dispose();
            bomb.Dispose();
        }
    }
}
