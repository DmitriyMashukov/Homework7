using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyOtusProject
{
    internal class UpdateHandler : IUpdateHandler
    {
        private List<string> _tasksForDoing = new List<string>();
        private int _maxTaskCount;
        private int _maxTaskLength;
        private string _name = "";

        public UpdateHandler(int maxTaskCount, int maxTaskLength)
        {
            _maxTaskCount = maxTaskCount;
            _maxTaskLength = maxTaskLength;
        }
        private static void ValidateString(string? str)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Строка не может быть null, пустой строкой или быть пробелом");
        }
        public static void DescriptionOfHelp(ITelegramBotClient botClient, Chat chat)
        {
            var text = new StringBuilder("Краткая информация о том, как пользоваться программой:" +
                "\n /start - Команда для начала работы с приложением. С помощью неё вы можете " +
                "зарегистрироватся в приложении, введя своё имя." +
                "\n /help - Команда со справочной информацией по работе с приложением." +
                "\n /info - Предоставляет информацию о версии программы и дате её создания." +
                "\n /addtask - Позволяет добавить новую книгу в список." +
                "\n /showtasks - Отображает список всех добавленных книг." +
                "\n /removetask - Позволяет удалять книги по номеру в списке." +
                "\n /exit - Команда для завершения работы приложения.");
            botClient.SendMessage(chat, text.ToString());
        }
        public void AddTask(ITelegramBotClient botClient, Chat chat)
        {
            if (_tasksForDoing.Count >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            botClient.SendMessage(chat, "Введите через запятую: название книги, имя и фамилию автора, количество страниц.");
            string task = Console.ReadLine();
            ValidateString(task);

            if (task.Length > _maxTaskLength)
                throw new TaskLengthLimitException(task.Length, _maxTaskLength);

            if (_tasksForDoing.Contains(task))
                throw new DuplicateTaskException(task);

            _tasksForDoing.Add(task);
            botClient.SendMessage(chat, $"Книга '{task}' добавлена в список.");
        }

        private void ShowTasksList(ITelegramBotClient botClient, Chat chat)
        {
            if (_tasksForDoing.Count == 0)
            {
                botClient.SendMessage(chat, "Вы ещё не добавили книг в список.");
            }
            else
            {
                botClient.SendMessage(chat, "\nСписок книг, которые хочу прочитать:");
                for (int i = 0; i < _tasksForDoing.Count; i++)
                    botClient.SendMessage(chat, $"{i + 1}. {_tasksForDoing[i]}");
            }
        }
        public void DeleteTask(ITelegramBotClient botClient, Chat chat)
        {
            ShowTasksList(botClient, chat);

            if (_tasksForDoing.Count == 0)
                return;

            botClient.SendMessage(chat, "Введите номер книги для удаления:");
            int numberOfTask;

            bool isNumber = int.TryParse(Console.ReadLine(), out numberOfTask);

            if (isNumber && numberOfTask > 0 && numberOfTask <= _tasksForDoing.Count)
            {
                string removedTask = _tasksForDoing[numberOfTask - 1];
                _tasksForDoing.RemoveAt(numberOfTask - 1);
                botClient.SendMessage(chat, $"Книга '{removedTask}' удалена из списка.");
            }
            else
            {
                botClient.SendMessage(chat, "Данного номера книги нет в списке.");
            }
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                var message = update.Message;
                if (message == null) return;

                var chat = message.Chat;
                var input = message.Text;

                switch (input)
                {
                    case "/start":
                        botClient.SendMessage(chat, "Пожалуйста, введите ваше имя:");
                        break;
                    case "/help":
                        DescriptionOfHelp(botClient, chat);
                        break;
                    case "/info":
                        botClient.SendMessage(chat, "Версия программы 1.0. Дата создания: 26.02.2025");
                        break;
                    case "/addtask":
                        AddTask(botClient, chat);
                        break;
                    case "/showtasks":
                        ShowTasksList(botClient, chat);
                        break;
                    case "/removetask":
                        DeleteTask(botClient, chat);
                        break;
                    case "/exit":
                        botClient.SendMessage(chat, "Завершение работы программы.");
                        break;
                    default:
                        if (string.IsNullOrEmpty(_name) && !input.StartsWith("/"))
                        {
                            _name = input;
                            botClient.SendMessage(chat, $"Имя {_name} успешно установлено.");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_name))
                            {
                                botClient.SendMessage(chat, $"Приветствую, пользователь! Список доступных команд:" +
                                    "\n/start \n/help \n/info \n/addtask \n/showtasks \n/removetask \n/exit");
                            }
                            else
                            {
                                botClient.SendMessage(chat, $"Приветствую, {_name}! Чем могу помочь?" +
                                    "\n/start \n/help \n/info \n/addtask \n/showtasks \n/removetask \n/exit");
                            }
                        }
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (TaskCountLimitException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (TaskLengthLimitException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (DuplicateTaskException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                botClient.SendMessage(update.Message.Chat, ex.Message);
            }
        }
    }
}
