using eMailroom.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IronOcr;
using IronPdf;
using System.Diagnostics;
using System.Drawing;

namespace eMailroom.Controllers
{
    public class SecretaryController : Controller
    {
        public ActionResult Index()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return RedirectToAction("Tasks");
        }

        public ActionResult Mails()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

        public ActionResult NewMail()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

        public ActionResult Companys()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

        public ActionResult Contacts()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

        public ActionResult Employees()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

        public ActionResult Tasks()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Secretary")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

        [HttpPost]
        public string GetParties()
        {
            string employees = "[null]";
            string contacts = "[null]";

            if (Session["EmployeeId"] != null && (string)Session["EmployeeType"] == "Secretary")
            {
                string query;
                SqlDataReader reader;
                DataTable table;

                query = "SELECT Id, Position, Firstname, Lastname FROM [Employee] WHERE Position <> 'Secretary' ORDER BY Position, Lastname, Firstname";
                reader = Database.ExecuteDqlQuery(query);

                if (reader != null && reader.HasRows)
                {
                    table = new DataTable();
                    table.Load(reader);
                    employees = JsonConvert.SerializeObject(table);
                }

                query = "SELECT CT.Id, CT.Firstname, CT.Lastname, COALESCE('('+CP.SocialReason+')','') AS SocialReason FROM [Contact] CT LEFT JOIN [Company] CP ON CT.Company = CP.Id";
                reader = Database.ExecuteDqlQuery(query);

                if (reader != null && reader.HasRows)
                {
                    table = new DataTable();
                    table.Load(reader);
                    contacts = JsonConvert.SerializeObject(table);
                }

                Database.Close();
            }
            return "{\"Employees\":" + employees + ",\"Contacts\":" + contacts + "}";
        }

        [HttpPost]
        public JsonResult AddMail()
        {
            if (Session["EmployeeId"] != null && (string)Session["EmployeeType"] == "Secretary")
            {
                try
                {
                    if (Request.Form["type"] != "" && Request.Form["date"] != "" && Request.Form["sender"] != "" && Request.Form["receiver"] != "" && Request.Form["message"] != "")
                    {

                        byte[] fileBytes = null;
                        string fileName = null;
                        Stream fs = null;
                        BinaryReader br = null;

                        try
                        {
                            if (Request.Files["mail"].ContentLength > 0 && Path.GetExtension(Request.Files["mail"].FileName).ToUpper() == ".PDF")
                            {
                                fileName = Path.GetFileName(Request.Files["mail"].FileName);
                                fs = Request.Files["mail"].InputStream;
                                br = new BinaryReader(fs);
                                fileBytes = br.ReadBytes((Int32)fs.Length);
                            }
                        }
                        catch
                        {
                            fileName = null;
                            fileBytes = null;

                        }
                        finally
                        {
                            if (fs != null)
                                fs.Close();
                            if (br != null)
                                br.Close();
                        }

                        Mail mail = new Mail(Request.Form["type"], Request.Form["date"], Request.Form["sender"], Request.Form["receiver"], Request.Form["object"], Request.Form["message"], fileName, fileBytes);

                        foreach (HttpPostedFileBase attachement in Request.Files.GetMultiple("attachements[]"))
                        {
                            try
                            {
                                if (attachement.ContentLength > 0 && Path.GetExtension(attachement.FileName).ToUpper() == ".PDF")
                                {
                                    fileName = Path.GetFileName(attachement.FileName);
                                    fs = attachement.InputStream;
                                    br = new BinaryReader(fs);
                                    fileBytes = br.ReadBytes((Int32)fs.Length);

                                    mail.AddAttachment(new Attachment(fileName, fileBytes));
                                }
                            }
                            catch
                            {

                            }
                            finally
                            {
                                if (fs != null)
                                    fs.Close();
                                if (br != null)
                                    br.Close();
                            }
                        }

                        if (mail.Save())
                            return Json(new { session = true, alertClass = "success", alertMessage = Resources.Resource.MailAddedSuccess });
                        else
                            return Json(new { session = true, alertClass = "danger", alertMessage = "Saving mail in the database failed, contacts the administrator" });
                    }
                    else
                        return Json(new { session = true, alertClass = "warning", alertMessage = Resources.Resource.EmptyRequiredField });
                }
                catch
                {
                   return Json(new { session = true, alertClass = "danger", alertMessage = Resources.Resource.Error });
                }              
            }
            else
                return Json(new { session = false });
        }

        [HttpPost]
        public JsonResult ApplyOcr()
        {
            if (Session["EmployeeId"] != null && (string)Session["EmployeeType"] == "Secretary")
            {
                if (new[] { "ara", "eng", "fra" }.Contains(Request.Form["lang"]) )
                {
                    Session["MailLanguage"] = Request.Form["lang"];

                    try
                    {
                        string fileName = Path.ChangeExtension(Path.GetFileName(Request.Files["mail"].FileName), ".png");
                        Stream fs = Request.Files["mail"].InputStream;
                        PdfDocument PDF = new PdfDocument(fs);
                        PDF.ToPngImages(Server.MapPath("\\Temp") + "\\mail_scan.png");
                        
                        string cmd = "cmd /C \"\"" + Server.MapPath("\\Tesseract") + "\\Tesseract-OCR\\tesseract.exe\" --tessdata-dir \"" + Server.MapPath("\\Tesseract") + "\\tessdata\" \"" + Server.MapPath("\\Temp") + "\\mail_scan.png\" \"" + Server.MapPath("\\Temp") + "\\mail_scan\" -l "+ Request.Form["lang"] + " pdf\"";
                        Process p;
                        p = new Process();
                        string[] args = cmd.Split(new char[] { ' ' });
                        p.StartInfo.FileName = args[0];
                        int startPoint = cmd.IndexOf(' ');
                        string s = cmd.Substring(startPoint, cmd.Length - startPoint);
                        p.StartInfo.Arguments = s;
                        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        string output = p.StandardOutput.ReadToEnd();
                        p.Dispose();
                    }
                    catch
                    {
                        return Json(new { session = true, success = false, message = Resources.Resource.Error });
                    }

                    return Json(new { session = true, success = true, url = "http://" + Request.Url.Authority + "/Temp/mail_scan.pdf" });
                }
                
                return Json(new { session = true, success = false, message = Resources.Resource.Error });
            }

            return Json(new { session = false });
        }

        public JsonResult AutoFill()
        {
            if (Session["EmployeeId"] != null && (string)Session["EmployeeType"] == "Secretary")
            {
                string mailDate = "";
                string mailObject = "";
                string mailMessage = "";

                Bitmap mailImg = new Bitmap(Server.MapPath("\\Temp\\mail_scan.png"));
                Bitmap CroppedImage;

                // Date
                CroppedImage = mailImg.Clone(new System.Drawing.Rectangle(572, 13, 608, 130), mailImg.PixelFormat); // X, Y, Width, Height
                CroppedImage.Save(Server.MapPath("\\Temp\\mail_scan_date.png"));

                // Object
                CroppedImage = mailImg.Clone(new System.Drawing.Rectangle(8, 408, 1167, 90), mailImg.PixelFormat);
                CroppedImage.Save(Server.MapPath("\\Temp\\mail_scan_object.png"));

                // Message
                CroppedImage = mailImg.Clone(new System.Drawing.Rectangle(8, 504, 1167, 800), mailImg.PixelFormat);
                CroppedImage.Save(Server.MapPath("\\Temp\\mail_scan_message.png"));
                try
                {
                    foreach (string CroppedImageName in new List<string> { "mail_scan_date", "mail_scan_object", "mail_scan_message" })
                    {

                        string cmd = "cmd /C \"\"" + Server.MapPath("\\Tesseract") + "\\Tesseract-OCR\\tesseract.exe\" --tessdata-dir \"" + Server.MapPath("\\Tesseract") + "\\tessdata\" \"" + Server.MapPath("\\Temp") + "\\" + CroppedImageName + ".png\" \"" + Server.MapPath("\\Temp") + "\\" + CroppedImageName + "\" -l " + (string)Session["MailLanguage"] + " txt\"";

                        Process p = new Process();
                        string[] args = cmd.Split(new char[] { ' ' });
                        p.StartInfo.FileName = args[0];
                        int startPoint = cmd.IndexOf(' ');
                        string s = cmd.Substring(startPoint, cmd.Length - startPoint);
                        p.StartInfo.Arguments = s;
                        p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.Start();
                        string output = p.StandardOutput.ReadToEnd();
                        p.Dispose();

                    }


                    mailDate = System.IO.File.ReadAllText(Server.MapPath("\\Temp\\mail_scan_date.txt"));
                    int index = mailDate.IndexOf("/")-2;
                    mailDate = mailDate.Substring(index, 10);
                    mailDate = (DateTime.ParseExact(mailDate, "dd/mm/yyyy", null)).ToString("yyyy-mm-dd");

                    mailObject = System.IO.File.ReadAllText(Server.MapPath("\\Temp\\mail_scan_object.txt")).Replace("Objet :", "");

                    mailMessage = System.IO.File.ReadAllText(Server.MapPath("\\Temp\\mail_scan_message.txt"));
                    mailMessage = mailMessage.Replace("\n\n", "-double-space-here-");
                    mailMessage = mailMessage.Replace("\n", " ");
                    mailMessage = mailMessage.Replace("-double-space-here-", "\n\n");

                    return Json(new { session = true, success = true, mailDate, mailObject, mailMessage });
                }
                catch
                {
                    return Json(new { session = true, success = false, message = Resources.Resource.Error });
                }
            }
            else
                return Json(new { session = false });
        }
    }
}