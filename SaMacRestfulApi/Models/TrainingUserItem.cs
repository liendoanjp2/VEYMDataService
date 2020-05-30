using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaMacRestfulApi.Models
{
    public class TrainingUserItem
    {
        public string membershipID { get; set; }

        public int placeInQueue { get; set; }

        public string timeSignedUp { get; set; }

        public string status { get; set; }
    }
}