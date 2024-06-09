using System;
using System.Linq;
class Program
{
    static void Main()
    {
        Random random = new Random();
        double[] array = new double[10];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = Math.Round(random.NextDouble() * 20 - 10, 2);
        }

        // 1. Вычисление суммы положительных элементов
        double sumPositive = array.Where(x => x > 0).Sum();
        Console.WriteLine($"Сумма положительных элементов: {sumPositive}");

        // 2. Вычисление произведения элементов, кроме максимального по модулю и минимального по модулю
        double maxAbs = array.Max(Math.Abs);
        double minAbs = array.Min(Math.Abs);
        double product = array.Where(x => Math.Abs(x) != maxAbs && Math.Abs(x) != minAbs).Aggregate(1.0, (prod, next) => prod * next);
        Console.WriteLine($"Произведение элементов (кроме максимального по модулю и минимального по модулю): {product}");

        // 3. Упорядочение элементов массива по убыванию
        double[] sortedArray = array.OrderByDescending(x => x).ToArray();
        Console.WriteLine("Элементы массива, упорядоченные по убыванию:");
        foreach (double num in sortedArray)
        {
            Console.WriteLine(num);
        }
    }
}
