using MongoDB.Bson;
using MongoDB.Driver;
using VEYMServices.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace VEYMServices.Controllers
{
    [Authorize]
    public class UsersController : ApiController
    {
        // 2 Functionalities
        // Gets a user from the Users Collection based on MembershipID

        // Body Fields: membershipID
        // Response: Whole User Document

        // GET api/<controller>
        public HttpResponseMessage Get([FromBody]User user)
        {
            // user must have a Membership ID, if not return everything
            if (string.IsNullOrEmpty(user?.membershipID))
            {
                List<User> allUsers = MongoConnections.getMongoUsersCollection().Find(new BsonDocument()).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, allUsers);
            }

            var getFilter = Builders<User>.Filter.Eq("membershipID", user.membershipID);
            if (getFilter != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, MongoConnections.getMongoUsersCollection().Find(getFilter).FirstOrDefault());
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }


        // Adds a user to the Users Collection

        // Body Fields: membershipID, name, rank, chapter, leaugeOfChapters
        // Response: Whole User Document (Will populate _id and a blank trainingCamps with that)

        // Notes: Check first if the user already exists in the database with the same membershipID. If there is DO NOT ADD

        // PUT api/<controller>
        public HttpResponseMessage Post([FromBody]User user)
        {
            // Input must have a Membership ID
            if (string.IsNullOrEmpty(user?.membershipID))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Check if user exists already
            var getFilter = Builders<User>.Filter.Eq("membershipID", user.membershipID);

            if (MongoConnections.getMongoUsersCollection().Find(getFilter).FirstOrDefault() != null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "A user already exists with MemberID: " + user.membershipID);
            }

            // User with membershipID passed in is not in the database, add it!

            //Create a new empty List
            user.trainingCamps = new List<string>();
            MongoConnections.getMongoUsersCollection().InsertOne(user);

            return Request.CreateResponse(HttpStatusCode.Created, user);
        }

        // Deletes a user in the Users Collection based on Membership ID

        // Body Fields: membershipID
        // Response: Deleted confirmation

        // DELETE api/<controller>
        public HttpResponseMessage Delete([FromBody]User user)
        {
            // input must have a Membership ID
            if (string.IsNullOrEmpty(user?.membershipID))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var deleteFilter = Builders<User>.Filter.Eq("membershipID", user.membershipID);
            if (deleteFilter != null)
            {
                // Find the user in the database
                // Loop though all of their trainings
                // go to the training and loop though the sign up list

                // Refacto this into meathod
                // Delete out of the sign up List
                // Update everyone's priority 

                MongoConnections.getMongoUsersCollection().DeleteOne(deleteFilter);
                return Request.CreateResponse(HttpStatusCode.OK, user.membershipID + " has been deleted from the database.");

                
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}