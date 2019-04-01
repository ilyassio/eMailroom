using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eMailroom.Models;
using Newtonsoft.Json;

namespace eMailroom.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            if (Session["EmployeeId"] == null)
                return RedirectToAction("Authentication", "Home");
            else
                return RedirectToAction("Index", (string)Session["EmployeeType"]);
        }

        public ActionResult Authentication()
        {
            if (Session["EmployeeId"] != null)
                return RedirectToAction("Index", (string)Session["EmployeeType"]);
            else
                return View();
        }

        [HttpPost]
        public ActionResult Signin()
        {
            if (Request.Form["id"] != "" && Request.Form["password"] != "")
            {
                Employee employee = new Employee(Request.Form["id"]);

                var employeeInfo = employee.Signin(Request.Form["password"]);

                if (employeeInfo.Item1 != null && employeeInfo.Item2 != null)
                {
                    Session["EmployeeId"] = employeeInfo.Item1;

                    if (employeeInfo.Item2 == "Secretary")
                        Session["EmployeeType"] = employeeInfo.Item2;
                    else
                        Session["EmployeeType"] = "Employee";

                    return RedirectToAction("Index");
                }
                else
                    return RedirectToAction("Authentication", "Home", new { alertClass = "danger", alertMessage = "Id and/or password are invalid" });
            }
            else
                return RedirectToAction("Authentication", "Home", new { alertClass = "warning", alertMessage = "Please fill in all fields" });
        }

        public ActionResult Signout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Authentication");
        }

        [HttpPost]
        public JsonResult ChangePassword()
        {
            if (Session["EmployeeId"] != null)
            {
                Employee employee = new Employee(Session["EmployeeId"].ToString());

                if (Request.Form["oldPassword"] != "" && Request.Form["newPassword"] != "" && Request.Form["confirmPassword"] != "")
                {
                    if (Request.Form["newPassword"] != Request.Form["confirmPassword"])
                        return Json(new { signedIn = true, alertClass = "danger", alertMessage = "Password confirmation doesn't match" });

                    else
                    {
                        int res = employee.ChangePassword(Request.Form["oldPassword"], Request.Form["newPassword"]);

                        if (res == 1)
                            return Json(new { signedIn = true, alertClass = "success", alertMessage = "Password successfully changed" });
                        else if (res == 0)
                            return Json(new { signedIn = true, alertClass = "danger", alertMessage = "Old password incorrect" });
                        else
                            return Json(new { signedIn = true, alertClass = "danger", alertMessage = "An error has occurred, contact the administrator" });
                    }
                }
                else
                    return Json(new { signedIn = true, alertClass = "danger", alertMessage = "Please complete all fields" });
            }
            else
                return Json(new { signedIn = false });

        }

    }
}