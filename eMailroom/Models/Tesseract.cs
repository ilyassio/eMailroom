using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace eMailroom.Models
{
    public static class Tesseract
    {
        public static void Run(string cmd)
        {
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
    }
}