using UnityEngine;
using Random = UnityEngine.Random;

public class SpaceRenderer : MonoBehaviour
{
    [SerializeField] public Color color;
    [SerializeField] private MeshRenderer renderer;

    private void Awake()
    {
        // until we have actual Space paramenters as ScriptableObjects, we'll have random colors.
        color = Random.ColorHSV(0,1,0,1,0,1,1,1);
    }

    public void SetUpSpace(float scale)
    {
        // scale the space relative to board/camera size
        transform.localScale *= scale;
        renderer.material.color = color;
    }
}
