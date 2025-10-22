using Interactable;
using Logging;
using UnityEngine;
using Space = PsycheOpoly.Board.Space;
using Random = UnityEngine.Random;

public class SpaceRenderer : InteractableGameObject
{
    [SerializeField] public Color color;
    [SerializeField] private MeshRenderer meshRenderer;

    private Space space;
    
    // debug timer - remove when we add actual functionality.
    private float hoverTime = 1f;
    private float hoverTimer = 0f;
    
    // to add: EventChannels to communicate with our other elements
    
    private void Awake()
    {
        // until we have actual Space parameters as ScriptableObjects, we'll have random colors.
        color = Random.ColorHSV(0,1,0,1,0,1,1,1);
    }

    public void SetUpSpace(Space spaceData, float scale)
    {
        space = spaceData;
        // scale the space relative to board/camera size
        transform.localScale *= scale;
        meshRenderer.material.color = color;

        name = space.Name;

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
    }
    
    public override void OnHoverExit()
    {
        Logging.Logger.Info(
            $"SpaceRenderer: {name}", 
            "On Hover Exit", 
            LogCategory.UI, this);
        
        hoverTimer = 0f; // reset hover timer, remove later
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
