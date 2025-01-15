using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace MuMuDocker
{
    internal class Program
    {
        static bool isStartGame = false;
        static string hiddenNumber = "1234";
        static int historyMessageID = -1;
        static string historyMessageText = "";
        static void Main(string[] args)
        {
            var tgclient = new TelegramBotClient("8048489833:AAFxefliTFVLg1TZPUoYr5SdreKHQJ8FPLs");
            tgclient.StartReceiving(HandleUpdate, HandleError);

            Console.ReadKey();
        }

        private static async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var chatId = update.Message.Chat.Id;
                var message = update.Message.Text;
                var userName = update.Message.From.FirstName;

                if (message == "/start")
                {
                    string text = $"Здравствуйте {userName}, вы зашли в игру быки и коровы! Выберите действие: ";

                    var keyboard = new InlineKeyboardMarkup([
                        [InlineKeyboardButton.WithCallbackData("📚Правила игры","/rules")],
                        [InlineKeyboardButton.WithCallbackData("🕹️Начать игру","/startGame")]
                        ]);
                    await client.SendMessage(chatId, text, replyMarkup: keyboard);
                }
                else if (isStartGame)
			    {
				if (!IsValid(message))
				{
					await client.SendMessage(chatId, "Нельзя вводить число, с повторяющимеся цифрами, " +
						"нельзя вводить буквы " +
						"и число должно содержать только 4 цифры!\n\nВведите ваше число:"
						);
				}
				else
				{
					(int cowsCount, int bullsCount) = CountingCowsAndBulls(message);
					await client.SendMessage(chatId, $"Число: {message}\n -Быков: {bullsCount}\n -Коров: {cowsCount}\n\n");

					if (bullsCount == 4)
					{
						await client.SendMessage(chatId, "Вы выйграли!");
                        var keyboard = new InlineKeyboardMarkup([
                        [InlineKeyboardButton.WithCallbackData("Начать игру сначала?","/startGame")]
                        ]);
                        await client.SendMessage(chatId, message, replyMarkup: keyboard);
					}

				}
			}
                else
                {
                    await client.DeleteMessage(chatId, update.Message.MessageId);

                    (int bullsCount, int cowsCount) = CountingCowsAndBulls(message);
                    var text = $"Число: {message}\n Быков: {bullsCount}\n Коров: {cowsCount}\n\n";

                    historyMessageText += text;

                    if (historyMessageID == -1)
                    {
                        var historyMessage = await client.SendMessage(chatId, text);
                        historyMessageID = historyMessage.MessageId;
                    }
                    else
                    {
                        await client.EditMessageText(chatId, historyMessageID, historyMessageText);
                    }

                    //await client.SendMessage(chatID, text);
                    if (bullsCount == 4)
                    {
                        await client.SendMessage(chatId, "Ура! Вы победили! Поздравляем!");
                        var keyboard = new InlineKeyboardMarkup([
                        [InlineKeyboardButton.WithCallbackData("Начать игру сначала?","/startGame")]
                        ]);
                        await client.SendMessage(chatId, text, replyMarkup: keyboard);
                    }
                }
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                var chatID = update.CallbackQuery.Message.Chat.Id;
                var message = update.CallbackQuery.Data;

                switch (message)
                {
                    case "/rules":
                        {
                            var text = "Правила игры";
                            var keyboard = new InlineKeyboardMarkup([[InlineKeyboardButton.WithCallbackData("Играть", "/startGame")]]);
                            await client.SendMessage(chatID, text, replyMarkup: keyboard);
                            break;
                        }
                    case "/startGame":
                        {
                            hiddenNumber = GenerateHiddenNumber();

                            var text = $"Бот загадал число!\nВведите ваше число:"; //{hiddenNumber}
                            await client.SendMessage(chatID, text);

                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private static string GenerateHiddenNumber()
        {
            var digits = Enumerable.Range(0, 10).ToList();
            var rnd = new Random();
            string randomNumber = "";
            while (randomNumber.Length != 4)
            {
                int randomIndex = rnd.Next(digits.Count);
                randomNumber += digits[randomIndex];
                digits.RemoveAt(randomIndex);
            }
            return randomNumber;
        }

        private static (int bullsCount, int cowsCount) CountingCowsAndBulls(string message)
        {
            int bullsCount = 0;
            int cowsCount = 0;

            for (int i = 0; i < message.Length; i++)
            {
                for (int j = 0; j < hiddenNumber.Length; j++)
                {
                    if (message[i] == hiddenNumber[j])
                    {
                        if (i == j)
                        {
                            bullsCount++;
                        }
                        else
                        {
                            cowsCount++;
                        }
                    }
                }
            }
            return (bullsCount, cowsCount);
        }
        private static bool IsValid(string userNumber)
	{
		if (userNumber.Length != 4)
		{
			return false;
		}

		foreach (char charToCheck in userNumber)
			if (!char.IsDigit(charToCheck))
				return false;

		for (int i = 0; i < userNumber.Length; i++)
		{
			for (int j = 0; j < userNumber.Length; j++)
			{
				if (userNumber[i] == userNumber[j] && i != j)
					return false;
			}
		}

		return true;
	}


        private static async Task HandleError(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
