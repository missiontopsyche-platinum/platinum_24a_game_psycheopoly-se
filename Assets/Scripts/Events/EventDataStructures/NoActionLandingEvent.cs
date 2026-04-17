using System;

namespace Events.EventDataStructures
{
    public class NoActionLandingEvent
    {
        public String spaceName { get; }
        public String flavorText { get; }

        public NoActionLandingEvent(String spaceName, String flavorText)
        {
            this.spaceName = spaceName;
            this.flavorText = flavorText;
        }
    }
}