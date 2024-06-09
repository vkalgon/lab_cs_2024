using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Xml.Serialization;

namespace MyCalcAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatorController : ControllerBase
    {
        private List<double> results = new List<double>();
        private double? currentResult = null;

        [HttpGet]
        public ActionResult<IEnumerable<double>> GetResults()
        {
            return Ok(results);
        }

        [HttpPost("operand")]
        public ActionResult AddOperand(double operand)
        {
            try
            {
                if (currentResult == null)
                {
                    currentResult = operand;
                }
                else
                {
                    return BadRequest("Operation missing. Please enter an operation after the operand.");
                }
                results.Add(currentResult.Value);
                return Ok(currentResult);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("operation")]
        public ActionResult PerformOperation(string operation, double operand)
        {
            try
            {
                if (currentResult == null)
                {
                    return BadRequest("Operand missing. Please enter an operand before the operation.");
                }

                if (operation == "+")
                {
                    currentResult += operand;
                }
                else if (operation == "-")
                {
                    currentResult -= operand;
                }
                else if (operation == "/")
                {
                    if (operand != 0)
                    {
                        currentResult /= operand;
                    }
                    else
                    {
                        return BadRequest("Division by zero is not allowed.");
                    }
                }
                else if (operation == "*")
                {
                    currentResult *= operand;
                }
                else
                {
                    return BadRequest("Invalid operation. Please enter a valid operation.");
                }

                results.Add(currentResult.Value);
                return Ok(currentResult);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("step/{step}")]
        public ActionResult GoToStep(int step)
        {
            try
            {
                if (step >= 1 && step <= results.Count)
                {
                    currentResult = results[step - 1];
                    results.Add(currentResult.Value);
                    return Ok(currentResult);
                }
                else
                {
                    return BadRequest("Invalid step number. Please enter a valid step number.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("save/{saveOption}")]
        public ActionResult SaveData(string saveOption)
        {
            try
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
                    default:
                        return BadRequest("Invalid save option. Please choose 1 (JSON), 2 (XML), or 3 (SQLite).");
                }
                return Ok("Data saved successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("load/{saveOption}")]
        public ActionResult<IEnumerable<double>> LoadData(string saveOption)
        {
            try
            {
                switch (saveOption)
                {
                    case "1":
                        return Ok(LoadFromJson());
                    case "2":
                        return Ok(LoadFromXml());
                    case "3":
                        return Ok(LoadFromSQLite());
                    default:
                        return BadRequest("Invalid save option. Please choose 1 (JSON), 2 (XML), or 3 (SQLite).");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private List<double> LoadFromJson()
        {
            if (System.IO.File.Exists("results.json"))
            {
                string json = System.IO.File.ReadAllText("results.json");
                return JsonConvert.DeserializeObject<List<double>>(json);
            }
            return new List<double>();
        }

        private void SaveToJson(List<double> results)
        {
            string json = JsonConvert.SerializeObject(results, Formatting.Indented);
            System.IO.File.WriteAllText("results.json", json);
        }

        private List<double> LoadFromXml()
        {
            if (System.IO.File.Exists("results.xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<double>));
                using (FileStream fs = new FileStream("results.xml", FileMode.Open))
                {
                    return (List<double>)serializer.Deserialize(fs);
                }
            }
            return new List<double>();
        }

        private void SaveToXml(List<double> results)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<double>));
            using (FileStream fs = new FileStream("results.xml", FileMode.Create))
            {
                serializer.Serialize(fs, results);
            }
        }

        private List<double> LoadFromSQLite()
        {
            List<double> results = new List<double>();
            if (!System.IO.File.Exists("results.db"))
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

        private void SaveToSQLite(List<double> results)
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
}
