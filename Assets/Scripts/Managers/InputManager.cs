using Interactable;
using Logging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;

        private IHoverable currentHovered;

        private void Start()
        {
            if (!mainCamera)
                mainCamera = Camera.main;

            if (mainCamera) return;
            
            Logging.Logger.Error("InputManager.Start",
                "Unable to find camera in scene!",
                LogCategory.UI);
            Destroy(this);
        }
        
        private void Update()
        {
            if (Mouse.current == null) return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hitData;

            if (Physics.Raycast(ray, out hitData, Mathf.Infinity))
            {
                HandleHover(hitData.collider.GetComponent<IHoverable>());
                
                if (Mouse.current.leftButton.wasPressedThisFrame) // left click
                    HandleClick(hitData.collider.GetComponent<IClickable>(), true);
                else if (Mouse.current.rightButton.wasPressedThisFrame) // right click
                    HandleClick(hitData.collider.GetComponent<IClickable>(), false);
            }
            else
            {
                HandleHover(null); // clear the current hoverable when nothing is hit
            }
        }

        private void HandleHover(IHoverable hoverable)
        {
            if (hoverable != null) 
            {
                if (hoverable != currentHovered)
                {
                    currentHovered?.OnHoverExit();
                    currentHovered = hoverable;
                    currentHovered?.OnHoverEnter();
                }
                else
                {
                    currentHovered?.OnHover();
                }
            }
            else
            {
                if (currentHovered != null)
                {
                    currentHovered.OnHoverExit();
                    currentHovered = null;
                }
            }
        }

        private void HandleClick(IClickable clickable, bool isLeftClick)
        {
            if (clickable != null)
            {
                if (isLeftClick)
                    clickable.OnLeftClick();
                else
                    clickable.OnRightClick();
            }
        }
    }
}