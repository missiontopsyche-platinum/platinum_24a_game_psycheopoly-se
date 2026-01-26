using Interactable;
using Logging;
using UnityEngine;

public class SpaceRenderer : InteractableGameObject
{
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] private TileAlwaysVisibleUI alwaysVisibleUI;
    
    private SpaceData spaceData;
    
    // debug timer - remove when we add actual functionality.
    private float hoverTime = 1f;
    private float hoverTimer = 0f;

    public void SetUpSpace(SpaceData inputSpaceData, float scale)
    {
        spaceData = inputSpaceData;
        alwaysVisibleUI?.Apply(spaceData);
        transform.localScale *= scale;
        meshRenderer.material.color = this.spaceData.spaceColor;

        name = spaceData.spaceName;

        if (alwaysVisibleUI != null)
            alwaysVisibleUI.Apply(spaceData);
        
        // ensure box collider
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (!boxCollider)
            boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(scale, scale, 1f);
    }

    public override void OnLeftClick()
    {
        Logging.Logger.Info(
            $"SpaceRenderer: {name}", 
            "On Left Click", 
            LogCategory.UI, this);
    }
    
    public override void OnRightClick()
    {
        Logging.Logger.Info(
            $"SpaceRenderer: {name}", 
            "On Right Click", 
            LogCategory.UI, this);
    }
    
    public override void OnHoverEnter()
    {
        Logging.Logger.Info(
            $"SpaceRenderer: {name}", 
            "On Hover Enter", 
            LogCategory.UI, this);

        spaceData.OnHover();
    }
    
    public override void OnHoverExit()
    {
        Logging.Logger.Info(
            $"SpaceRenderer: {name}",
            "On Hover Exit",
            LogCategory.UI, this);
        hoverTimer = 0f; // reset hover timer, remove later
        
        spaceData.OnExit();
    }

    public override void OnHover()
    {
        // this is all temporary to throw logs to indicate hovering is being checked.
        hoverTimer -= Time.deltaTime;
        if (hoverTimer < 0)
        {
            Logging.Logger.Info(
                $"SpaceRenderer: {name}", 
                "On Hover (current)", 
                LogCategory.UI, this);
            hoverTimer = hoverTime;
        }
    }
}
