namespace Data
{
    /// <summary>
    /// This struct contains a package used by PlayerManager to build out PlayerControllers
    /// with the correct sub-class (Human or AI), and is created in GameManager on game initialization
    /// based on the Game Configuration that is set before the game starts.
    /// </summary>
    public class PlayerConfig
    {
        public readonly Player playerData;
        public readonly bool isHuman;

        public PlayerConfig(Player playerData, bool isHuman)
        {
            this.playerData = playerData;
            this.isHuman = isHuman;
        }
    }
}