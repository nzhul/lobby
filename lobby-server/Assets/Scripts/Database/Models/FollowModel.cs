using MongoDB.Bson;
using MongoDB.Driver;

public class FollowModel
{
    public ObjectId _id;

    public MongoDBRef Sender;

    public MongoDBRef Target;
}