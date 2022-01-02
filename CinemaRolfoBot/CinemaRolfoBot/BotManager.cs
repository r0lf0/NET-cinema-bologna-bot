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
            var chatId = update?.Message?.Chat?.Id;
            var messageText = update?.Message?.Text;
            Log.Debug($"Received '{messageText}' message in chat {chatId}.");

            if (chatId == null)
                Log.Error("Received message without ChatId.");
            else if (update.Type != UpdateType.Message
                         || update.Message!.Type != MessageType.Text
                         || string.IsNullOrWhiteSpace(messageText))
                _ = SendErrorMessageWithHelpRedirect(chatId);
            else if (Commands_Help.Contains(messageText, StringComparer.OrdinalIgnoreCase))
                _ = HandleHelpCmd(chatId, update.Message);
            else
                _ = SendErrorMessageWithHelpRedirect(chatId);

            return;
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

        private async Task<string> SendErrorMessageWithHelpRedirect(ChatId chatId)
        {
            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: EmojiParser.ReplaceColonNames(Msg_Error));
            return sentMessage.Text;
        }

        private async Task<string> HandleHelpCmd(ChatId chatId, Message message)
        {
            Message sentMessage = await telegramBotClient.SendTextMessageAsync(
                chatId: chatId,
                text: Msg_Welcome);
            return sentMessage.Text;
        }

        private static readonly IEnumerable<string> Commands_Help = new List<string> { "/aiuto", "/help", "/start" };
        private static readonly IEnumerable<string> Commands_TodayShowings = new List<string> { "/todayshowings" };
        private static readonly IEnumerable<string> Commands_DailyShowingsAll = new List<string> { "/completeshowings" };
        private static readonly IEnumerable<string> Commands_FilmDetails = new List<string> { "/film" };
        private static readonly IEnumerable<string> Commands_WhichTime = new List<string> { "/ora" };

        private const string Msg_Welcome = "Ciao, sono il bot non ufficiale del The Space di Bologna." +
                                           "Ecco le cose che puoi chiedermi:" +
                                           "\n/aiuto per rileggere questo messaggio' " +
                                           "\n/showPerData per ricevere la programmazione per data' " +
                                           "\n/film per la lista dei film in programmazione' " +
                                           "\n/film seguito da parte del titolo di un film per conoscerne i dettagli";

        private const string Msg_Error = "Scusa, non ho capito... :sob:" +
                                         "\nPer il momento accetto solo i comandi riportati qui: /aiuto";
    }
}