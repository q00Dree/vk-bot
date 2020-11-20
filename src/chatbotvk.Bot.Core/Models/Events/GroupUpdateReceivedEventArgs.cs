using System;
using VkNet.Model.GroupUpdate;

namespace chatbotvk.Bot.Core.Models.Events
{
    public class GroupUpdateReceivedEventArgs : EventArgs
    {
        public GroupUpdate Update;

        public GroupUpdateReceivedEventArgs(GroupUpdate update)
        {
            this.Update = update;
        }
    }
}
