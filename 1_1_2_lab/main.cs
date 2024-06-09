using System;

namespace MatrixOperations
{
    class Program
    {
        static void Main(string[] args)
        {
            // Генерация двумерного массива случайных целых чисел от 0 до 10
            Random random = new Random();
            int rows = 5;
            int cols = 5;
            int[,] matrix = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = random.Next(-11, 11);
                }
            }

            Console.WriteLine("Исходная матрица:");
            PrintMatrix(matrix);

            // 1. Определить количество столбцов, не содержащих ни одного нулевого элемента
            int nonZeroColumns = CountNonZeroColumns(matrix);
            Console.WriteLine($"Количество столбцов, не содержащих ни одного нулевого элемента: {nonZeroColumns}");

            // 2. Переставить строки матрицы в соответствии с ростом характеристик (сумма положительных четных элементов)
            matrix = SortRowsByCharacteristic(matrix);
            Console.WriteLine("Матрица после сортировки строк по характеристикам:");
            PrintMatrix(matrix);
        }

        static void PrintMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }

        static int CountNonZeroColumns(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int count = 0;

            for (int j = 0; j < cols; j++)
            {
                bool hasZero = false;
                for (int i = 0; i < rows; i++)
                {
                    if (matrix[i, j] == 0)
                    {
                        hasZero = true;
                        break;
                    }
                }
                if (!hasZero)
                {
                    count++;
                }
            }

            return count;
        }

        static int[,] SortRowsByCharacteristic(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            // Создаем массив характеристик и индексов строк
            int[][] rowWithCharacteristics = new int[rows][];

            for (int i = 0; i < rows; i++)
            {
                int characteristic = CalculateRowCharacteristic(matrix, i);
                rowWithCharacteristics[i] = new int[cols + 1];
                rowWithCharacteristics[i][0] = characteristic;

                for (int j = 0; j < cols; j++)
                {
                    rowWithCharacteristics[i][j + 1] = matrix[i, j];
                }
            }

            // Сортируем строки по характеристикам
            Array.Sort(rowWithCharacteristics, (a, b) => a[0].CompareTo(b[0]));

            // Переносим отсортированные строки обратно в матрицу
            int[,] sortedMatrix = new int[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sortedMatrix[i, j] = rowWithCharacteristics[i][j + 1];
                }
            }

            return sortedMatrix;
        }

        static int CalculateRowCharacteristic(int[,] matrix, int row)
        {
            int cols = matrix.GetLength(1);
            int sum = 0;

            for (int j = 0; j < cols; j++)
            {
                if (matrix[row, j] > 0 && matrix[row, j] % 2 == 0)
                {
                    sum += matrix[row, j];
                }
            }

            return sum;
        }
    }
}