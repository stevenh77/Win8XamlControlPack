using System;

namespace Win8XamlControlPack
{
    public class MultiContentControlStateChangedEventArgs : EventArgs
    {
        public MultiContentControlState NewState { get; set; }

        public MultiContentControlState OldState { get; set; }

        public MultiContentControlStateChangedEventArgs(MultiContentControlState oldState, MultiContentControlState newState)
        {
            NewState = newState;
            OldState = oldState;
        }
    }
}