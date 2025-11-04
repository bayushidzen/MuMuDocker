using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

static class Program
{
	static bool isStartGame = false;
	static string hiddenNumber = string.Empty;

	static void Main()
	{
		var botClient = new TelegramBotClient("Token");

		botClient.StartReceiving(HandleUpdate, HandleError);
		Console.ReadKey();
	}



	private static async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken token)
	{
		if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
		{
			var chatId = update.Message!.Chat.Id;
			var message = update.Message.Text!;
			var userName = update.Message.From?.FirstName;

			if (message == "/start")
			{
				string text = $"Здравствуйте {userName}, вы зашли в игру быки и коровы! Выберите действие:";
				var keyboard = new InlineKeyboardMarkup([
						[
							InlineKeyboardButton.WithCallbackData("📚Правила игры", "/rules")
						],
						[
							InlineKeyboardButton.WithCallbackData("🕹️Играть", "/game")
						]]);

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
					}

				}
			}
		}
		else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
		{
			var chatId = update.CallbackQuery!.Message!.Chat.Id;
			switch (update.CallbackQuery?.Data)
			{
				case "/game":
					{
						isStartGame = true;
						hiddenNumber = GenerateRandomNumber();

						var text = "Бот загадал число!\nВведите ваше число:";
						await client.SendMessage(chatId, text);
						break;
					}
				case "/rules":
					{
						var text = "Правила:\r\n\n" +
									"Бот задает случайное число длинной в 4 цифры, а ваша задача его отгадать. Каждая цифра от 0-9." +
									" Все цифры в числе различны, то есть числа 1233 быть не может! 0 может быть первой цифрой, " +
									"то есть числа по типу 0829 могут встретится! \r\n\n" +
									"Выдвигая свои числа, вы можете получить быков и коров.\n🐂Бык – какая-то цифра стоит на своем месте. " +
									"\n🐄Корова – какая-то цифра есть, но стоит не на своем месте.\r\n\n" +
									"За отгадывание числа вы можете получить от 10 баллов, " +
									"в зависимости от того за сколько попыток вы отгадали. " +
									"Например, если вы отгадали число за 6 попыток, то вы получите 4 балла. " +
									"Также можно уйти в минус, в таком случае из вашего счёта будет вычтено n-ное количество баллов. " +
									"Ваш счет в минус уйти не может, минимум 0!\r\n\n" +
									"В онлайн режиме число поучаемых баллов фиксировано, " +
									"а именно вы получите 20 баллов в случае победы и -10 баллов в случае поражения. " +
									"В онлайн режиме вы поочередно с противником пытаетесь отгадать число, " +
									"ваша задача сделать это первым иначе вы потерпите поражение!";

						var kbrd = new InlineKeyboardMarkup([
						[
							InlineKeyboardButton.WithCallbackData("🕹️Играть", "/game")
						]]);

						await client.SendMessage(chatId, text, replyMarkup: kbrd);
						break;
					}

			}
		}
	}

	private static string GenerateRandomNumber()
	{
		var numbers = Enumerable.Range(0, 10).ToList();
		var rnd = new Random();
		string randomNumber = "";
		while (randomNumber.Length != 4)
		{
			int randomIndex = rnd.Next(numbers.Count);
			randomNumber += numbers[randomIndex];
			numbers.RemoveAt(randomIndex);
		}
		return randomNumber;
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

	private static (int cowsCount, int bullsCount) CountingCowsAndBulls(string userNumber)
	{
		int cowsCount = 0;
		int bullsCount = 0;

		for (int i = 0; i < userNumber.Length; i++)
		{
			for (int j = 0; j < userNumber.Length; j++)
			{
				if (userNumber[i] == hiddenNumber[j] && j == i)
				{
					bullsCount++;
				}
				else if (userNumber[i] == hiddenNumber[j])
				{
					cowsCount++;
				}
			}
		}

		return (cowsCount, bullsCount);
	}
		private static async Task HandleError(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
	{
		throw new NotImplementedException();
	}

}
