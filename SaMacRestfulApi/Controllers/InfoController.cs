using MongoDB.Bson;
using MongoDB.Driver;
using SaMacRestfulApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace VEYMServices.Controllers
{
    public class InfoController : ApiController
    {
        // POST api/<controller>
        public void Post()
        {
            List<string> adminUsersList = new List<string>();
            adminUsersList.Add("philips.nguyen@veym.net");
            MongoConnections.getMongoInfoCollection().InsertOne(new Info() { masterId = "0000", numberOfTrainings = 0, numberOfUsers = 0, listOfAdminEmailAddresses = adminUsersList });
        }

        // GET api/<controller>
        public HttpResponseMessage Get()
        {
            List<Info> listOfInfo = MongoConnections.getMongoInfoCollection().Find(new BsonDocument()).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, listOfInfo[0]);
        }
    }
}