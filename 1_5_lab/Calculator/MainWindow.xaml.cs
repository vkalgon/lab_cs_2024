using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<OperationResult> Operations { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Operations = new ObservableCollection<OperationResult>();
            OperationsLog.ItemsSource = Operations;
            CommandInput.KeyDown += CommandInput_KeyDown;
            OperationsLog.MouseDoubleClick += OperationsLog_MouseDoubleClick;
        }

        private void ExecuteCommand(object sender, RoutedEventArgs e)
        {
            ProcessCommand();
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessCommand();
            }
        }

        private void ProcessCommand()
        {
            string commandText = CommandInput.Text;
            CommandInput.Clear();

            if (string.IsNullOrWhiteSpace(commandText))
                return;

            try
            {
                double result = EvaluateExpression(commandText);
                AddOperationResult(commandText, result.ToString(), true);
            }
            catch (Exception ex)
            {
                AddOperationResult(commandText, ex.Message, false);
            }
        }

        private void AddOperationResult(string command, string result, bool success)
        {
            Operations.Add(new OperationResult
            {
                Command = command,
                CommandResult = $"Команда: {command}, Результат: {result}",
                BackgroundColor = success ? Brushes.White : Brushes.LightCoral
            });
        }

        private void OperationsLog_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OperationsLog.SelectedItem is OperationResult selectedOperation && selectedOperation.BackgroundColor == Brushes.White)
            {
                MessageBox.Show($"Переход к результату: {selectedOperation.CommandResult}");
            }
        }

        private double EvaluateExpression(string expression)
        {
            var tokens = Regex.Split(expression, @"([+\-*/])").Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            var postfix = ConvertToPostfix(tokens);
            return EvaluatePostfix(postfix);
        }

        private string[] ConvertToPostfix(string[] tokens)
        {
            var precedence = new Dictionary<string, int>
            {
                { "+", 1 }, { "-", 1 },
                { "*", 2 }, { "/", 2 }
            };

            var output = new List<string>();
            var operators = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, out _))
                {
                    output.Add(token);
                }
                else
                {
                    while (operators.Count > 0 && precedence.ContainsKey(operators.Peek()) && precedence[operators.Peek()] >= precedence[token])
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
            }

            while (operators.Count > 0)
            {
                output.Add(operators.Pop());
            }

            return output.ToArray();
        }

        private double EvaluatePostfix(string[] postfix)
        {
            var stack = new Stack<double>();

            foreach (var token in postfix)
            {
                if (double.TryParse(token, out var num))
                {
                    stack.Push(num);
                }
                else
                {
                    var b = stack.Pop();
                    var a = stack.Pop();

                    stack.Push(token switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        _ => throw new InvalidOperationException("Неверный оператор")
                    });
                }
            }

            return stack.Pop();
        }
    }

    public class OperationResult
    {
        public string Command { get; set; }
        public string CommandResult { get; set; }
        public Brush BackgroundColor { get; set; }
    }
}
