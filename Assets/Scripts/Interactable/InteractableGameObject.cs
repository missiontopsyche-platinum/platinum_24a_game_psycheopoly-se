using UnityEngine;

namespace Interactable
{
    [RequireComponent(typeof(Collider))]
    public abstract class InteractableGameObject : MonoBehaviour, IClickable, IHoverable
    {
        public abstract void OnLeftClick();
        public abstract void OnRightClick();
        public abstract void OnHoverEnter();
        public abstract void OnHoverExit();
        public abstract void OnHover();
    }
}