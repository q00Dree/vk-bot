using chatbotvk.Bot.Core;
using chatbotvk.Bot.Core.Contracts;
using chatbotvk.Bot.Core.Models.Events;
using chatbotvk.Core.Models;
using chatbotvk.Core.Services.External;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.RequestParams;

namespace chatbotvk.Bot.EntryPoint
{
    /// <summary>
    /// Чат бот.
    /// </summary>
    public class Bot
    {
        public IVkBotManager VkBot { get; private set; }
        public ILogger<Bot> Logger { get; set; }

        private IExchangeRateService ExchangeRateService { get; set; }
        public Bot(IVkBotManager VkBot, 
                   ILogger<Bot> logger,
                   IExchangeRateService exchangeRateService)
        {
            this.VkBot = VkBot;
            this.Logger = logger;
            this.ExchangeRateService = exchangeRateService;
        }
        public void Start()
        {
            VkBot.OnMessageReceived += NewMessageHandler;
            VkBot.OnBotStarted += OnBotStartedHandler;
            VkBot.OnGroupUpdateReceived += VkBot_OnGroupUpdateReceived;

            this.VkBot.Start();
        }

        // Подробнее https://vk.com/dev/groups_events
        // А так же:
        // https://vknet.github.io/vk/messages/send/
        // https://github.com/vknet/vk/wiki/FAQ
        // https://github.com/vknet/vk
        #region Event Handlers
        /// <summary>
        /// Обработчик обновлений в группе.
        /// </summary>
        private void VkBot_OnGroupUpdateReceived(object sender, GroupUpdateReceivedEventArgs e)
        {
            //TODO: Сделать обработку событий обновлений группы.
            this.Logger.LogInformation($"{DateTime.Now.ToShortTimeString()} Event type: {e.Update.Type}.");
        }
        /// <summary>
        /// Обработчик запуска бота.
        /// </summary>
        private void OnBotStartedHandler(object sender, EventArgs eventArgs)
        {
            //TODO: Сделать обработку запуска бота.
            this.Logger.LogInformation($"{DateTime.Now.ToShortTimeString()} Overbeered bot is working now.");
        }
        /// <summary>
        /// Обработчик получения нового сообщения.
        /// </summary>
        private async void NewMessageHandler(object sender, MessageReceivedEventArgs eventArgs)
        {
            VkBotManager instance = sender as VkBotManager;

            long peerId = (long)eventArgs.Message.PeerId;
            long fromId = (long)eventArgs.Message.FromId;
            string message_text = eventArgs.Message.Text;

            if (message_text == "test")
            {
                await SendMessageAsync(message_text.ToUpper(), peerId);
            }
            else if(message_text == "cat")
            {
                string catUrl = @"https://loremflickr.com/400/300/";
                await SendMessageWithImageFromWebAsync("cat", peerId, catUrl, "jpg");
            }
            else if(message_text == "rate")
            {
                BankResponse bankResponse = await ExchangeRateService.GetCurrentExchangeRatesAsync();

                StringBuilder messageBuilder = new StringBuilder();

                messageBuilder.Append($"Актуальный котировки на {bankResponse.CurrentRateDate.AddHours(3)}.\n");

                foreach (var item in bankResponse.ActualValue)
                {
                    messageBuilder.Append($"Курс {item.Key} {item.Value} рублей.\n");
                }

                messageBuilder.Append($"Предыдущие котировки на {bankResponse.LastRateUpdateDate.AddHours(3)}.\n");

                foreach (var item in bankResponse.PreviousValue)
                {
                    messageBuilder.Append($"Курс {item.Key} {item.Value} рублей.\n");
                }

                await SendMessageAsync(messageBuilder.ToString(), peerId);
            }
        }
        #endregion

        #region Messages related stuff.
        /// <summary>
        /// Отправляет простое асинхронное сообщение.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="peerId">Получатель (личные сообщения/беседа).</param>
        private async Task SendMessageAsync(string message,
                                            long peerId)
        {
            await VkBot.Api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = peerId, //Id получателя
                Message = message, //Сообщение
                RandomId = Environment.TickCount //Уникальный идентификатор
            });
        }

        /// <summary>
        /// Отправляем сообщение с картинкой из интернета.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="peerId">Идентификатор получателя.</param>
        /// <param name="fileSrc">Адрес файла в сети.</param>
        /// <param name="fileExtension">Расширение файла.</param>
        /// <returns></returns>
        private async Task SendMessageWithImageFromWebAsync(string message,
                                                            long peerId,
                                                            string fileSrc,
                                                            string fileExtension)
        {
            // Получить адрес сервера для загрузки картинок в сообщении
            var uploadServer = await VkBot.Api.Photo.GetMessagesUploadServerAsync(peerId);

            // Загрузить картинку на сервер VK.
            var response = await UploadFileFromWebAsync(
                serverUrl: uploadServer.UploadUrl,
                fileSrc: fileSrc,
                fileExtension: fileExtension);

            // Сохранить загруженный файл
            var attachment = await VkBot.Api.Photo.SaveMessagesPhotoAsync(response);

            //Отправить сообщение с нашим вложением
            await VkBot.Api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = peerId, //Id получателя
                Message = message, //Сообщение
                Attachments = attachment, //Вложение
                RandomId = Environment.TickCount //Уникальный идентификатор
            });
        }

        /// <summary>
        /// Отправляем сообщение с картинкой из интернета.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="peerId">Идентификатор получателя.</param>
        /// <param name="fileSrc">Адрес файла в сети.</param>
        /// <param name="fileExtension">Расширение файла.</param>
        /// <returns></returns>
        private async Task SendMessageWithImageFromHostAsync(string message,
                                                             long peerId,
                                                             string filePath,
                                                             string fileExtension)
        {
            // Получить адрес сервера для загрузки картинок в сообщении
            var uploadServer = await VkBot.Api.Photo.GetMessagesUploadServerAsync(peerId);

            // Загрузить картинку на сервер VK.
            var response = await UploadFileFromHostAsync(
                serverUrl: uploadServer.UploadUrl,
                filePath: filePath,
                fileExtension: fileExtension);

            // Сохранить загруженный файл
            var attachment = await VkBot.Api.Photo.SaveMessagesPhotoAsync(response);

            //Отправить сообщение с нашим вложением
            await VkBot.Api.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = peerId, //Id получателя
                Message = message, //Сообщение
                Attachments = attachment, //Вложение
                RandomId = Environment.TickCount //Уникальный идентификатор
            });
        }

        /// <summary>
        /// Скачиваем файл из интернета и прикрепляем его к сообщению.
        /// </summary>
        /// <param name="serverUrl">Адрес сервера, на который будет сохранен файл.</param>
        /// <param name="fileSrc">Адрес картинки из интернета.</param>
        /// <param name="fileExtension">Расширение картинки.</param>
        /// <returns>Ответ от сервера, на котором сохраняется мультимедия.</returns>
        private async Task<string> UploadFileFromWebAsync(string serverUrl,
                                                          string fileSrc,
                                                          string fileExtension = "jpg")
        {
            // Получение массива байтов из файла
            byte[] data = await GetBytesFromWebAsync(fileSrc);

            // Создание запроса на загрузку файла на сервер
            using (var client = new HttpClient())
            {
                MultipartFormDataContent requestContent = new MultipartFormDataContent();
                ByteArrayContent content = new ByteArrayContent(data);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                requestContent.Add(content, "file", $"file.{fileExtension}");

                // асинхронный запрос.
                HttpResponseMessage response = await client.PostAsync(serverUrl, requestContent);

                // считываем ответ и переводим в строчный формат.
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                return Encoding.Default.GetString(responseBody);
            }
        }

        /// <summary>
        /// Скачиваем файл из директории и прикрепляем его к сообщению.
        /// </summary>
        /// <param name="serverUrl">Адрес сервера, на который будет сохранен файл.</param>
        /// <param name="fileSrc">Адрес картинки локально</param>
        /// <param name="fileExtension">Расширение картинки.</param>
        /// <returns>Ответ от сервера, на котором сохраняется мультимедия.</returns>
        private async Task<string> UploadFileFromHostAsync(string serverUrl,
                                                           string filePath,
                                                           string fileExtension = "jpg")
        {
            // Получение массива байтов из файла
            byte[] data = await GetBytesFromHostAsync(filePath);

            // Создание запроса на загрузку файла на сервер
            using (var client = new HttpClient())
            {
                MultipartFormDataContent requestContent = new MultipartFormDataContent();
                ByteArrayContent content = new ByteArrayContent(data);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                requestContent.Add(content, "file", $"file.{fileExtension}");

                // асинхронный запрос.
                HttpResponseMessage response = await client.PostAsync(
                    requestUri: serverUrl, 
                    content: requestContent);

                // считываем ответ и переводим в строчный формат.
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                return Encoding.Default.GetString(responseBody);
            }
        }

        /// <summary>
        /// Получаем файл(поток байтов) из интернета.
        /// </summary>
        /// <param name="fileUrl">Адрес файла в интернете.</param>
        /// <returns>Возвращает поток байтов.</returns>
        private async Task<byte[]> GetBytesFromWebAsync(string fileUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                return await webClient.DownloadDataTaskAsync(fileUrl);
            }
        }

        /// <summary>
        /// Получаем файл(поток байтов) из локальной директории хоста.
        /// </summary>
        /// <param name="filePath">Адрес файла на хосте.</param>
        /// <returns>Возвращает поток байтов.</returns>
        private async Task<byte[]> GetBytesFromHostAsync(string filePath)
        {
            return await File.ReadAllBytesAsync(filePath);
        }
        #endregion
    }
}
