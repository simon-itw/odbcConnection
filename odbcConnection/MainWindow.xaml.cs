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

        private void BtnVisibility()
        {
            // type casting strings und abfrage auf anfang kleiner als ende
            if(DateTime.TryParse(auswahlDatumAnfang, out DateTime startDatum) &&
                DateTime.TryParse(auswahlDatumEnde, out DateTime endDatum))
            {
                TimeSpan urlaubBeantragt, urlaubMax = TimeSpan.FromDays(21);
                urlaubBeantragt = endDatum - startDatum;
                 
                if (endDatum>=startDatum && urlaubBeantragt <= urlaubMax)
                {

                    btnSend.Visibility = Visibility.Visible;
                    //urlaubBeantragt = endDatum - startDatum;
                    tb_urlaubBeantragt.Visibility = Visibility.Visible;
                    tb_urlaubBeantragt.Text = $"Dauer: {urlaubBeantragt.Days} Tage beantragt.\nVerbleibende Urlaubstage: {urlaubMax.Days - urlaubBeantragt.Days}";
                }
                else
                {
                    if(urlaubBeantragt > urlaubMax)
                    {
                        btnSend.Visibility = Visibility.Collapsed;
                        tb_urlaubBeantragt.Visibility = Visibility.Collapsed;
                        MessageBox.Show("Die Auswahl übersschreitet deine maximalen Urlaubstage");
                    }
                    else
                    {
                        btnSend.Visibility = Visibility.Collapsed;
                        tb_urlaubBeantragt.Visibility = Visibility.Collapsed;
                        MessageBox.Show("Das EndDatum darf nicht vor dem AnfangDatum liegen");
                    }
                   
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

    }

   
}