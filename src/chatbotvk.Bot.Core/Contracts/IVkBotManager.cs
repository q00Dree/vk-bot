using chatbotvk.Bot.Core.Models.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VkNet.Abstractions;

namespace chatbotvk.Bot.Core.Contracts
{
    public interface IVkBotManager : IDisposable
    {
        IVkApi Api { get; }
        ILogger<VkBotManager> Logger { get; }
        long GroupId { get; }
        string GroupUrl { get; }

        string FilteredGroupUrl { get; }
        event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
        event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        event EventHandler OnBotStarted;

        Task StartAsync();
        void Start();
    }
}
