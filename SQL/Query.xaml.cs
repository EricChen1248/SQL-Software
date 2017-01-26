using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;

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
		private readonly List<DataTable> tables = new List<DataTable>();
		public MainWindow()
		{
			InitializeComponent();
			InitReservedWords();
			SQLTxt.AppendText("Type Here");
			FileLocationTxt.Text = AppDomain.CurrentDomain.BaseDirectory;
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

			if (start == null) return;

			var currentChar = GetCurrentChar(SQLTxt, start, LogicalDirection.Backward);
			while (currentChar != " " && currentChar != "")
			{
				if (start == null) continue;
				start = start.GetNextInsertionPosition(LogicalDirection.Backward);
				currentChar = GetCurrentChar(SQLTxt, start, LogicalDirection.Backward);
			}
			
			if (start != null)  SQLTxt.Selection.Select(start, currentPosition);
			var delim = new[] {' ', '\r', '\n', '(', ')', ','};
			var text = SQLTxt.Selection.Text.TrimEnd(delim).TrimStart(delim);
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
		
		private void ExecuteBtn_Click(object sender, RoutedEventArgs e)
		{
			var delim = new[] {' ', '\r', '\t', '\n'};
			var text =
				new TextRange(SQLTxt.Document.ContentStart, SQLTxt.Document.ContentEnd).Text.Split('"').Select(
					(element, index) => index%2 == 0 ? element.Split(delim, StringSplitOptions.RemoveEmptyEntries) : new[] {element}).SelectMany(element => element).ToList();
			var commands = new Queue<string>(text);
			var firstCommand = commands.Dequeue().ToLower();
		    try
		    {
		        if (firstCommand == "select")
		            Select(commands);
		    }
		    catch (InvalidOperationException)
		    {
		        MessageBox.Show("Incomplete SQL Statement");
		    }
		    catch (SQLStatementIncompleteException)
		    {
		        //Nothing
		    }
		    catch (SQLErrorException)
		    {
		        //Nothing
		    }
		    catch (Exception exception)
		    {
		        MessageBox.Show("Contact developer with the following message: " + exception.Message);
		    }
		    finally
		    {
		        tables.Clear();
		    }
		}

		private void Select(Queue<string> text)
		{
			var keywords = new[] {"from", "as"};
			var selectedColumns = new Dictionary<string,string>();
			var nextWord = text.Dequeue();
			do
			{
				if (keywords.Contains(nextWord.ToLower()))
				{
					MessageBox.Show("Forbidden Name in statement: " + nextWord);
					throw new SQLErrorException();
				}
				selectedColumns.Add(text.Peek().ToLower() == "as" ? text.Dequeue() : nextWord, nextWord);
				nextWord = text.Dequeue();
			} while (nextWord == ",");


			//Create DataTable
			if (nextWord.ToLower() == "from")
			{
				do
				{
					tables.Add(ConvertCSVToDataTable(FileLocationTxt.Text + text.Dequeue() + ".csv"));
				} while (text.Count > 0 && text.Peek() == ",");
			}
			if (text.Count == 0)
			{
				if (tables.Count > 1)
				{
					MessageBox.Show("Error. Missing Joint Table Condition");
					return;
				}
			    var results = new Results {ResultsTable = tables[0]};
                results.ShowResults();
                results.Show();
			    return;
			}
		    if (text.Peek().ToLower() != "join" || text.Peek().ToLower() != "where")
		    {
				MessageBox.Show("Missing From Statement");
		        throw new SQLStatementIncompleteException();
		    }

			if (text.Peek().ToLower() == "where")
			{
				
			}
		}

		/// <summary>
		/// Converts CSV files to Data Table
		/// </summary>
		/// <param name="strFilePath"></param>
		/// <returns></returns>
		private static DataTable ConvertCSVToDataTable(string strFilePath)
		{
			var dt = new DataTable();
			using (var sr = new StreamReader(strFilePath))
			{
				var readLine = sr.ReadLine();
				if (readLine == null)
				{
					MessageBox.Show("File " + strFilePath + " does not exist");
					throw new NullReferenceException();
				}

				var headers = readLine.Split(',');
				foreach (var header in headers)
					dt.Columns.Add(header);
				while (!sr.EndOfStream)
				{
					var rows = sr.ReadLine()?.Split(',');
					var dr = dt.NewRow();
					for (var i = 0; i < headers.Length; i++)
						dr[i] = rows?[i];
					dt.Rows.Add(dr);
				}
			}
			return dt;
		}

		/// <summary>
		/// Clears richtextbox text on first focus
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SQLTxt_OnGotFocus(object sender, RoutedEventArgs e)
		{
			SQLTxt.Document.Blocks.Clear();
			SQLTxt.GotFocus -= SQLTxt_OnGotFocus;
			SQLTxt.LostFocus += SQLTxt_LostFocus;
		}

		/// <summary>
		/// Fills SQLTxt with instructions if empty
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SQLTxt_LostFocus(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(new TextRange(SQLTxt.Document.ContentStart, SQLTxt.Document.ContentEnd).Text)) return;
			SQLTxt.Document.Blocks.Clear();
			SQLTxt.Document.Blocks.Add(new Paragraph(new Run("Type Here")));
			SQLTxt.GotFocus += SQLTxt_OnGotFocus;
			SQLTxt.LostFocus -= SQLTxt_LostFocus;
		}
	}

}
