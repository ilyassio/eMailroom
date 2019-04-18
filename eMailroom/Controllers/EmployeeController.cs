using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace eMailroom.Controllers
{
    public class EmployeeController : Controller
    {

        public ActionResult Index()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Employee")
                return RedirectToAction("Authentication", "Home");
            else
                return RedirectToAction("ListMails", "Employee");
        }

        public ActionResult ListMails()
        {
            if (Session["EmployeeId"] == null || (string)Session["EmployeeType"] != "Employee")
                return RedirectToAction("Authentication", "Home");
            else
                return View();
        }

    }
}