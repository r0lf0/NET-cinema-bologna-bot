using CinemaRolfoBot.Utils;
using log4net;
using System.Collections.ObjectModel;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CinemaRolfoBot
{
    public class BotManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BotManager));

        private TelegramBotClient telegramBotClient = null;

        public BotManager(string token)
        {
            telegramBotClient = new TelegramBotClient(token);
            var me = AsyncHelpers.RunSync(() => telegramBotClient.GetMeAsync());
            Log.Info($"Telegram bot '{me.FirstName}' with id '{me.Id}' correctly initialized.");

            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };
            telegramBotClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;
            if (update.Message!.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText,
                cancellationToken: cancellationToken);
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}