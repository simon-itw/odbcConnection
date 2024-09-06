using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace odbcConnection.Data
{
    public class FeiertagConnector
    {
        private readonly string apiUrl = "https://get.api-feiertage.de?states=be";

        public async Task<List<DateTime>> GetFeiertageAsync()
        {
            List<DateTime> feiertagsDaten = new List<DateTime>();
            using (HttpClient client = new HttpClient())
            {

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(content);

                    if (jsonResponse["feiertage"] is JArray feiertageArray)
                    {
                        foreach(var feiertag in feiertageArray)
                        {
                            DateTime datum = DateTime.Parse(feiertag["date"].ToString());
                            feiertagsDaten.Add(datum);
                        }
                    }
                    else
                    {
                        throw new Exception("Das JSON enthält nicht das erwartete 'feiertag' - Array");
                    }
                }
                else
                {
                    throw new HttpRequestException("Fehler beim RequestAufruf");
                }
            }
            return feiertagsDaten;
        }

        public async Task<List<Feiertag>> GetFeiertagAsync()
        {
            List<Feiertag> feiertagsDaten = new List<Feiertag>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(content);

                    if (jsonResponse["feiertage"] is JArray feiertageArray)
                    {
                        foreach (var feiertag in feiertageArray)
                        {
                            DateTime datum = DateTime.Parse(feiertag["date"].ToString());
                            string name = feiertag["fname"]?.ToString() ?? "Unbekannt"; 
                            feiertagsDaten.Add(new Feiertag { Datum = datum, Name = name });
                        }
                    }
                    else
                    {
                        throw new Exception("Das JSON enthält nicht das erwartete 'feiertag' - Array");
                    }
                }
                else
                {
                    throw new HttpRequestException("Fehler beim RequestAufruf");
                }
            }
            return feiertagsDaten;
        }

        public class Feiertag
        {
            public DateTime Datum { get; set; }
            public string? Name { get; set; }
        }
    }
}



