using chatbotvk.Bot.Core;
using chatbotvk.Bot.Core.Contracts;
using chatbotvk.Bot.Core.Models.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace chatbotvk.Bot.EntryPoint
{
    public class Bot
    {
        public IVkBotManager VkBot { get; private set; }
        public ILogger<Bot> Logger { get; set; }
        public Bot(IVkBotManager VkBot, ILogger<Bot> logger)
        {
            this.VkBot = VkBot;
            this.Logger = logger;
        }
        public void Start()
        {
            VkBot.OnMessageReceived += NewMessageHandler;
            VkBot.OnBotStarted += OnBotStartedHandler;

            this.VkBot.Start();

            Console.ReadLine();
        }
        private void OnBotStartedHandler(object sender, EventArgs eventArgs)
        {
            this.Logger.LogInformation("overbeered chatbot is working!");
        }
        private void NewMessageHandler(object sender, MessageReceivedEventArgs eventArgs)
        {
            VkBotManager instanse = sender as VkBotManager;

            var peerId = eventArgs.Message.PeerId;
            var fromId = eventArgs.Message.FromId;

            string message_text = eventArgs.Message.Text;

            if (message_text == "Hi")
            {
                instanse.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    PeerId = peerId,
                    Message = "Hello"
                });
            }
        }
    }
}
