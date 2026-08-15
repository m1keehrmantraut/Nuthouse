using Nuthouse.Combat.Projectiles;
using Nuthouse.Player;
using UnityEngine;

namespace Nuthouse.World.Interact
{
    public sealed class AxeInteractable : Interactable
    {
        private AxeProjectile axe;
        private bool consumed;

        public void Setup(AxeProjectile projectile)
        {
            axe = projectile;
            radius = 1.2f;
            IsAvailable = true;
        }

        public override void Interact(PlayerFacade player)
        {
            if (consumed) return;
            if (!IsAvailable) return;
            consumed = true;
            IsAvailable = false;
            player.ReturnAxe(1);
            if (axe != null) axe.PickUp();
        }
    }
}