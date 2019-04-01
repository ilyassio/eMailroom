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

        public ActionResult ListMails()
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

                query = "SELECT Id, Position, Firstname, Lastname FROM [Employee] WHERE Position <> 'Secretary' AND Position <> 'Technician' ORDER BY Position, Lastname, Firstname";
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
                            return Json(new { signedIn = true, alertClass = "success", alertMessage = "Mail added successfully" });
                        else
                            return Json(new { signedIn = true, alertClass = "danger", alertMessage = "Saving mail in the database failed, contacts the administrator" });
                    }
                    else
                        return Json(new { signedIn = true, alertClass = "warning", alertMessage = "Please fill in all required fields" });
                }
                catch
                {
                   return Json(new { signedIn = true, alertClass = "danger", alertMessage = "An error has occurred, contact the administrator" });
                }              

            }
            else
                return Json(new { signedIn = false });
        }

        [HttpPost]
        public string ViewMail()
        {
            return "";            
        }
    }
}