using Nuthouse.Combat;
using Nuthouse.Core.Events;
using Nuthouse.Core.StateMachine;
using Nuthouse.Health;
using Nuthouse.Input;
using Nuthouse.Player.Combat;
using Nuthouse.Player.Movement;
using Nuthouse.Player.Presentation;
using Nuthouse.Player.States;
using UnityEngine;

namespace Nuthouse.Player
{
    public sealed class PlayerFacade : MonoBehaviour
    {
        [Header("Конфиги (SO)")]
        [SerializeField] private MovementConfig movementConfig;
        [SerializeField] private JumpConfig jumpConfig;
        [SerializeField] private HealthConfig healthConfig;
        [SerializeField] private StaminaConfig staminaConfig;
        [SerializeField] private InputConfig inputConfig;

        [Header("Бой (SO)")]
        [SerializeField] private AttackDefinition primaryAttack;
        [SerializeField] private AttackDefinition heavyAttack;
        [SerializeField] private AxeThrowDefinition axeThrow;

        [Header("Слои/точки префаба")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform ceilingCheck;
        [SerializeField] private Transform hitboxRoot;
        [SerializeField] private Hurtbox hurtbox;

        private PlayerContext ctx;
        private bool controlEnabled = true;

        private void Awake()
        {
            ctx = new PlayerContext { Facade = this, MovementConfig = movementConfig, JumpConfig = jumpConfig, HealthConfig = healthConfig };

            ctx.Ground = new GroundDetector(groundCheck, movementConfig, groundLayer, ceilingCheck);
            ctx.Facing = new FacingController(transform);
            ctx.Motor = new CharacterMotor(GetComponent<Rigidbody2D>(), movementConfig, jumpConfig);
            ctx.Input = new InputReader(inputConfig);
            ctx.Buffer = new InputBuffer(inputConfig);
            ctx.Health = new Nuthouse.Health.Health(healthConfig, ctx);
            ctx.Stamina = new Nuthouse.Health.Stamina(staminaConfig);
            ctx.Combat = new PlayerCombat(ctx, primaryAttack, axeThrow);
            ctx.Interact = new InteractController(ctx, interactableLayer);
            ctx.HitboxRoot = hitboxRoot;

            if (hurtbox != null) hurtbox.Bind(ctx.Health);

            ctx.States = new StateRegistry();
            RegisterStates();
            ctx.StateMachine = new PlayerStateMachine();

            ctx.Animator = GetComponent<PlayerAnimatorDriver>();
            if (ctx.Animator != null) ctx.Animator.Init(ctx);

            ctx.Input.ActionPressed += OnActionPressed;
            ctx.StateMachine.ChangeState(ctx.States.Get<IdleState>());
        }

        private void RegisterStates()
        {
            ctx.States.Register(new IdleState(ctx));
            ctx.States.Register(new RunState(ctx));
            ctx.States.Register(new CrouchState(ctx));
            ctx.States.Register(new JumpState(ctx));
            ctx.States.Register(new FallState(ctx));
            ctx.States.Register(new AttackState(ctx));
            ctx.States.Register(new AxeThrowState(ctx));
            ctx.States.Register(new HurtState(ctx));
            ctx.States.Register(new DeadState(ctx));
        }

        private void OnActionPressed(InputAction action)
        {
            if (action == InputAction.Pause)
            {
                GameEvents.PublishPauseRequested();
                return;
            }
            if (!controlEnabled) return;
            if (action == InputAction.Throw && !ctx.Combat.HasAxe) return;
            ctx.Buffer.Add(action);
        }

        private void Update()
        {
            ctx.ControlEnabled = controlEnabled;
            ctx.Buffer.Tick();

            if (ctx.Health != null && ctx.Health.IsDead)
            {
                if (ctx.StateMachine.Current is not DeadState)
                    ctx.StateMachine.ChangeState(ctx.States.Get<DeadState>());
            }
            else if (ctx.PendingHurt.HasValue)
            {
                ctx.StateMachine.ChangeState(ctx.States.Get<HurtState>());
            }
            else
            {
                ctx.StateMachine.Tick(Time.deltaTime);
            }

            ctx.Interact.Tick();
            ctx.Stamina.Tick(Time.deltaTime);
            ctx.Health.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            ctx.Ground.UpdateGround();
            ctx.StateMachine.FixedTick(Time.fixedDeltaTime);
        }

        public void SetControlEnabled(bool enabled)
        {
            controlEnabled = enabled;
            if (!enabled) ctx.Buffer.Clear();
        }

        public void ReturnAxe(int count) => ctx.Combat.ReturnAxe(count);

        public AttackDefinition PrimaryAttack => primaryAttack;
        public AttackDefinition HeavyAttack => heavyAttack;
        public AxeThrowDefinition AxeThrow => axeThrow;

        public float StaminaNormalized => ctx != null && ctx.Stamina != null ? ctx.Stamina.Normalized : 0f;

        public void ApplyDamage(in DamageInfo info)
        {
            if (ctx.Health != null)
                ctx.Health.TakeDamage(in info);
        }

        private void OnDestroy()
        {
            if (ctx?.Input != null)
            {
                ctx.Input.ActionPressed -= OnActionPressed;
                ctx.Input.Dispose();
            }
        }
    }
}
