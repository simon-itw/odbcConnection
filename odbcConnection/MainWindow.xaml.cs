using Newtonsoft.Json.Linq;
using odbcConnection.Data;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace odbcConnection
{
    public partial class MainWindow : Window
    {
        string auswahlDatumAnfang;
        string auswahlDatumEnde;
        
 
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectedDateChanged_Anfang(object sender, SelectionChangedEventArgs e)
        {
            DatumDisplay(ref auswahlDatumAnfang,datePickerAnfang, tb_anfangDisplay);
            BtnVisibility();
        }

        private void SelectedDateChanged_Ende(object sender, SelectionChangedEventArgs e)
        {
            DatumDisplay(ref auswahlDatumEnde, datePickerEnde, tb_endeDisplay);
            BtnVisibility();
        }

        private void DatumDisplay(ref string datum, DatePicker datePicker, TextBlock tb)
        {
            if (datePicker.SelectedDate.HasValue)
            {
                datum = datePicker.SelectedDate.Value.ToString("dd.MM.yyyy");
                tb.Text = datum;
            }
            else
            {
                datum = null;
                tb.Text = string.Empty;
            }
        }

        private async Task<List<DateTime>> GetFeiertageAsync()
        {
            FeiertagConnector connector = new FeiertagConnector();
            List<DateTime> feiertagsDaten = await connector.GetFeiertageAsync();

            return feiertagsDaten;
        }


        private async void Button_Feiertag_Konsole(object sender, RoutedEventArgs e)
        {
            FeiertagConnector connector = new FeiertagConnector();
            try
            {
                List<DateTime> feiertage = await connector.GetFeiertageAsync();
                foreach (var feiertag in feiertage)
                {
                    Debug.WriteLine($"Feiertag: {feiertag:dd.MM.yyyy}");

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Abrufen der Feiertage: {ex.Message}");
                MessageBox.Show("Fehler beim Abrufen der Feiertage.");
            }
        }

        private bool istFeiertagWochenende(DateTime date, List<DateTime> feiertage)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || feiertage.Contains(date.Date);  
        }


        private async void BtnVisibility()
        {
            if (DateTime.TryParse(auswahlDatumAnfang, out DateTime startDatum) && DateTime.TryParse(auswahlDatumEnde, out DateTime endDatum))
            {
                if (endDatum < startDatum)
                {
                    ShowError("Das EndDatum darf nicht vor dem AnfangsDatum liegen");
                }

                List<DateTime> feiertage = await GetFeiertageAsync();
                TimeSpan urlaubBeantragt = endDatum - startDatum, urlaubMax = TimeSpan.FromDays(27);

                int feiertagUndWochenendeAnzahl = 0;
                for (DateTime datum = startDatum; datum <= endDatum; datum = datum.AddDays(1))
                {
                    if (istFeiertagWochenende(datum, feiertage))
                    {
                        feiertagUndWochenendeAnzahl++;
                    }
                }

                urlaubBeantragt -= TimeSpan.FromDays(feiertagUndWochenendeAnzahl);

                if (urlaubBeantragt > urlaubMax)
                {                  
                    ShowError("Die Auswahl übersschreitet deine maximalen Urlaubstage");
                }
                else
                {
                    if (startDatum == endDatum)
                    {
                        urlaubBeantragt += TimeSpan.FromDays(1);
                    }

                    ShowSuccess(urlaubBeantragt, urlaubMax);
                }
            }
        }

        private void Button_Send(object sender, RoutedEventArgs e)
        {
           
            DatabaseConnector connect = new DatabaseConnector();

            try
            {
                // SQLClient query string und parameter syntax
                //string query = "INSERT INTO t_urlaub (anfangsDatum, endDatum) VALUES (@AuswahlAnfang, @AuswahlEnde)";

                //Odbc query string und syntax
                string query = "INSERT INTO t_urlaub (anfangsDatum, endDatum) VALUES (?, ?)";

                using (IDbConnection connection = connect.GetConnection())
                {
                    connect.OpenConnection(connection);

                    using(IDbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = query;

                        // parameter definieren um die eigentlichen werte zu übergeben und injections zu verhindern
                        IDbDataParameter paramAnfang = command.CreateParameter();
                        paramAnfang.ParameterName = "?";
                        paramAnfang.Value = auswahlDatumAnfang;

                        IDbDataParameter paramEnde = command.CreateParameter();
                        paramEnde.ParameterName = "?";
                        paramEnde.Value = auswahlDatumEnde; 

                        command.Parameters.Add(paramAnfang);
                        command.Parameters.Add(paramEnde);

                        int rowsAffected = command.ExecuteNonQuery();

                        if(rowsAffected > 0)
                        {
                            MessageBox.Show("Daten eingetragen");
                        }
                        else
                        {
                            MessageBox.Show("Daten NICHT eingetragen");
                        }  
                    }
                    
                    connect.CloseConnection(connection);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der Daten: {ex.Message}");
            }



        }

        private void ShowError(string message)
        {
            btnSend.Visibility = Visibility.Collapsed;
            tb_urlaubBeantragt.Visibility = Visibility.Collapsed;
            MessageBox.Show(message);
        }

        private void ShowSuccess(TimeSpan urlaubBeantragt, TimeSpan urlaubMax)
        {
            btnSend.Visibility = Visibility.Visible;
            tb_urlaubBeantragt.Visibility = Visibility.Visible;
            tb_urlaubBeantragt.Text = $"Dauer: {urlaubBeantragt.Days} Tage beantragt.\nVerbleibende Urlaubstage: {urlaubMax.Days - urlaubBeantragt.Days}\nStatus: ";
        }

    }

   
}