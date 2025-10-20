using UnityEngine;
using UnityEngine.Serialization;
using Space = PsycheOpoly.Board.Space;
using Random = UnityEngine.Random;

public class SpaceRenderer : MonoBehaviour
{
    [SerializeField] private Space space;
    [SerializeField] public Color color;
    [SerializeField] private MeshRenderer meshRenderer;

    private void Awake()
    {
        // until we have actual Space parameters as ScriptableObjects, we'll have random colors.
        color = Random.ColorHSV(0,1,0,1,0,1,1,1);
    }

    public void SetUpSpace(Space space, float scale)
    {
        this.space = space;
        // scale the space relative to board/camera size
        transform.localScale *= scale;
        meshRenderer.material.color = color;

        name = space.Name;
    }
}
