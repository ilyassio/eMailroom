using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMailroom.Models
{
    public abstract class Person
    {
        public string Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public char Gender { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}