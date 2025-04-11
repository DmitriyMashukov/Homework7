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

        private IUserService _userService;
        private IToDoService _toDoService;

        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
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
                "зарегистрироватся в приложении." +
                "\n /help - Команда со справочной информацией по работе с приложением." +
                "\n /info - Предоставляет информацию о версии программы и дате её создания." +
                "\n /addtask - Позволяет добавить новую книгу в список. Для добавления книги напишите команду" +
                " и через пробел укажите название книги, имя и фамилию автора и количество страниц." +
                "\n /showtasks - Отображает список всех добавленных книг." +
                "\n /removetask - Позволяет удалять книги по номеру в списке. Для удаления необходимо написать" +
                " команду и через пробел указать номер книги, которую хотите удалить." +
                "\n /completetask - Позволяет отметить книгу как прочитанную по её ID. Для использования напишите" +
                " команду и через пробел укажите ID книги." +
                "\n /exit - Команда для завершения работы приложения.");
            botClient.SendMessage(chat, text.ToString());
        }

        private void ShowTasksList(ITelegramBotClient botClient, Chat chat, ToDoUser user)
        {
            var activeTasks = _toDoService.GetActiveByUserId(user.UserId);

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

                if (existingUser == null && input != "/start" && input != "/help" && input != "/info")
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
                    case string s when s.StartsWith("/addtask "):
                        if (existingUser == null) return;

                        var taskName = input.Substring("/addtask ".Length).Trim();
                        ValidateString(taskName);

                        var newTask = _toDoService.Add(existingUser, taskName);
                        botClient.SendMessage(chat, $"Книга '{newTask.Name}' добавлена в список.");
                        break;
                    case "/showtasks":
                        if (existingUser != null)
                            ShowTasksList(botClient, chat, existingUser);
                        break;
                    case string s when s.StartsWith("/removetask "):
                        if (existingUser == null) return;

                        var inputNumber = input.Substring("/removetask ".Length).Trim();
                        if (int.TryParse(inputNumber, out int taskNumber))
                        {
                            var activeTasks = _toDoService.GetActiveByUserId(existingUser.UserId).ToList();
                            if (taskNumber > 0 && taskNumber <= activeTasks.Count)
                            {
                                var taskToRemove = activeTasks[taskNumber - 1];
                                _toDoService.Delete(taskToRemove.Id);
                                botClient.SendMessage(chat, $"Книга '{taskToRemove.Name}' удалена из списка.");
                            }
                            else
                            {
                                botClient.SendMessage(chat, "Данного номера книги нет в списке.");
                            }
                        }
                        else
                        {
                            botClient.SendMessage(chat, "Неверный формат номера задачи.");
                        }
                        break;
                    case string s when s.StartsWith("/completetask "):
                        if (existingUser == null) return;

                        var taskIdInput = input.Substring("/completetask ".Length).Trim();
                        if (Guid.TryParse(taskIdInput, out Guid taskId))
                        {
                            var allTasks = _toDoService.GetAllByUserId(existingUser.UserId);
                            ToDoItem taskToComplete = null;

                            foreach (var task in allTasks)
                            {
                                if (task.Id == taskId)
                                {
                                    taskToComplete = task;
                                    break;
                                }
                            }

                            if (taskToComplete != null)
                            {
                                _toDoService.MarkCompleted(taskId);
                                botClient.SendMessage(chat, $"Книга '{taskToComplete.Name}' отмечена как прочитанная.");
                            }
                            else
                            {
                                botClient.SendMessage(chat, "Книга с указанным ID не найдена.");
                            }
                        }
                        else
                        {
                            botClient.SendMessage(chat, "Неверный формат ID книги. ID должен быть в формате GUID.");
                        }
                        break;
                    case "/exit":
                        botClient.SendMessage(chat, "Завершение работы программы.");
                        break;
                    default:
                        botClient.SendMessage(chat, $"Приветствую, {existingUser.TelegramUserName}! Список доступных команд:" +
                                "\n/start \n/help \n/info \n/addtask \n/showtasks \n/removetask \n/completetask \n/exit");
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
