using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMailroom.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string SocialReason { get; set; }
        public string Type { get; set; }
        public string SectorActivity { get; set; }
        public string Note { get; set; }
        public Contact MainContact { get; set; }

        public Company(int id)
        {

        }

        public Company (string SocialReason, string Type, string SectorActivity, string Note, string IdMainContact)
        {

        }
    }

    
}