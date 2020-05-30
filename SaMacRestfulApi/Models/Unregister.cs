using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaMacRestfulApi.Models
{
    public class Unregister
    {
        public string membershipID { get; set; }
        public string trainingID { get; set; }
        public bool sendToTheBack { get; set; }
    }
}