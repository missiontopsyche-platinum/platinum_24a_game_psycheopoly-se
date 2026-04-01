using System.Collections.Generic;
using Data;

namespace Data
{
    public class GameConfiguration
    {
        /// <summary>
        /// A static list of PlayerConfig objects that are created during game
        /// setup, and then consumed by GameManager while setting up the
        /// actual game scene.
        ///
        /// Since there is no state and no logic here, this is fine as a static
        /// data object.
        /// </summary>
        public static List<PlayerConfig> playerConfigs { get; set; }
        
        // In the future, we could move the configurable rules in here, assuming they have no internal
        // logic. Alternatively, we could just have a static reference to it in here.
    }
}