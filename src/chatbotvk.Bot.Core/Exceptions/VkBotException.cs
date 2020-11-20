using System;
using System.Collections.Generic;
using System.Text;

namespace chatbotvk.Bot.Core.Exceptions
{	class VkBotException : Exception
	{
		public VkBotException()
		{
		}

		public VkBotException(string message)
			: base(message)
		{
		}

		public VkBotException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
