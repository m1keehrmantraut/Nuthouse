using UnityEngine;
using UnityEngine.InputSystem;

namespace Nuthouse.Input
{
    public sealed class InputReader
    {
        private readonly InputActionAsset asset;
        private readonly InputActionMap gameplay;
        private readonly InputConfig config;
        private float attackDownTime = -1f;

        public Vector2 MoveAxis { get; private set; }
        public bool RunHeld { get; private set; }
        public bool CrouchHeld { get; private set; }
        public bool JumpHeld { get; private set; }

        public event System.Action<InputAction> ActionPressed;

        public InputReader(InputConfig config = null)
        {
            this.config = config;
            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "PlayerInputActions";

            gameplay = asset.AddActionMap("Gameplay");

            var move = gameplay.AddAction("Move", InputActionType.Value, null, null, null, null, "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            gameplay.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            gameplay.AddAction("Run", InputActionType.Button, "<Keyboard>/leftShift");

            var crouch = gameplay.AddAction("Crouch", InputActionType.Button, "<Keyboard>/s");
            crouch.AddBinding("<Keyboard>/leftCtrl");

            var attack = gameplay.AddAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            attack.AddBinding("<Keyboard>/j");

            var throwAction = gameplay.AddAction("Throw", InputActionType.Button, "<Mouse>/rightButton");
            throwAction.AddBinding("<Keyboard>/k");

            gameplay.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            gameplay.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");

            BindCallbacks(gameplay);
            gameplay.Enable();
        }

        private void BindCallbacks(InputActionMap map)
        {
            map["Move"].performed += c => MoveAxis = c.ReadValue<Vector2>();
            map["Move"].canceled += _ => MoveAxis = Vector2.zero;
            map["Run"].started += _ => RunHeld = true;
            map["Run"].canceled += _ => RunHeld = false;
            map["Crouch"].started += _ => CrouchHeld = true;
            map["Crouch"].canceled += _ => CrouchHeld = false;

            map["Jump"].started += _ => { JumpHeld = true; ActionPressed?.Invoke(InputAction.Jump); };
            map["Jump"].canceled += _ => JumpHeld = false;

            // Hold-charge: тап Attack → лёгкая; удержание ≥ heavyAttackHold → тяжёлая.
            // Решение принимается по отпускании клавиши (charge/release).
            map["Attack"].started  += _ => attackDownTime = Time.time;
            map["Attack"].canceled += _ =>
            {
                float hold = attackDownTime > 0f ? Time.time - attackDownTime : 0f;
                attackDownTime = -1f;
                float threshold = config != null ? config.heavyAttackHold : 0.25f;
                ActionPressed?.Invoke(hold >= threshold ? InputAction.HeavyAttack : InputAction.Attack);
            };
            map["Throw"].performed += _ => ActionPressed?.Invoke(InputAction.Throw);
            map["Interact"].performed += _ => ActionPressed?.Invoke(InputAction.Interact);
            map["Pause"].performed += _ => ActionPressed?.Invoke(InputAction.Pause);
        }

        public void SetMapEnabled(string mapName, bool enabled)
        {
            var map = asset.FindActionMap(mapName);
            if (map == null) return;
            if (enabled) map.Enable();
            else map.Disable();
        }

        public void Dispose()
        {
            if (asset != null)
                UnityEngine.Object.Destroy(asset);
        }
    }
}
