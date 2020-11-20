using System;
using VkNet.Model;

namespace chatbotvk.Bot.Core.Models.Events
{
	public class MessageReceivedEventArgs : EventArgs
	{
		public MessageReceivedEventArgs(Message message)
		{
			this.Message = message;
		}
		public Message Message;
	}
}
