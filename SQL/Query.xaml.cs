using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Data;

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
			SQLTxt.AppendText("Select ");
			FileLocationTxt.Text = AppDomain.CurrentDomain.BaseDirectory + "Table1.csv";
		}

		private void InitReservedWords()
		{
			Keywords = new []{ "select ", "from ", "join ", "on ", "group by ", "if ", "desc ", "where "};
			Types = new[] {"int ", "date ", "float ", "varchar ", "byte "};
			Expressions = new[] {"count", "avg", "max", "min"};
			SQLTxt.TextChanged += SQLTxt_OnTextChanged; 
		}
		
		private void SQLTxt_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (SQLTxt.CaretPosition.Paragraph == null) return;
			var start = SQLTxt.CaretPosition.Paragraph.ContentStart;
			var end = SQLTxt.CaretPosition.Paragraph.ContentEnd;
			var range = new TextRange(start, end);
			var text = range.Text;
			foreach (var keyword in Keywords)
			{
				var indexOf = text.ToLower().IndexOf(keyword, StringComparison.Ordinal);
				if (indexOf == -1) continue;
				var expressionRange = new TextRange(range.Start.GetPositionAtOffset(indexOf), range.Start.GetPositionAtOffset(indexOf + keyword.Length));
				expressionRange.ApplyPropertyValue(ForegroundProperty, "#569cd6");
				SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, Brushes.White);
			}
			foreach (var expression in Expressions)
			{
				var indexOf = text.ToLower().IndexOf(expression, StringComparison.Ordinal);
				if (indexOf == -1) continue;
				var keywordRange = new TextRange(range.Start.GetPositionAtOffset(indexOf), range.Start.GetPositionAtOffset(indexOf + expression.Length));
				keywordRange.ApplyPropertyValue(ForegroundProperty, Brushes.OrangeRed);
				SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, Brushes.White);
			}
			foreach (var type in Types)
			{
				var indexOf = text.ToLower().IndexOf(type, StringComparison.Ordinal);
				if (indexOf == -1) continue;
				var typeRange = new TextRange(range.Start.GetPositionAtOffset(indexOf), range.Start.GetPositionAtOffset(indexOf + type.Length));
				typeRange.ApplyPropertyValue(ForegroundProperty, "#41C9B0");
				SQLTxt.Selection.ApplyPropertyValue(ForegroundProperty, Brushes.White);
			}
			//range.ApplyPropertyValue(TextElement.ForegroundProperty , Brushes.Blue);
			SQLTxt.Foreground = Brushes.White;
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
			string nextWord;
			var alias = "";

			while (!keywords.Contains((nextWord = text.Dequeue()).ToLower()))
				columns.Add(nextWord);
			if (nextWord.ToLower() == "as")
			{
				alias = text.Dequeue();
				nextWord = text.Dequeue();
			}
			//Create DataTable
			if (nextWord.ToLower() == "from")
			{
				
			}
		}
	}
}
