using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace vilniuseventsAPI.Controllers
{
    public static class EventsMySqlConnection
    { 
        public  static MySqlConnection GetConnection()
        {
            var connString = new MySqlConnectionStringBuilder
            {
                //Connection Settings
            };

            return new MySqlConnection(connString.ToString());
        }
    }
}
