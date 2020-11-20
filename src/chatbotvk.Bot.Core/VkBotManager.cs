﻿using chatbotvk.Bot.Core.Configurations;
using chatbotvk.Bot.Core.Contracts;
using chatbotvk.Bot.Core.Exceptions;
using chatbotvk.Bot.Core.Models.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Enums;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.GroupUpdate;
using VkNet.Model.RequestParams;

namespace chatbotvk.Bot.Core
{
    public class VkBotManager : IVkBotManager
    {
        #region .ctors
        public VkBotManager(ILogger<VkBotManager> logger,
                            IVkApi vkApi,
                            string accessToken = AuthOptions.VK_TOKEN,
                            string groupUrl = AuthOptions.COMMUNITY_URL,
                            int longPollTimeoutWaitSeconds = 25)
        {

            this.Logger = logger;
            this.Api = vkApi;

            this.SetupVkBot(accessToken, groupUrl, longPollTimeoutWaitSeconds);
        }
        #endregion

        #region Fields
        private LongPollServerResponse _pollSettings { get; set; }
        private int _longPollTimeoutWaitSeconds { get; set; } = 25;
        public event EventHandler<GroupUpdateReceivedEventArgs> OnGroupUpdateReceived;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        public event EventHandler OnBotStarted;
        public IVkApi Api { get; private set; }
        public long GroupId { get; private set; }
        public string GroupUrl { get; private set; }
        public string FilteredGroupUrl { get; private set; }
        public ILogger<VkBotManager> Logger { get; private set; }
        #endregion

        #region Methods
        private void SetupLongPoll()
        {
            _pollSettings = Api.Groups.GetLongPollServer((ulong)this.GroupId);
            this.Logger.LogInformation($"VkBot: LongPoolSettings received. ts: {_pollSettings.Ts}");
        }
        private long ResolveGroupId(string groupUrl)
        {
            this.FilteredGroupUrl = Regex.Replace(groupUrl, ".*/", "");

            VkObject result = this.Api.Utils.ResolveScreenName(this.FilteredGroupUrl);

            if (result == null || !result.Id.HasValue)
                throw new GroupNotResolvedException($"группа '{groupUrl}' не существует.");

            if (result.Type != VkObjectType.Group)
                throw new GroupNotResolvedException("GroupUrl не указывает на группу.");

            long groupId = result.Id.Value;

            this.Logger.LogInformation($"VkBot: GroupId resolved. id: {groupId}");
            return groupId;
        }
        private void SetupVkBot(string accessToken, string groupUrl, int longPollTimeoutWaitSeconds)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrEmpty(groupUrl))
                throw new ArgumentNullException(nameof(groupUrl));

            Api.Authorize(new ApiAuthParams
            {
                AccessToken = accessToken
            });

            this._longPollTimeoutWaitSeconds = longPollTimeoutWaitSeconds;
            this.GroupUrl = groupUrl;
            this.GroupId = this.ResolveGroupId(groupUrl);

            ServicePointManager.DefaultConnectionLimit = 20; //ограничение параллельных соединений для HttpClient
        }
        public void Dispose()
        {
            Api.Dispose();
        }
        public void Start()
        {
            this.StartAsync().GetAwaiter().GetResult();
        }
        public async Task StartAsync()
        {
            this.SetupLongPoll();
            this.OnBotStarted?.Invoke(this, null);
            while (true)
            {
                try
                {
                    BotsLongPollHistoryResponse longPollResponse = await Api.Groups.GetBotsLongPollHistoryAsync(
                        new BotsLongPollHistoryParams
                        {
                            Key = this._pollSettings.Key,
                            Server = this._pollSettings.Server,
                            Ts = this._pollSettings.Ts,
                            Wait = this._longPollTimeoutWaitSeconds
                        })
                        .ContinueWith(CheckLongPollResponseForErrorsAndHandle)
                        .ConfigureAwait(false);

                    if (longPollResponse == default(BotsLongPollHistoryResponse))
                        continue;

                    this.ProcessLongPollEvents(longPollResponse);
                    _pollSettings.Ts = longPollResponse.Ts;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex.Message + "\r\n" + ex.StackTrace);
                    throw;
                }
            }
        }
        private void ProcessLongPollEvents(BotsLongPollHistoryResponse pollResponse)
        {
            foreach (GroupUpdate update in pollResponse.Updates)
            {
                OnGroupUpdateReceived?.Invoke(this, new GroupUpdateReceivedEventArgs(update));
                if (update.Type == GroupUpdateType.MessageNew)
                {
                    OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(update.MessageNew.Message));
                }
            }
        }
        private T CheckLongPollResponseForErrorsAndHandle<T>(Task<T> task)
        {
            if (task.IsFaulted)
            {
                if (task.Exception is AggregateException ae)
                {
                    foreach (Exception ex in ae.InnerExceptions)
                    {
                        if (ex is LongPollOutdateException lpoex)
                        {
                            _pollSettings.Ts = lpoex.Ts;
                            return default(T);
                        }
                        else if (ex is LongPollKeyExpiredException)
                        {
                            this.SetupLongPoll();
                            return default(T);
                        }
                        else if (ex is LongPollInfoLostException)
                        {
                            this.SetupLongPoll();
                            return default(T);
                        }
                        else
                        {
                            Console.WriteLine(ex.Message);
                            throw ex;
                        }
                    }
                }

                this.Logger.LogError(task.Exception.Message);
                throw task.Exception;
            }
            else if (task.IsCanceled)
            {
                this.Logger.LogWarning(
                    "CheckLongPollResponseForErrorsAndHandle() : task.IsCanceled, possibly timeout reached");
                return default(T);
            }
            else
            {
                try
                {
                    return task.Result;
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex.Message);
                    throw;
                }
            }
        }
        #endregion
    }
}
