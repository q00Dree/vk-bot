using System;
using System.Collections.Generic;
using System.Text;

namespace chatbotvk.Bot.Core.Exceptions
{
	class GroupNotResolvedException : VkBotException
	{
		public GroupNotResolvedException()
		{
		}

		public GroupNotResolvedException(string message)
			: base(message)
		{
		}

		public GroupNotResolvedException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
