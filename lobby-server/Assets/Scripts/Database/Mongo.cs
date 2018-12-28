using MongoDB.Driver;
using UnityEngine;

public class Mongo
{
    private const string CONNECTION_STRING = "mongodb://nzhul:dido123@ds145184.mlab.com:45184/lobbydb";
    private const string DATABASE_NAME = "lobbydb";

    private MongoClient client;
    private MongoServer server;
    private MongoDatabase db;

    private MongoCollection accounts;

    public void Init()
    {
        client = new MongoClient(CONNECTION_STRING);
        server = client.GetServer();
        db = server.GetDatabase(DATABASE_NAME);

        // This is where we would initialize collections
        accounts = db.GetCollection<AccountModel>("account");

        Debug.Log("Database has been initialized");
    }

    public void Shutdown()
    {
        client = null;
        server.Shutdown();
        db = null;
    }

    #region Fetch

    #endregion

    #region Update
    #endregion

    #region Insert
    public bool InsertAccount(string username, string password, string email)
    {
        AccountModel newAccount = new AccountModel();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;
        newAccount.Discriminator = "0000";

        accounts.Insert(newAccount);

        return true;
    }
    #endregion

    #region Delete
    #endregion
}
