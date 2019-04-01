using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMailroom.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }

        public Attachment(string FileName, byte[] FileBytes)
        {
            this.FileName = FileName ?? "";
            this.FileBytes = FileBytes;
        }

        public bool Save(int IdMail)
        {
            if (Id == 0)
            {
                string query = "INSERT INTO [dbo].[Attachment] ( FileName, FileBytes, IdMail ) VALUES ( @fn, @fb, @im )";
                int nbrRowsChanged = Database.ExecuteDmlQuery(query, new string[] { "fn", "fb", "im" }, new object[] { FileName, (object)FileBytes ?? 0, IdMail });
                if (nbrRowsChanged == 1)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
    }
}