using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace eMailroom.Models
{
    public class Mail
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool Treated { get; set; }

        public string Date { get; set; }

        public Person Sender { get; set; }
        public Person Receiver { get; set; }
                
        public string Object { get; set; }
        public string Message { get; set; }

        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
        public Attachment[] Attachments;

        public Mail(string Type, string Date, string SenderId, string ReceiverId, string Object, string Message, string FileName, byte[] FileBytes)
        {
            this.Type = Type;
            this.Date = Date;
            this.Object = Object;
            this.Message = Message;
            this.FileName = FileName ?? "";
            this.FileBytes = FileBytes;

            if(Type == "incoming")
            {
                this.Sender = new Contact(SenderId);
                this.Receiver = new Employee(ReceiverId);
                this.Treated = false;
            }
            else if(Type == "outgoing")
            {
                this.Sender = new Employee(SenderId);
                this.Receiver = new Contact(ReceiverId);
                this.Treated = true;
            }

        }

        public ArrayList GetAttachments()
        {
            if (Attachments == null)
                Attachments = new Attachment[0];

            ArrayList ListAttachments = new ArrayList();
            ListAttachments.AddRange(Attachments);

            return ListAttachments;
        } 
        
        public void AddAttachment(Attachment newAttachment)
        {
            if (Attachments == null)
                Attachments = new Attachment[0];

            ArrayList ListAttachments = new ArrayList();
            ListAttachments.AddRange(Attachments);

            if (!ListAttachments.Contains(newAttachment))
                ListAttachments.Add(newAttachment);

            Attachments = ListAttachments.ToArray(typeof(Attachment)) as Attachment[];
        }
        
        public bool Save()
        {
            if (Id == 0)
            {
                // I have to check here if sender and receiver exists before saving the mail !!!
                
                string query = "INSERT INTO [dbo].[Mail] (Type, Treated, Date, IdSender, IdReceiver, Object, Message, FileName, FileBytes) VALUES (@type, @treated, @date, @idSender, @idReceiver, @object, @message, @fn, @fb)";
                string[] parameters = new string[] { "type", "treated", "date", "idSender", "idReceiver", "object", "message", "fn", "fb"};
                object[] values = new object[] { Type, Treated , Date, Sender.Id, Receiver.Id, Object, Message, FileName, (object)FileBytes ?? 0};

                int nbrRowsChanged = Database.ExecuteDmlQuery(query, parameters, values);
                
                if (nbrRowsChanged == 1)
                {
                    SqlDataReader reader = Database.ExecuteDqlQuery("SELECT TOP 1 Id FROM [dbo].[Mail] ORDER BY Id Desc");
                    reader.Read();
                    Id = reader.GetInt32(0);
                    foreach (Attachment attachment in GetAttachments())
                    {
                        if (!attachment.Save(Id))
                            return false;
                    }
                    
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

    }
}