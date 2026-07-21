using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        // 1. Работа со списком (List<T>)
        Console.WriteLine("1. Работа со списком (List<int>):");
        Console.WriteLine("Исходный список: 1, 2, 3, 4, 5");
        
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
        
        // Удвоить все чётные числа с помощью LINQ
        var doubledEvenNumbers = numbers.Select(x => x % 2 == 0 ? x * 2 : x).ToList();
        
        // Вывод результата
        Console.Write("Удвоены чётные числа: ");
        Console.WriteLine(string.Join(", ", doubledEvenNumbers)); // Ожидаемый вывод: 1, 4, 3, 8, 5
        Console.WriteLine(); 

        // 2. Работа со словарем (Dictionary<TKey, TValue>)
        Console.WriteLine("2. Работа со словарем (Dictionary<char, int>):");
        string text = "hello world"; // Можно изменить слово для примера
        Console.WriteLine($"Исходная строка: \"{text}\"");
        
        // Подсчёт частоты символов в строке, игнорируя пробелы
        var charFrequency = text
            .Where(c => !Char.IsWhiteSpace(c)) // Игнорируем пробелы
            .GroupBy(c => c)                   // Группируем по символам
            .ToDictionary(g => g.Key, g => g.Count()); // Преобразуем в словарь

        // Вывод результата
        Console.WriteLine("Частота символов:");
        foreach (var pair in charFrequency)
        {
            Console.WriteLine($"  Символ '{pair.Key}': {pair.Value} раз(а)");
        }
        Console.ReadKey();
    }
}