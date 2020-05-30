using MongoDB.Driver;
using SaMacRestfulApi.Models;
using System;
using VEYMServices.Models;

namespace VEYMServices
{
    public class MongoConnections
    {
        private static IMongoDatabase mongoDatabaseConnection;
        private static IMongoCollection<User> mongoUsersCollection;
        private static IMongoCollection<Training> mongoTrainingCollection;
        private static IMongoCollection<Info> mongoInfoCollection;

        public MongoConnections()
        {
            InitalizeMongoConnection();
        }

        private void InitalizeMongoConnection()
        {
            //Connect with mongo
            String connectionString = "INSERT CONNECTION STRING HERE";
            MongoClient clientConnection = new MongoClient(connectionString);

            mongoDatabaseConnection = clientConnection.GetDatabase("TNTT");
            mongoUsersCollection = mongoDatabaseConnection.GetCollection<User>("Users");
            mongoTrainingCollection = mongoDatabaseConnection.GetCollection<Training>("Training");
            mongoInfoCollection = mongoDatabaseConnection.GetCollection<Info>("Info");
        }

        public static IMongoDatabase getMongoDatabaseConnection()
        {
            return mongoDatabaseConnection;
        }

        public static IMongoCollection<User> getMongoUsersCollection()
        {
            return mongoUsersCollection;
        }

        public static IMongoCollection<Training> getMongoTrainingCollection()
        {
            return mongoTrainingCollection;
        }

        public static IMongoCollection<Info> getMongoInfoCollection()
        {
            return mongoInfoCollection;
        }
    }
}