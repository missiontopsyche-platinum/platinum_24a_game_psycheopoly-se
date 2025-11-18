using Logging;
using UnityEngine;

[CreateAssetMenu(menuName = "Board Spaces/Board Container")]
public class BoardSpaceContainer : ScriptableObject
{
    [SerializeField] private SpaceData[] spaces;

    /// <summary>
    /// Get a 'deep' copy of the BoardSpaceContainer's spaces. Technically, this will
    /// not have unique spaces, but it does preserve the ordered data within this container.
    /// </summary>
    /// <returns>copy of Space array</returns>
    public SpaceData[] GetSpaces()
    {
        if (spaces == null)
        {
            Logging.Logger.Error("BoardSpaceContainer.GetSpaces",
                "Spaces container is empty!",
                LogCategory.Core, this);
            return null;
        }
        SpaceData[] spacesCopy = new SpaceData[spaces.Length];

        for (int i = 0; i < spaces.Length; i++)
        {
            spacesCopy[i] = spaces[i];
        }

        return spacesCopy;
    }
}