using Nuthouse.Combat;
using Nuthouse.Core.Events;
using Nuthouse.Core.StateMachine;
using Nuthouse.Player.States;
using UnityEngine;

namespace Nuthouse.Player.Presentation
{
    public sealed class PlayerAnimatorDriver : MonoBehaviour
    {
        private Animator animator;
        private PlayerContext ctx;

        public void Init(PlayerContext context)
        {
            ctx = context;
            animator = GetComponentInChildren<Animator>();
            if (ctx != null && ctx.StateMachine != null)
                ctx.StateMachine.StateChanged += OnStateChanged;

            MovementEvents.Jumped += OnJumped;
            CombatEvents.AttackStarted += OnAttackStarted;
            CombatEvents.ThrowStarted += OnThrowStarted;
            DamageEvents.DamageTaken += OnDamageTaken;
        }

        private void OnDestroy()
        {
            if (ctx != null && ctx.StateMachine != null)
                ctx.StateMachine.StateChanged -= OnStateChanged;

            MovementEvents.Jumped -= OnJumped;
            CombatEvents.AttackStarted -= OnAttackStarted;
            CombatEvents.ThrowStarted -= OnThrowStarted;
            DamageEvents.DamageTaken -= OnDamageTaken;
        }

        private void OnStateChanged(IState prev, IState next)
        {
            if (animator == null) return;

            bool running = next is RunState;
            bool crouching = next is CrouchState;
            animator.SetBool("Running", running);
            animator.SetBool("Crouching", crouching);

            if (next is DeadState)
                animator.SetBool("IsDead", true);
            else
                animator.SetBool("IsDead", false);
        }

        private void OnAttackStarted(AttackDefinition def)
        {
            if (animator == null) return;
            animator.SetTrigger(def != null ? def.animTrigger : "Fight");
        }

        private void OnThrowStarted(AxeThrowDefinition def)
        {
            if (animator == null) return;
            animator.SetTrigger(def != null ? def.animTrigger : "ThrowAxe");
        }

        private void OnDamageTaken(DamageInfo info, GameObject target)
        {
            if (animator == null) return;
            animator.SetTrigger("Hit");
        }

        private void OnJumped()
        {
            if (animator == null) return;
        }

        private void LateUpdate()
        {
            if (animator == null || ctx == null || ctx.Motor == null) return;
            animator.SetFloat("Speed", Mathf.Abs(ctx.Motor.Velocity.x));
            animator.SetFloat("VerticalVelocity", ctx.Motor.VerticalVelocity);
            animator.SetBool("Grounded", ctx.Ground != null && ctx.Ground.IsGrounded);
        }
    }
}