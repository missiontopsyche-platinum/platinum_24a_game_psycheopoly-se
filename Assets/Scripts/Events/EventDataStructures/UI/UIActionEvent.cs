namespace Events.EventDataStructures.UI
{
    public class UIActionEvent
    {
        public UIType UIType { get; }
        public UIActionContext Context { get; }

        /// <summary>
        /// Creates a UI Action Event that will be caught by the PlayerController to resolve player actions
        /// on the UI. This will be created by UI objects to respond to user input.
        /// </summary>
        /// <param name="uiType">The type of UI that is sending the Action to resolve.</param>
        /// <param name="context">The context needed for PlayerController to resolve the action.</param>
        public UIActionEvent(UIType uiType, UIActionContext context)
        {
            UIType = uiType;
            Context = context;
        }
    }
}