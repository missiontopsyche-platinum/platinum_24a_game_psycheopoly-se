using System;
using System.Collections.Generic;
using UnityEngine;

namespace Events.EventDataStructures
{
    /// <summary>
    /// Event data for a space being hovered over by the mouse. Contains information about
    /// the space, including the Name, Color, and then a list of descriptor attributes.
    /// </summary>
    public class SpaceHoverEvent
    {
        public String spaceName { get; private set; }
        public Color spaceColor { get; private set; }
        public List<String> spaceInformation;

        public SpaceData spaceData { get; private set; }
        public Sprite smallIcon { get; private set; }
        public Sprite artwork { get; private set; }
                
        public SpaceHoverEvent(SpaceData data)
        {
            spaceData = data;
            spaceName = data != null ? data.spaceName : string.Empty;
            spaceColor = data != null ? data.spaceColor : Color.white;
            spaceInformation = new List<string>();

            smallIcon = data != null ? data.smallIcon : null;
            artwork = data != null ? data.artwork : null;
        }

        /// <summary>
        /// Append a line to Space Information to be displayed on the OnHoverUI panel.
        /// </summary>
        /// <param name="newInfo">New line to add to the UI panel.</param>
        public void AppendInformation(String newInfo)
        {
            spaceInformation.Add(newInfo);
        }
    }
}