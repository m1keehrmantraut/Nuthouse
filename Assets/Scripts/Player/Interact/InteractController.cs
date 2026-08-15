using Nuthouse.Input;
using UnityEngine;
using Nuthouse.World.Interact;

namespace Nuthouse.Player
{
    public sealed class InteractController
    {
        private readonly PlayerContext ctx;
        private readonly LayerMask interactableLayer;
        private readonly float radius = 1.5f;

        public InteractController(PlayerContext context, LayerMask layer)
        {
            ctx = context;
            interactableLayer = layer;
        }

        public void Tick()
        {
            if (!ctx.ControlEnabled) return;
            if (ctx.Facade == null) return;
            if (!ctx.Buffer.TryConsume(InputAction.Interact)) return;

            Vector2 origin = ctx.Facade.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, interactableLayer);

            Interactable best = null;
            float bestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent<Interactable>(out var interactable)) continue;
                if (!interactable.IsAvailable) continue;

                float dist = ((Vector2)interactable.transform.position - origin).sqrMagnitude
                             - interactable.radius * interactable.radius;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = interactable;
                }
            }

            if (best != null)
                best.Interact(ctx.Facade);
        }
    }
}