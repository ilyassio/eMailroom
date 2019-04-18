using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace eMailroom.Models
{
    public class Mail
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool Treated { get; set; }

        public string Date { get; set; }
        public string EntryDate { get; set; }
        public Person Sender { get; set; }
        public Person Receiver { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public string FileName { get; set; }
        public byte[] FileBytes { get; set; }
        public Attachment[] Attachments;

        public Mail(string Type, string Date, string SenderId, string ReceiverId, string Subject, string Message, string FileName, byte[] FileBytes)
        {
            this.Type = Type;
            this.Date = Date;
            this.Subject = Subject;
            this.Message = Message;
            this.FileName = FileName ?? "";
            this.FileBytes = FileBytes;

            if (Type == "incoming")
            {
                this.Sender = new Contact(SenderId);
                this.Receiver = new Employee(ReceiverId);
                this.Treated = false;
            }
            else
            {
                this.Sender = new Employee(SenderId);
                this.Receiver = new Contact(ReceiverId);
                this.Treated = true;
            }

        }

        public Mail(int Id, string Type, string Date, string EntryDate, string IdSender, string IdReceiver, string Subject, string Message)
        {
            this.Id = Id;
            this.Type = Type;
            this.Date = Date;
            this.EntryDate = EntryDate;

            if (Type == "incoming")
            {
                Sender = new Contact(IdSender);
                Receiver = new Employee(IdReceiver);
            }
            else if(Type == "outgoing")
            {
                Sender = new Employee(IdSender);
                Receiver = new Contact(IdReceiver);
            }

            this.Subject = Subject;
            this.Message = Message;
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

                string query = "INSERT INTO [dbo].[Mail] (Type, Treated, Date, IdSender, IdReceiver, Subject, Message, FileName, FileBytes) VALUES (@type, @treated, @date, @idSender, @idReceiver, @subject, @message, @fn, @fb)";
                string[] parameters = new string[] { "type", "treated", "date", "idSender", "idReceiver", "subject", "message", "fn", "fb" };
                object[] values = new object[] { Type, Treated, Date, Sender.Id, Receiver.Id, Subject, Message, FileName, (object)FileBytes ?? 0 };

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

                    MailAddress addressFrom = new MailAddress("apptest.emailroom@gmail.com");
                    MailAddress addressTo = new MailAddress("ilmakroum@gmail.com");
                    MailMessage message = new MailMessage(addressFrom, addressTo)
                    {
                        Subject = this.Subject,
                        Body = this.Message
                    };

                    foreach (var attachment in Attachments)
                        message.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(attachment.FileBytes), Path.GetFileName(attachment.FileName)));

                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("apptest.emailroom@gmail.com", "Casablanca@2019")
                    };

                    smtp.Send(message);

                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }


        public static List<Mail> GetMails(string Id)
        {
            List<Mail> mails = new List<Mail>();

            string query = "SELECT Id, Type, Date, EntryDate, IdSender, IdReceiver, Subject, Message FROM [Mail]";
            SqlDataReader reader = Database.ExecuteDqlQuery(query);

            if (reader != null && reader.HasRows)
            {
                try
                {
                    while (reader.Read())
                    {
                        mails.Add(new Mail(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2).ToShortDateString(), reader.GetDateTime(3).ToString(), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7)));
                    }
                }
                catch
                {

                }
                finally
                {
                    Database.Close();
                }
            }
            
            return mails;
        }

        public static List<Mail> GetMails(string idSecretary, string type, string date, string entryDate, string idSender, string idReceiver, string subject, string message)
        {
            List<Mail> mails = new List<Mail>();

            string query = "SELECT Id, Type, Date, EntryDate, IdSender, IdReceiver, Subject, Message FROM [Mail] WHERE Type = IIF(@type = '', Type, @type) AND IdSender = IIF(@idSender = '', IdSender, @idSender) AND IdReceiver = IIF(@idReceiver = '', IdReceiver, @idReceiver)";
            string[] _params = new string[] { "type", "idSender", "idReceiver" };
            object[] paramsValues = new object[] { type, idSender, idReceiver};
            SqlDataReader reader = Database.ExecuteDqlQuery(query, _params, paramsValues);

            if (reader != null && reader.HasRows)
            {
                try
                {
                    while (reader.Read())
                    {
                        mails.Add(new Mail(reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2).ToShortDateString(), reader.GetDateTime(3).ToString(), reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7)));
                    }
                }
                catch
                {

                }
                finally
                {
                    Database.Close();
                }
            }

            return mails;
        }
    }
}