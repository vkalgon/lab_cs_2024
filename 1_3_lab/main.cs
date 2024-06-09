using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        string saveOption = DisplayMenu();
        List<double> results = LoadData(saveOption);
        double? currentResult = null;

        DisplayUsage();

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
                    SaveData(saveOption, results);
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

    static string DisplayMenu()
    {
        Console.WriteLine("Choose a storage option:");
        Console.WriteLine("1 - JSON");
        Console.WriteLine("2 - XML");
        Console.WriteLine("3 - SQLite");
        string choice = Console.ReadLine().Trim();
        return choice;
    }

    static void DisplayUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("when a first symbol on line is ‘>’ – enter operand (number)");
        Console.WriteLine("when a first symbol on line is ‘@’ – enter operation");
        Console.WriteLine("operation is one of ‘+’, ‘-‘, ‘/’, ‘*’ or");
        Console.WriteLine("‘#’ followed with number of evaluation step");
        Console.WriteLine("‘q’ to exit");
    }

    static List<double> LoadData(string saveOption)
    {
        switch (saveOption)
        {
            case "1":
                return LoadFromJson();
            case "2":
                return LoadFromXml();
            case "3":
                return LoadFromSQLite();
            default:
                return new List<double>();
        }
    }

    static void SaveData(string saveOption, List<double> results)
    {
        switch (saveOption)
        {
            case "1":
                SaveToJson(results);
                break;
            case "2":
                SaveToXml(results);
                break;
            case "3":
                SaveToSQLite(results);
                break;
        }
    }

    static List<double> LoadFromJson()
    {
        if (File.Exists("results.json"))
        {
            string json = File.ReadAllText("results.json");
            return JsonConvert.DeserializeObject<List<double>>(json);
        }
        return new List<double>();
    }

    static void SaveToJson(List<double> results)
    {
        string json = JsonConvert.SerializeObject(results, Formatting.Indented);
        File.WriteAllText("results.json", json);
    }

    static List<double> LoadFromXml()
    {
        if (File.Exists("results.xml"))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<double>));
            using (FileStream fs = new FileStream("results.xml", FileMode.Open))
            {
                return (List<double>)serializer.Deserialize(fs);
            }
        }
        return new List<double>();
    }

    static void SaveToXml(List<double> results)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<double>));
        using (FileStream fs = new FileStream("results.xml", FileMode.Create))
        {
            serializer.Serialize(fs, results);
        }
    }

    static List<double> LoadFromSQLite()
    {
        List<double> results = new List<double>();
        if (!File.Exists("results.db"))
        {
            return results;
        }

        using (SQLiteConnection connection = new SQLiteConnection("Data Source=results.db"))
        {
            connection.Open();
            string query = "SELECT Value FROM Results";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(reader.GetDouble(0));
                    }
                }
            }
        }
        return results;
    }

    static void SaveToSQLite(List<double> results)
    {
        using (SQLiteConnection connection = new SQLiteConnection("Data Source=results.db"))
        {
            connection.Open();
            string createTableQuery = "CREATE TABLE IF NOT EXISTS Results (Id INTEGER PRIMARY KEY, Value REAL)";
            using (SQLiteCommand createTableCommand = new SQLiteCommand(createTableQuery, connection))
            {
                createTableCommand.ExecuteNonQuery();
            }

            string deleteQuery = "DELETE FROM Results";
            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
            {
                deleteCommand.ExecuteNonQuery();
            }

            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                string insertQuery = "INSERT INTO Results (Value) VALUES (@Value)";
                using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.Add(new SQLiteParameter("@Value"));
                    foreach (var result in results)
                    {
                        insertCommand.Parameters["@Value"].Value = result;
                        insertCommand.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
    }
}
