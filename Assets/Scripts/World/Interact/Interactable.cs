using Nuthouse.Player;
using UnityEngine;

namespace Nuthouse.World.Interact
{
    public abstract class Interactable : MonoBehaviour
    {
        public float radius = 1f;
        public bool IsAvailable { get; protected set; } = true;

        public abstract void Interact(PlayerFacade player);
    }
}