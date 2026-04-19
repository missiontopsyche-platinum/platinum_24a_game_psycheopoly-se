using System;
using Interactable;
using Logging;
using UnityEngine;

public class SpaceRenderer : InteractableGameObject
{
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] private TileAlwaysVisibleUI alwaysVisibleUI;
    [SerializeField] private GameObject ownedIconGO;
    [SerializeField] private GameObject mortgagedIconGO;


    private SpaceData spaceData;

    private bool isSetUp = false;
    
    // debug timer - remove when we add actual functionality.
    private float hoverTime = 1f;
    private float hoverTimer = 0f;

    private void Update()
    {
        if(isSetUp)
            RefreshTileStateVisuals();
    }

    public void SetUpSpace(SpaceData inputSpaceData, float scale)
    {
        spaceData = inputSpaceData;
        alwaysVisibleUI?.Apply(spaceData);
        transform.localScale *= scale;
        if (spaceData != null && spaceData.Artwork != null && meshRenderer != null)
        {
            var mat = meshRenderer.material;
            Texture2D tex = spaceData.Artwork.texture;

            // add texture (URP uses _BaseMap, Built-in uses _MainTex)
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            else mat.mainTexture = tex;

            //no tint
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            else mat.color = Color.white;
        }
        else if (meshRenderer != null)
        {
            // Original behavior: color-only tile
            meshRenderer.material.color = this.spaceData.spaceColor;
        }

        name = spaceData.spaceName;

        //call method to identify if Owned/Mortgaged Inidactor is visible
        RefreshIndicators();

        if (alwaysVisibleUI != null)
            alwaysVisibleUI.Apply(spaceData);
        
        // ensure box collider
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (!boxCollider)
            boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(scale, scale, 1f);

        isSetUp = true;
    }

    //easier to call the method rather than running it EVERY single time in SetUpSpace
    private void RefreshIndicators()
    {
        //only display indicators for spaces that are ownable
        bool isOwnable = spaceData is OwnableSpaceData;
        bool isOwned = false;

        bool isMortgaged = false;

        if (isOwnable && spaceData is OwnableSpaceData ownable)
        {
            isOwned = ownable.GetOwner() != null;
            isMortgaged = ownable.isMortgaged;


        }

        //mortgage replaces owned 
        if (ownedIconGO != null)
            ownedIconGO.SetActive(isOwnable && isOwned && !isMortgaged);

        if (mortgagedIconGO != null)
            mortgagedIconGO.SetActive(isOwnable && isMortgaged);
    }

    public void RefreshTileStateVisuals()
    {
        RefreshIndicators();

        if (alwaysVisibleUI != null && spaceData != null)
            alwaysVisibleUI.Apply(spaceData);
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
