using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        DisplayUsage();
        List<double> results = new List<double>();
        double? currentResult = null;

        while (true)
        {
            string input = Console.ReadLine();

            if (input.StartsWith(">"))
            {
                try
                {
                    double operand = double.Parse(input.Substring(1).Trim());
                    if (currentResult == null)
                    {
                        currentResult = operand;
                    }
                    else
                    {
                        Console.WriteLine("Operation missing. Please enter an operation after the operand.");
                        continue;
                    }
                    results.Add(currentResult.Value);
                    Console.WriteLine($"[# {results.Count}] = {currentResult}");
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid operand. Please enter a valid number.");
                }
            }
            else if (input.StartsWith("@"))
            {
                string operation = input.Substring(1).Trim();

                if (operation == "q")
                {
                    break;
                }
                else if (operation.StartsWith("#"))
                {
                    try
                    {
                        int step = int.Parse(operation.Substring(1).Trim());
                        if (step >= 1 && step <= results.Count)
                        {
                            currentResult = results[step - 1];
                            results.Add(currentResult.Value);
                            Console.WriteLine($"[# {results.Count}] = {currentResult}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid step number. Please enter a valid step number.");
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid step number. Please enter a valid step number.");
                    }
                }
                else if (operation == "+" || operation == "-" || operation == "/" || operation == "*")
                {
                    if (currentResult == null)
                    {
                        Console.WriteLine("Operand missing. Please enter an operand before the operation.");
                        continue;
                    }

                    Console.Write("Enter next operand: ");
                    string nextOperandInput = Console.ReadLine();
                    try
                    {
                        double nextOperand = double.Parse(nextOperandInput.Trim());
                        if (operation == "+")
                        {
                            currentResult += nextOperand;
                        }
                        else if (operation == "-")
                        {
                            currentResult -= nextOperand;
                        }
                        else if (operation == "/")
                        {
                            if (nextOperand != 0)
                            {
                                currentResult /= nextOperand;
                            }
                            else
                            {
                                Console.WriteLine("Division by zero is not allowed.");
                                continue;
                            }
                        }
                        else if (operation == "*")
                        {
                            currentResult *= nextOperand;
                        }

                        results.Add(currentResult.Value);
                        Console.WriteLine($"[# {results.Count}] = {currentResult}");
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid operand. Please enter a valid number.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid operation. Please enter a valid operation.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please start with ‘>’ for operand or ‘@’ for operation.");
            }
        }
    }

    static void DisplayUsage()
    {S
        Console.WriteLine("Usage:");
        Console.WriteLine("when a first symbol on line is ‘>’ – enter operand (number)");
        Console.WriteLine("when a first symbol on line is ‘@’ – enter operation");
        Console.WriteLine("operation is one of ‘+’, ‘-‘, ‘/’, ‘*’ or");
        Console.WriteLine("‘#’ followed with number of evaluation step");
        Console.WriteLine("‘q’ to exit");
    }
}
