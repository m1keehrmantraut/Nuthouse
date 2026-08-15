using Nuthouse.Combat;
using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using Nuthouse.Player.Combat;
using Nuthouse.Player.Movement;
using Nuthouse.Player.Presentation;
using UnityEngine;

namespace Nuthouse.Player
{
    public sealed class PlayerContext
    {
        public PlayerFacade Facade;
        public CharacterMotor Motor;
        public GroundDetector Ground;
        public FacingController Facing;
        public InputReader Input;
        public InputBuffer Buffer;
        public PlayerStateMachine StateMachine;
        public StateRegistry States;
        public PlayerCombat Combat;
        public Nuthouse.Health.Health Health;
        public Nuthouse.Health.Stamina Stamina;
        public InteractController Interact;
        public PlayerAnimatorDriver Animator;
        public Transform HitboxRoot;
        public MovementConfig MovementConfig;
        public JumpConfig JumpConfig;
        public Nuthouse.Health.HealthConfig HealthConfig;

        public HurtRequest? PendingHurt;
        public bool ControlEnabled = true;

        // Флаг «следующая ближняя атака — тяжёлая», выставляется стейтом перед
        // переходом в AttackState и сбрасывается в Enter. Расширение hold-charge
        // (нет в 05 §6) — проходит через контекст, не через события (02 §3).
        public bool PendingHeavyAttack;
    }
}
