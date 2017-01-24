using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Data;
using System.Diagnostics;

namespace SQL
{
	/// <summary>
	/// Interaction logic for Query.xaml
	/// </summary>
	public partial class MainWindow
	{
		public string[] Keywords { get; private set; }
		public string[] Types { get; private set; }
		public string[] Expressions { get; set; }

		private DataTable data = new DataTable();
		public MainWindow()
		{
			InitializeComponent();
			InitReservedWords();
			SQLTxt.AppendText("Type Here");
			FileLocationTxt.Text = AppDomain.CurrentDomain.BaseDirectory + "Table1.csv";
		}

		private void InitReservedWords()
		{
			Keywords = new []{ "select", "from", "join", "on", "group by", "if", "desc", "where"};
			Types = new[] {"int", "date", "float", "varchar", "byte"};
			Expressions = new[] {"count", "avg ", "max ", "min"};
			SQLTxt.TextChanged += SQLTxt_OnTextChanged; 
		}
		private void SQLTxt_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			var currentPosition = SQLTxt.CaretPosition;
			var start = currentPosition?.GetNextInsertionPosition(LogicalDirection.Backward);
			if (start == null)
				return;

			var currentChar = GetCurrentChar(SQLTxt, start, LogicalDirection.Backward);
			while (currentChar != " " && currentChar != "")
			{
				if (start == null) continue;
				start = start.GetNextInsertionPosition(LogicalDirection.Backward);
				currentChar = GetCurrentChar(SQLTxt, start, LogicalDirection.Backward);
			}

			if (start != null) SQLTxt.Selection.Select(start, currentPosition);

			var text = SQLTxt.Selection.Text.TrimEnd(' ').TrimStart('\r','\n');
			Debug.WriteLine(text);

			if (Keywords.Any(x => x == text.ToLower()))
				 SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, "#569cd6");
			else if (Expressions.Any(x => x == text.ToLower()))
				SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty,Brushes.OrangeRed);
			else if (Types.Any(x => x == text.ToLower()))
				SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, "#41C9B0");
			else
				SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, Brushes.White);

			SQLTxt.CaretPosition = currentPosition;
			SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, Brushes.White);
			
			SQLTxt.Foreground = Brushes.White;
		}

		private static string GetCurrentChar(RichTextBox richTextBox, TextPointer pointer, LogicalDirection direction)
		{
			var textPointer = pointer.GetNextInsertionPosition(direction);
			if (textPointer == null)
				return "";
			richTextBox.Selection.Select(pointer,textPointer);
			return richTextBox.Selection.Text.Length == 0 ? "" : richTextBox.Selection.Text[0].ToString();
		}

		private void ExecuteBtn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var delim = new[] {' ', '\r', '\t', '\n'};
			var text =
				new TextRange(SQLTxt.Document.ContentStart, SQLTxt.Document.ContentEnd).Text.Split('"').Select(
					(element, index) => index%2 == 0 ? element.Split(delim, StringSplitOptions.RemoveEmptyEntries) : new[] {element}).SelectMany(element => element).ToList();
			var commands = new Queue<string>(text);
			var firstCommand = commands.Dequeue().ToLower();
			if (firstCommand== "select")
				Select(commands);
		}

		private static void Select(Queue<string> text)
		{
			var keywords = new[] {"from", "as"};
			var columns = new List<string>();
			var alias = new List<string>();
			var nextWord = text.Dequeue();
			do
			{
				if (!keywords.Contains(nextWord.ToLower()))
					columns.Add(nextWord);
				if (nextWord.ToLower() != "as") continue;
				alias.Add(text.Dequeue());
				nextWord = text.Dequeue();
			} while (nextWord == ",");

			//Create DataTable
			if (nextWord.ToLower() == "from")
			{
				
			}
		}
	}
}
