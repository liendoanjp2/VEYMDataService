using MongoDB.Bson;
using MongoDB.Driver;
using SaMacRestfulApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VEYMServices.Models;

namespace VEYMServices.Controllers
{
    [Authorize]
    public class TrainingController : ApiController
    {

        //Patch a training
        //TODO

        // Get a training camp based off trainingID

        // GET api/<controller>/5
        public HttpResponseMessage Get([FromBody]Training trainng)
        {
            // user must have a Membership ID, if not return everything
            if (string.IsNullOrEmpty(trainng?.trainingID))
            {
                List<Training> allTrainings = MongoConnections.getMongoTrainingCollection().Find(new BsonDocument()).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, allTrainings);
            }

            var getFilter = Builders<Training>.Filter.Eq("trainingID", trainng.trainingID);
            if (getFilter != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, MongoConnections.getMongoTrainingCollection().Find(getFilter).FirstOrDefault());
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        // Add a new training Camp

        // Body Fields: trainingID, trainingName, trainingUserCapacity
        // Response: Whole Training Document (Will populate _id, currentTrainingUserCount = 0 , empty usersList) 

        // Notes: Check first if the Training  already exists in the database with the same trainingID. If there is DO NOT ADD

        // POST api/<controller>
        public HttpResponseMessage Post([FromBody]Training training)
        {
            // training must have a trainingName
            if (string.IsNullOrEmpty(training?.trainingName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Get the master Info
            // Find the the master info deocument by hardcode 0000. (setup)
            var getFilterInfo = Builders<Info>.Filter.Eq("masterId", "0000");

            // Get the Info, by the filter
            Info infoFound = MongoConnections.getMongoInfoCollection().Find(getFilterInfo).FirstOrDefault();

            int nextTrainingID = infoFound.numberOfTrainings + 1;

            // prep the update of infoFound
            infoFound.numberOfTrainings = nextTrainingID;

            // hard code these fields to initalize in the database
            training.currentTrainingUserCount = 0;
            training.trainingID = nextTrainingID.ToString().PadLeft(4,'0');
            training.signupList = new List<TrainingUserItem>();

            MongoConnections.getMongoTrainingCollection().InsertOne(training);

            // Update Info with the new number of trainings
            var updateInfo = Builders<Info>.Update.Inc("numberOfTrainings",1);
            MongoConnections.getMongoInfoCollection().UpdateOne(getFilterInfo, updateInfo);

            return Request.CreateResponse(HttpStatusCode.OK, training);
        }

        // Put a user into the signup list

        // Body Fields: trainingID, membershipID
        // Response: Success or Not Success. 
        // Change: Training signupList is added with a TrainingUserItem(membershipID, placeInQueue, timeSignedUp)
        //         User trainingCamps is added with the trainingID 

        // Note: User must not have the trainingID in trainingCamps

        // PUT api/<controller>/5
        public HttpResponseMessage Put([FromBody]Register register)
        {
            // Add the user to the registered list in training

            // Find the training by trainingID. (setup)
            var getFilterTraining = Builders<Training>.Filter.Eq("trainingID", register.trainingID);
            // Find the user by membership ID. (setup)
            var getFilterUser = Builders<User>.Filter.Eq("membershipID", register.membershipID);

            if (getFilterTraining != null && getFilterUser != null)
            {
                // Get the training, by the filter
                Training trainingFound = MongoConnections.getMongoTrainingCollection().Find(getFilterTraining).FirstOrDefault();
                // Get the user, by the filter
                User userFound = MongoConnections.getMongoUsersCollection().Find(getFilterUser).FirstOrDefault();

                //This is used for updating currentTrainingUserCount in Training and placeInQueue in TrainingUserItem
                int nextCount = (trainingFound.currentTrainingUserCount + 1);

                // if the User already has the trainingID in trainingCamps, DO NOT ADD
                if (userFound.trainingCamps.Contains(register.trainingID))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "User is already signed up for this training");
                }
                else
                {
                    // Update signupList in the Training object with a new TrainingUserItem with membership ID, Place in the Queue, time signed up, incomplete status
                    var updateUsersListTraining = Builders<Training>.Update.Push("signupList", new TrainingUserItem { membershipID = register.membershipID, placeInQueue = nextCount, timeSignedUp = DateTime.Now.ToString(), status =  Constants.TRAINING_STATUS_PENDING_REGISTRATION });
                    MongoConnections.getMongoTrainingCollection().UpdateOne(getFilterTraining, updateUsersListTraining);

                    // Update CurrentTrainingUserCount with the number is users
                    var updateCurrentTrainingUserCountTraining = Builders<Training>.Update.Set("currentTrainingUserCount", nextCount.ToString());
                    MongoConnections.getMongoTrainingCollection().UpdateOne(getFilterTraining, updateCurrentTrainingUserCountTraining);

                    // Update TrainingCamps of a user with the training ID
                    var updateUser = Builders<User>.Update.Push("trainingCamps", register.trainingID);
                    MongoConnections.getMongoUsersCollection().UpdateOne(getFilterUser, updateUser);

                    return Request.CreateResponse(HttpStatusCode.OK, "User has been put on the signup list Priority: " + nextCount);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something is incorrect with the request");
            }
        }

        // 2 Functionalities

        // ***Funtionality 1. Send the User to the back of the signup List
        // Remove a User based off of membershipID from the signupList in Training based off of trainingID
        // Update All TrainingUserItems to account for the removed user
        // ReAdd the user to the end of the signup List

        // Body Fields: trainingID, membershipID, SendToTheBack(boolean) True
        // Response: Success or Not Success. 

        // ***Funtionality 2. Fully delete the TrainingUserItem from the signupList from the training based off of trainingID
        // Fully delete the trainingID found in trainingCamps from User based off of membershipID

        // Body Fields: trainingID, membershipID, SendToTheBack(boolean) False
        // Response: Success or Not Success. 

        // DELETE api/<controller>/5
        public HttpResponseMessage Delete([FromBody]Unregister unregister)
        {

            // Find the training by trainingID. (setup)
            var getFilterTraining = Builders<Training>.Filter.Eq("trainingID", unregister.trainingID);
            // Find the user by membership ID. (setup)
            var getFilterUser = Builders<User>.Filter.Eq("membershipID", unregister.membershipID);

            if (getFilterTraining != null && getFilterUser != null)
            {
                // Get the training, by the filter
                Training trainingFound = MongoConnections.getMongoTrainingCollection().Find(getFilterTraining).FirstOrDefault();
                // Get the user, by the filter
                User userFound = MongoConnections.getMongoUsersCollection().Find(getFilterUser).FirstOrDefault();

                // if the User already has the trainingID in trainingCamps
                if (userFound.trainingCamps.Contains(unregister.trainingID))
                {
                    if (unregister.sendToTheBack)
                    {
                        // Get the full list of TrainingUserItems
                        List<TrainingUserItem> signupListFromTraining = trainingFound.signupList;

                        int counter = 0;
                        int oldPlaceOnList = 0;

                        // Grab only the one we want to update
                        foreach(TrainingUserItem trainingUserItem in signupListFromTraining)
                        {
                            if(trainingUserItem.membershipID == unregister.membershipID)
                            {
                                //memberfound! Update it!
                                oldPlaceOnList = signupListFromTraining[counter].placeInQueue;
                                signupListFromTraining[counter].placeInQueue = trainingFound.currentTrainingUserCount + 1;
                                break;
                            }

                            counter++;
                        }

                        counter = 0;
                        //update the others based off the user added to the back of the signup list
                        foreach (TrainingUserItem trainingUserItem in signupListFromTraining)
                        {
                            if(trainingUserItem.placeInQueue > oldPlaceOnList)
                            {
                                signupListFromTraining[counter].placeInQueue--;
                            }

                            counter++;
                        }

                        var updateCurrentTrainingUserCountTraining = Builders<Training>.Update.Set("signupList", signupListFromTraining);
                        MongoConnections.getMongoTrainingCollection().UpdateOne(getFilterTraining, updateCurrentTrainingUserCountTraining);

                        return Request.CreateResponse(HttpStatusCode.OK, "User has been sent to the back with new priority: " + trainingFound.currentTrainingUserCount);
                    }
                    else
                    {
                        // Get the full list of TrainingUserItems
                        List<TrainingUserItem> signupListFromTraining = trainingFound.signupList;

                        int counter = 0;
                        int oldPlaceOnList = 0;

                        // Grab only the one we want to update
                        foreach (TrainingUserItem trainingUserItem in signupListFromTraining)
                        {
                            if (trainingUserItem.membershipID == unregister.membershipID)
                            {
                                //memberfound! Delete it!
                                oldPlaceOnList = trainingUserItem.placeInQueue;
                                signupListFromTraining.RemoveAt(counter);
                                break;
                            }

                            counter++;
                        }

                        counter = 0;
                        //update the others based off the user added to the back of the signup list
                        foreach (TrainingUserItem trainingUserItem in signupListFromTraining)
                        {
                            if (trainingUserItem.placeInQueue > oldPlaceOnList)
                            {
                                signupListFromTraining[counter].placeInQueue--;
                            }

                            counter++;
                        }

                        var updateSignUpListTraining = Builders<Training>.Update.Set("signupList", signupListFromTraining);
                        MongoConnections.getMongoTrainingCollection().UpdateOne(getFilterTraining, updateSignUpListTraining);

                        //deincrementSizeOfTraining
                        var updateCurrentTrainingUserCountTraining = Builders<Training>.Update.Set("currentTrainingUserCount", trainingFound.currentTrainingUserCount - 1);
                        MongoConnections.getMongoTrainingCollection().UpdateOne(getFilterTraining, updateCurrentTrainingUserCountTraining);

                        counter = 0;
                        //remove the training out of the User's training list
                        foreach (String training in userFound.trainingCamps)
                        {
                            userFound.trainingCamps.RemoveAt(counter);
                            counter++;

                            break;
                        }

                        var updateUserTrainingCamps = Builders<User>.Update.Set("trainingCamps", userFound.trainingCamps);
                        MongoConnections.getMongoUsersCollection().UpdateOne(getFilterUser, updateUserTrainingCamps);

                        //Functionality 2 full remove
                        return Request.CreateResponse(HttpStatusCode.OK, "User has been unregistered");
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "User is not even signed up to this training");
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Something is incorrect with the request");
            }
        }

    }
}