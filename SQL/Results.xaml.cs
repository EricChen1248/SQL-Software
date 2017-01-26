using System.Windows;
using System.Data;

namespace SQL
{
    /// <summary>
    /// Interaction logic for Results.xaml
    /// </summary>

    public partial class Results : Window
    {
        public DataTable ResultsTable = new DataTable();
        public Results()
		{
			InitializeComponent();
		}

		public void ShowResults()
		{
		    ResultsGrid.ItemsSource = ResultsTable.AsDataView();
		}
	}
}
