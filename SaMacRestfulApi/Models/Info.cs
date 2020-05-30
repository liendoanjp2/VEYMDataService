using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SaMacRestfulApi.Models
{
    public class Info
    {
        public ObjectId _id { get; set; }
        public string masterId { get; set; }
        public int numberOfTrainings { get; set; }
        public int numberOfUsers { get; set; }
        public List<string> listOfAdminEmailAddresses { get; set; }
    }
}