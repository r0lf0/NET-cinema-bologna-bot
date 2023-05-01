using CinemaRolfoBot.Utils;
using log4net;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
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
        private DbManager dbManager = null;

        public BotManager(string token, DbManager dbManager)
        {
            this.dbManager = dbManager;

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
            else if (Const.Commands_Help.Contains(messageText, StringComparer.OrdinalIgnoreCase))
                _ = HandleHelpCmd(chatId);
            else if (Const.Commands_Films.Contains(messageText, StringComparer.OrdinalIgnoreCase))
                _ = HandleFilmsCommand(chatId);
            else if (messageText.StartsWith(Const.CommandsPrefix_FilmDetail))
                _ = HandleFilmDetailCommand(chatId, update.Message);
            else if (Const.Commands_TodayShowings.Contains(messageText, StringComparer.OrdinalIgnoreCase))
                _ = HandleTodayShowingsCommand(chatId);
            else if (Const.Commands_DailyShowingsAll.Contains(messageText, StringComparer.OrdinalIgnoreCase))
                _ = HandleCompleteShowingsCommand(chatId);
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

        private async Task<IEnumerable<Message>> SendErrorMessageWithHelpRedirect(ChatId chatId)
        {
            return await SendMessage(chatId, BotMessagesUtils.TelegramStringEscape(Const.Msg_Error), parseEmoji: true);
        }

        private async Task<IEnumerable<Message>> HandleHelpCmd(ChatId chatId)
        {
            return await SendMessage(chatId, BotMessagesUtils.TelegramStringEscape(Const.Msg_Welcome), parseEmoji: false);
        }

        private async Task<IEnumerable<Message>> HandleFilmsCommand(ChatId chatId)
        {
            IEnumerable<Model.DB.Film>? films = dbManager.GetAllFilms(withShowings: false);
            string message = ShowingParser.ParseFilmsList(films);
            return await SendMessage(chatId, message, true);
        }

        private async Task<IEnumerable<Message>> HandleTodayShowingsCommand(ChatId chatId)
        {
            IEnumerable<Model.DB.Film>? films = dbManager.GetAllFilms(withShowings: true);
            string message = ShowingParser.ParseTodayShowingsList(films);
            return await SendMessage(chatId, message, parseEmoji: true);
        }

        private async Task<IEnumerable<Message>> HandleCompleteShowingsCommand(ChatId chatId)
        {
            IEnumerable<Model.DB.Film>? films = dbManager.GetAllFilms(withShowings: true);
            string message = ShowingParser.ParseCompleteShowingsList(films);
            return await SendMessage(chatId, message, parseEmoji: true);
        }

        private async Task<IEnumerable<Message>> HandleFilmDetailCommand(ChatId chatId, Message message)
        {
            List<Message> list = new List<Message>();
            string idFilm = message.Text.Remove(0, Const.CommandsPrefix_FilmDetail.Length);

            Model.DB.Film? film = dbManager.GetFilmDetail(idFilm);
            if (film == null)
                list.AddRange(await SendMessage(chatId, $"Film con id *{BotMessagesUtils.TelegramStringEscape(idFilm)}* non trovato", parseEmoji: false));
            else
            {
                try
                {
                    if (film.Poster != null)
                        list.Add(await SendPhoto(chatId, film.Poster));
                    list.AddRange(await SendMessage(chatId, ShowingParser.ParseFilmDetails(film), parseEmoji: true));
                    list.AddRange(await SendMessage(chatId, ShowingParser.ParseFilmShowings(film), parseEmoji: true));
                }
                catch (Exception ex)
                {
                    Log.Error($"Error while sending film details to {chatId}. Error type: '{ex.GetType()}', Error message: {ex.Message}");
                }
            }

            return list;
        }

        public async Task<Message> SendPhoto(ChatId chatId, byte[] photo, string? caption = null)
        {
            using (Stream stream = new MemoryStream(photo))
            {
                return await telegramBotClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: stream,
                    caption: caption
                );
            }
        }

        public async Task<IEnumerable<Message>> SendMessage(ChatId chatId, string message, bool parseEmoji)
        {
            List<Message> SentMessages = new List<Message>();

            if (parseEmoji)
                message = EmojiParser.ReplaceColonNames(message);

            try
            {
                foreach (string m in BotMessagesUtils.SplitMessage(message))
                {
                    SentMessages.Add(await telegramBotClient.SendTextMessageAsync(
                                            chatId: chatId,
                                            text: m,
                                            parseMode: ParseMode.MarkdownV2));
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Can't send message '{message}' to {chatId}. Exception type: {ex.GetType()}. Error message: {ex.Message}");
            }

            return SentMessages;
        }
    }
}