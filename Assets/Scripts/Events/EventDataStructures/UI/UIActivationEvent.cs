namespace Events.EventDataStructures.UI
{
    public class UIActivationEvent
    {
        public UIType UIType { get; }
        public UIActivationContext Context { get; }

        /// <summary>
        /// Creates a UI Activation Event that will be caught by the targeted UI and given the context to
        /// load and render it correctly.
        /// </summary>
        /// <param name="uiType">The type of UI that will catch and resolve this event.</param>
        /// <param name="context">The context needed for the UI to render dynamically.</param>
        public UIActivationEvent(UIType uiType, UIActivationContext context)
        {
            UIType = uiType;
            Context = context;
        }
    }
}