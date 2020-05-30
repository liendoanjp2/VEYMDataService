using MongoDB.Bson;
using SaMacRestfulApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VEYMServices.Models
{
    public class Training
    {
        public ObjectId _id { get; set; }
        public string trainingID { get; set; }
        public string trainingName { get; set; }
        public int trainingUserCapacity { get; set; }
        public int currentTrainingUserCount{ get; set; }

        public List<TrainingUserItem> signupList { get; set; }
    }
}