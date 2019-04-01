using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace eMailroom.Models
{
    public class Employee : Person
    {
        public string PasswordHash { get; set; }
        public string Position { get; set; }
        
        public Employee(string id)
        {
            string query = "SELECT * FROM [Employee] WHERE Id = @id";
            SqlDataReader reader = Database.ExecuteDqlQuery(query, new string[] { "id" }, new object[] { id });

            if (reader != null && reader.HasRows)
            {
                try
                {
                    reader.Read();
                    Id = reader.GetString(0);
                    PasswordHash = reader.GetString(1);
                    Position = reader.GetString(2);
                    Firstname = reader.GetString(3);
                    Lastname = reader.GetString(4);
                    Gender = reader.GetString(5)[0];
                    Email = reader.GetString(6);
                    Phone = reader.GetString(7);
                }
                catch
                {

                }
                finally
                {
                    Database.Close();
                }
              
            }
        }

        private bool CheckPassword(string password)
        {
            if (PasswordHash != null)
            {
                byte[] hashBytes = Convert.FromBase64String(PasswordHash);
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                        return false;
            }
            else
                return false;

            return true;
        }

        public Tuple<string, string> Signin(string password)
        {
            if (CheckPassword(password))            
                return Tuple.Create(Id, Position);            
            else            
                return Tuple.Create((string)null, (string)null);    
        }
        
        public int ChangePassword(string oldPassword, string newPassword)
        {
            if (CheckPassword(oldPassword))
            {
                // Create the salt value with a cryptographic PRNG:
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                // Create the Rfc2898DeriveBytes and get the hash value:
                var pbkdf2 = new Rfc2898DeriveBytes(newPassword, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);

                // Combine the salt and password bytes for later use:
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

                // Turn the combined salt+hash into a string for storage:
                PasswordHash = Convert.ToBase64String(hashBytes);

                string query = "UPDATE Employee SET PasswordHash = '"+PasswordHash+"' WHERE Id = '"+Id+"'";
                int nbrRowsChanged = Database.ExecuteDmlQuery(query);
                if (nbrRowsChanged == 1)
                    return 1;
                else
                    return -1;

            }
            else
                return 0;
        }

    }
}