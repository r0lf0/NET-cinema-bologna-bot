using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Utils
{
    public static class BotMessagesUtils
    {
        private static IEnumerable<char> CharactersToEscape = new List<char>() { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

        /// <summary>
        /// Escapes all CharactersToEscape
        /// </summary>
        public static string TelegramStringEscape(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (CharactersToEscape.Contains(input[i]))
                {
                    return TelegramStringEscapeImpl(input, i);
                }
            }

            return input;
        }

        private static string TelegramStringEscapeImpl(string input, int i)
        {
            StringBuilder sb = new StringBuilder();

            char ch = input[i];
            sb.Append(input.AsSpan(0, i));

            do
            {
                sb.Append('\\');
                switch (ch)
                {
                    case '\n':
                        ch = 'n';
                        break;

                    case '\r':
                        ch = 'r';
                        break;

                    case '\t':
                        ch = 't';
                        break;

                    case '\f':
                        ch = 'f';
                        break;
                }

                sb.Append(ch);
                i++;
                int lastpos = i;

                while (i < input.Length)
                {
                    ch = input[i];
                    if (CharactersToEscape.Contains(ch))
                        break;

                    i++;
                }

                sb.Append(input.AsSpan(lastpos, i - lastpos));
            } while (i < input.Length);

            return sb.ToString();
        }

        private const int BotMessageMaxLenght = 4096;

        public static IEnumerable<string> SplitMessage(string message)
        {
            List<string> output = new List<string>();

            while (message.Length >= BotMessageMaxLenght)
            {
                string tmp = message.Substring(0, BotMessageMaxLenght);
                int LastCarriageReturnIndex = tmp.LastIndexOf('\n');
                if (LastCarriageReturnIndex != -1)
                {
                    output.Add(message.Substring(0, LastCarriageReturnIndex + 1));
                    message = message.Substring(LastCarriageReturnIndex + 1);
                }
                else
                {
                    output.Add(message.Substring(0, BotMessageMaxLenght));
                    message = message.Substring(BotMessageMaxLenght);
                }
            }

            output.Add(message);

            return output;
        }
    }
}