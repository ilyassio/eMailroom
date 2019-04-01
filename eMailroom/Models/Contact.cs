using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMailroom.Models
{
    public class Contact : Person
    {
        public Company Company { get; set; }
        public string Position { get; set; }

        public Contact(string id)
        {
            string query = "SELECT * FROM [dbo].[Contact] WHERE Id = @id";
            SqlDataReader reader = Database.ExecuteDqlQuery(query, new string[] { "id" }, new object[] { Int32.Parse(id) });

            if (reader != null && reader.HasRows)
            {
                try
                {
                    reader.Read();
                    Id = reader.GetInt32(0).ToString();
                    Firstname = reader.GetString(1);
                    Lastname = reader.GetString(2);
                    Gender = reader.GetString(3)[0];
                    Phone = reader.GetString(4);
                    Email = reader.GetString(5);
                    Company = new Company(reader.GetInt32(6));
                    Position = reader.GetString(7);
                    
                }
                catch
                {
                    Company = null;
                    Position = null;
                }
                finally
                {
                    Database.Close();
                }
                
            }
        }

        

    }
}