﻿using Otus.ToDoList.ConsoleBot;
using System.Numerics;
using System.Text;

namespace MyOtusProject;

class Program
{
    public static int ParseAndValidateInt(string? str, int min, int max)
    {
        if (!int.TryParse(str, out int number))
            throw new ArgumentException("Не удалось преобразовать строку в число");

        if (number < min || number > max)
            throw new ArgumentException($"Число должно быть в диапазоне от {min} до {max}");

        return number;
    }
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Введите максимально допустимое количество задач от 1 до 100");
            var maxTaskCount = ParseAndValidateInt(Console.ReadLine(), 1, 100);

            Console.WriteLine("Введите максимально допустимую длину задачи от 1 до 100.");
            var maxTaskLength = ParseAndValidateInt(Console.ReadLine(), 1, 100);

            var botClient = new ConsoleBotClient();
            var userRepository = new InMemoryUserRepository();
            var userService = new UserService(userRepository);
            var toDoService = new ToDoService(maxTaskCount, maxTaskLength);
            var handler = new UpdateHandler(userService, toDoService);
            botClient.StartReceiving(handler);
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
