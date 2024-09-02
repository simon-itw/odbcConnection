using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Odbc;

namespace odbcConnection.Data
{
    public class DatabaseConnector
    {
        private readonly string connectionString;

        public DatabaseConnector()
        {
            // string aus der App.config abrufen
            connectionString = ConfigurationManager.ConnectionStrings["VerbindungsString"].ConnectionString;
        }

        public IDbConnection GetConnection()
        {
            return new OdbcConnection(connectionString);
        }

        public void OpenConnection(IDbConnection connection)
        {
            if(connection.State == ConnectionState.Closed)
            {
                connection.Open();  
            }
        }

        public void CloseConnection(IDbConnection connection)
        {
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            } 
        }
    }

    
    
}
