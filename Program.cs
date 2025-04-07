using System.Numerics;
using System.Text;

namespace MyOtusProject;

class Program
{
    static List<string> tasksForDoing = new List<string>();
    static int maxTaskCount;
    static int maxTaskLength;

    public static void ValidateString(string? str)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("Строка не может быть null, пустой строкой или быть пробелом");
    }
    public static int ParseAndValidateInt(string? str, int min, int max)
    {
        if (!int.TryParse(str, out int number))
            throw new ArgumentException("Не удалось преобразовать строку в число");

        if (number < min || number > max)
            throw new ArgumentException($"Число должно быть в диапазоне от {min} до {max}");

        return number;
    }
    public static void AddTask()
    {
        if (tasksForDoing.Count >= maxTaskCount)
            throw new TaskCountLimitException(maxTaskCount);

        Console.WriteLine("Введите через запятую: название книги, имя и фамилию автора, количество страниц.");
        string task = Console.ReadLine();
        ValidateString(task);

        if (task.Length > maxTaskLength)
            throw new TaskLengthLimitException(task.Length, maxTaskLength);

        if (tasksForDoing.Contains(task))
            throw new DuplicateTaskException(task);

        tasksForDoing.Add(task);
        Console.WriteLine($"Книга '{task}' добавлена в список.");
    }

    public static void ShowTasksList()
    {
        if (tasksForDoing.Count == 0)
        {
            Console.WriteLine("Вы ещё не добавили книг в список.");
        }
        else
        {
            Console.WriteLine("\nСписок книг, которые хочу прочитать:");
            for (int i = 0; i < tasksForDoing.Count; i++)
                Console.WriteLine($"{i + 1}. {tasksForDoing[i]}");
        }
    }

    public static void DeleteTask()
    {
        ShowTasksList();

        if (tasksForDoing.Count == 0)
            return;

        Console.WriteLine("Введите номер книги для удаления:");
        int numberOfTask;

        bool isNumber = int.TryParse(Console.ReadLine(), out numberOfTask);

        if (isNumber && numberOfTask > 0 && numberOfTask <= tasksForDoing.Count)
        {
            string removedTask = tasksForDoing[numberOfTask - 1];
            tasksForDoing.RemoveAt(numberOfTask - 1);
            Console.WriteLine($"Книга '{removedTask}' удалена из списка.");
        }
        else
        {
            Console.WriteLine("Данного номера книги нет в списке.");
        }
    }
    public static void ReturnMenu()
    {
        Console.WriteLine("Для выхода в меню команд нажмите Enter.");
        Console.ReadKey();
    }
    public static void DescriptionOfHelp()
    {
        var text = new StringBuilder("Краткая информация о том, как пользоваться программой:" +
            "\n /start - Команда для начала работы с приложением. С помощью неё вы можете " +
            "зарегистрироватся в приложении, введя своё имя." +
            "\n /help - Команда со справочной информацией по работе с приложением." +
            "\n /info - Предоставляет информацию о версии программы и дате её создания." +
            "\n /echo - При вводе этой команды с аргументом (например, Hello), программа " +
            "возвращает введенный текст (в данном примере \"Hello\"). Становится доступной только " +
            "после ввода имени." +
            "\n /addtask - Позволяет добавить новую книгу в список." +
            "\n /showtasks - Отображает список всех добавленных книг." +
            "\n /removetask - Позволяет удалять книги по номеру в списке." +
            "\n /exit - Команда для завершения работы приложения.");
        Console.WriteLine(text.ToString());
    }
    static void Main(string[] args)
    {
        bool isContinue = true; 
        while (isContinue) 
        {
            try
            {
                Console.WriteLine("Введите максимально допустимое количество задач от 1 до 100");
                string? inputNumCount = Console.ReadLine();
                maxTaskCount = ParseAndValidateInt(inputNumCount, 1, 100);

                Console.WriteLine("Введите максимально допустимую длину задачи от 1 до 100.");
                string? inputNumLength = Console.ReadLine();
                maxTaskLength = ParseAndValidateInt(inputNumLength, 1, 100);

                string name = "";

                do
                {
                    Console.Clear();
                    if (name != "")
                        Console.WriteLine($"Приветствую, {name}! Чем могу помочь?" +
                        "\n/start \n/help \n/info \n/echo \n/addtask \n/showtasks \n/removetask \n/exit");
                    else
                        Console.WriteLine($"Приветствую, пользователь! Список доступных команд:" +
                        "\n/start \n/help \n/info \n/addtask \n/showtasks \n/removetask \n/exit");

                    var input = Console.ReadLine();
                    switch (input)
                    {
                        case "/start":
                            Console.WriteLine("Пожалуйста, введите ваше имя:");
                            name = Console.ReadLine();
                            Console.WriteLine($"Имя {name} успешно установлено.");
                            ReturnMenu();
                            break;
                        case "/help":
                            DescriptionOfHelp();
                            ReturnMenu();
                            break;
                        case "/info":
                            Console.WriteLine("Версия программы 1.0. Дата создания: 26.02.2025");
                            ReturnMenu();
                            break;
                        case "/echo":
                            if (name == "")
                            {
                                Console.WriteLine("Чтобы активировать эту команду введите своё имя.");
                                ReturnMenu();
                                break;
                            }
                            Console.WriteLine("Введите аргумент:");
                            var arg = Console.ReadLine();
                            Console.WriteLine(arg);
                            ReturnMenu();
                            break;
                        case "/addtask":
                            AddTask();
                            ReturnMenu();
                            break;
                        case "/showtasks":
                            ShowTasksList();
                            ReturnMenu();
                            break;
                        case "/removetask":
                            DeleteTask();
                            ReturnMenu();
                            break;
                        case "/exit":
                            Console.WriteLine("Завершение работы программы.");
                            isContinue = false;
                            break;
                        default:
                            Console.WriteLine("Введена некорректная команда.");
                            ReturnMenu();
                            break;
                    }
                } while (isContinue);

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
                Console.WriteLine($"Произошла непредвиденная ошибка:\nType: {ex.GetType().Name}" +
                    $"\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException}");
                Console.WriteLine("Нажмите Enter для продолжения");
                Console.ReadLine();
            }
        }
    } 
}
