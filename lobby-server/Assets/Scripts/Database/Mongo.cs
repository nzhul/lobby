using System;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UnityEngine;

public class Mongo
{
    private const string CONNECTION_STRING = "mongodb://nzhul:dido123@ds145184.mlab.com:45184/lobbydb";
    private const string DATABASE_NAME = "lobbydb";

    private MongoClient client;
    private MongoServer server;
    private MongoDatabase db;

    private MongoCollection<AccountModel> accounts;
    private MongoCollection<FollowModel> follows;

    public void Init()
    {
        client = new MongoClient(CONNECTION_STRING);
        server = client.GetServer();
        db = server.GetDatabase(DATABASE_NAME);

        // This is where we would initialize collections
        accounts = db.GetCollection<AccountModel>("account");
        follows = db.GetCollection<FollowModel>("follow");

        Debug.Log("Database has been initialized");
    }

    public void Shutdown()
    {
        client = null;
        server.Shutdown();
        db = null;
    }

    #region Update
    #endregion

    #region Insert
    public bool InsertAccount(string username, string password, string email)
    {
        if (!Utility.IsEmail(email))
        {
            Debug.Log(email + " is not a email");
            return false;
        }

        if (!Utility.IsUsername(username))
        {
            Debug.Log(username + " is not a username");
            return false;
        }

        // Check if account already exists
        if (FindAccountByEmail(email) != null)
        {
            Debug.Log(email + " is already being used");
            return false;
        }

        AccountModel newAccount = new AccountModel();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;
        newAccount.Discriminator = "0000";

        // Roll for unique Discriminator

        int rollCount = 0;
        while (FindAccountByUsernameAndDiscriminator(newAccount.Username, newAccount.Discriminator) != null)
        {
            newAccount.Discriminator = UnityEngine.Random.Range(0, 9999).ToString("0000");

            rollCount++;
            if (rollCount > 1000)
            {
                Debug.Log("We rolled to many times, suggest username change!");
                return false;
            }
        }

        accounts.Insert(newAccount);

        return true;
    }
    public AccountModel LoginAccount(string usernameOrEmail, string password, int connectionId, string token)
    {
        AccountModel dbAccount = null;
        IMongoQuery query = null;

        // Find my account
        if (Utility.IsEmail(usernameOrEmail))
        {
            // if i logged in using a email
            query = Query.And(
                Query<AccountModel>.EQ(u => u.Email, usernameOrEmail),
                Query<AccountModel>.EQ(u => u.ShaPassword, password));

            dbAccount = accounts.FindOne(query);
        }
        else
        {
            // if i logged in using username#discriminator

            string[] data = usernameOrEmail.Split('#');
            if (data[1] != null)
            {
                query = Query.And(
                            Query<AccountModel>.EQ(u => u.Username, data[0]),
                            Query<AccountModel>.EQ(u => u.Discriminator, data[1]),
                            Query<AccountModel>.EQ(u => u.ShaPassword, password));

                dbAccount = accounts.FindOne(query);
            }
        }

        if (dbAccount != null)
        {
            // we found the account, lets login
            dbAccount.ActiveConnection = connectionId;
            dbAccount.Token = token;
            dbAccount.Status = 1;
            dbAccount.LastLogin = DateTime.UtcNow;

            accounts.Update(query, Update<AccountModel>.Replace(dbAccount));
        }
        else
        {
            // Didn't find anything, invalid credentials
        }

        return dbAccount;

    }

    #endregion

    #region Fetch

    public AccountModel FindAccountByEmail(string email)
    {
        var query = Query<AccountModel>.EQ(u => u.Email, email);
        return accounts.FindOne(query);
    }

    public AccountModel FindAccountByUsernameAndDiscriminator(string username, string discriminator)
    {
        var query = Query.And(
                        Query<AccountModel>.EQ(u => u.Username, username),
                        Query<AccountModel>.EQ(u => u.Discriminator, discriminator));

        return accounts.FindOne(query);
    }

    #endregion

    #region Delete
    #endregion
}
