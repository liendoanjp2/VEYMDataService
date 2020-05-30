using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace VEYMServices.Models
{
    public class User
    {
        public ObjectId _id { get; set; }
        public string membershipID { get; set; }
        public string emailAddress { get; set; }
        public string name { get; set; }

        public string rank { get; set; }

        public string chapter { get; set; }
        public string leaugeOfChapters { get; set; }
        public List<String> trainingCamps { get; set; }
    }
}