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
        private List<ToDoItem> _tasksForDoing = new List<ToDoItem>();
        private int _maxTaskCount;
        private int _maxTaskLength;
        private IUserService _userService;

        public UpdateHandler(int maxTaskCount, int maxTaskLength, IUserService userService)
        {
            _maxTaskCount = maxTaskCount;
            _maxTaskLength = maxTaskLength;
            _userService = userService;
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
            string taskName = Console.ReadLine();
            ValidateString(taskName);

            if (taskName.Length > _maxTaskLength)
                throw new TaskLengthLimitException(taskName.Length, _maxTaskLength);

            foreach (var task in _tasksForDoing)
            {
                if (task.Name == taskName)
                    throw new DuplicateTaskException(taskName);
            }

            var newTask = new ToDoItem
            {
                Name = taskName
            };

            _tasksForDoing.Add(newTask);
            botClient.SendMessage(chat, $"Книга '{taskName}' добавлена в список.");
        }

        private void ShowTasksList(ITelegramBotClient botClient, Chat chat)
        {
            var activeTasks = _tasksForDoing.Where(t => t.State == ToDoItemState.Active).ToList();
            if (activeTasks.Count == 0)
            {
                botClient.SendMessage(chat, "Книг, которые вы читаете в данный момент нет.");
            }
            else
            {
                botClient.SendMessage(chat, "\nСписок книг, которые вы читаете сейчас:");
                for (int i = 0; i < activeTasks.Count; i++)
                {
                    var task = activeTasks[i];
                    botClient.SendMessage(chat, $"{i + 1}. {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}");
                }     
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
                string removedTask = _tasksForDoing[numberOfTask - 1].Name;
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
                var user = message.From;

                var existingUser = _userService.GetUser(user.Id);

                if (existingUser == null && input != "/start")
                {
                    botClient.SendMessage(chat, "Для использования приложения необходимо зарегистрироваться. " +
                        "Введите команду /start \nСписок доступных команд:\n/start \n/help \n/info");
                    return;
                }

                switch (input)
                {
                    case "/start":
                        var registeredUser = _userService.RegisterUser(user.Id, user.Username ?? "Unknown");
                        botClient.SendMessage(chat, $"Добро пожаловать, {registeredUser.TelegramUserName}! Вы успешно зарегистрированы.");
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
                        botClient.SendMessage(chat, $"Приветствую, {existingUser.TelegramUserName}! Список доступных команд:" +
                                "\n/start \n/help \n/info \n/addtask \n/showtasks \n/removetask \n/exit");
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
