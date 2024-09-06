using odbcConnection.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace odbcConnection.subWin
{
    
    public partial class feiertagDataGrid : Window
    {
        public feiertagDataGrid()
        {
            InitializeComponent();
            LoadFeiertage();
        }

        private async void LoadFeiertage()
        {
            FeiertagConnector connector = new FeiertagConnector();
            try
            {
                List<FeiertagConnector.Feiertag> feiertage = await connector.GetFeiertagAsync();

                dataGridFeiertage.ItemsSource = feiertage.Select(f => new
                {
                    Datum = f.Datum.ToString("dd.MMMM.yyyy"),
                    Name = f.Name
                }).ToList();

                foreach(var feiertag in feiertage)
                {
                    Debug.WriteLine($"Feiertag: {feiertag.Datum:dd.MM.yyyy} - {feiertag.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Abrufen der Feiertage: {ex.Message}");
                MessageBox.Show("Fehler beim Abrufen der Feiertage.");
            }
        }

        private void Btn_Back(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Focus();

        }
    }
}
