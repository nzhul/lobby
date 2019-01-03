using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{

    private const int MAX_USER = 100;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;

    private byte reliableChannel;
    private int hostId;
    private int webHostId;
    private byte error;

    private Mongo db;

    private bool isStarted;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void Update()
    {
        UpdatemessagePump();
    }

    public void Init()
    {
        db = new Mongo();
        db.Init();

        NetworkTransport.Init();

        ConnectionConfig connectionConfig = new ConnectionConfig();
        reliableChannel = connectionConfig.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(connectionConfig, MAX_USER);

        // SERVER only code
        hostId = NetworkTransport.AddHost(topo, PORT, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, WEB_PORT, null);

        Debug.Log(string.Format("Opening connection on port {0} and webport {1}", PORT, WEB_PORT));

        isStarted = true;
    }

    public void Shutdown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }

    public void UpdatemessagePump()
    {
        if (!isStarted)
        {
            return;
        }
        // Which lane is he sending that message from

        byte[] recBuffer = new byte[BYTE_SIZE];

        int recievingHostId;
        int connectionId;
        int channelId;
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recievingHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);

        switch (type)
        {
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMessage msg = (NetMessage)formatter.Deserialize(ms);

                OnData(connectionId, channelId, recievingHostId, msg);
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected throught host {1}", connectionId, recievingHostId));
                break;
            case NetworkEventType.DisconnectEvent:
                DisconnectEvent(recievingHostId, connectionId);
                break;
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type!");
                break;
            default:
                break;
        }
    }

    #region OnData
    private void DisconnectEvent(int recievingHostId, int connectionId)
    {
        Debug.Log(string.Format("User {0} has disconnected :(", connectionId));

        // Get a reference to the connected Account
        AccountModel dbAccount = db.FindAccountByConnectionId(connectionId);


        // Just making sure he was indeed authenticated
        if (dbAccount == null)
        {
            return;
        }

        db.UpdateAccountAfterDisconnection(dbAccount.Email);

        // Prepare and send our update message
        Net_FollowUpdate msg = new Net_FollowUpdate();
        AccountModel updatedAccount = db.FindAccountByEmail(dbAccount.Email);
        msg.Follow = updatedAccount.GetAccount();

        foreach (var f in db.FindAllFollowBy(dbAccount.Email))
        {
            if (f.ActiveConnection == 0)
            {
                continue;
            }

            SendClient(recievingHostId, connectionId, msg);
        }
    }

    private void OnData(int connectionId, int channelId, int recievingHostId, NetMessage msg)
    {
        switch (msg.OperationCode)
        {
            case NetOperationCode.None:
                Debug.Log("Unexpected NETOperationCode");
                break;
            case NetOperationCode.CreateAccount:
                CreateAccount(connectionId, channelId, recievingHostId, (Net_CreateAccount)msg);
                break;
            case NetOperationCode.LoginRequest:
                LoginRequest(connectionId, channelId, recievingHostId, (Net_LoginRequest)msg);
                break;
            case NetOperationCode.AddFollow:
                AddFollow(connectionId, channelId, recievingHostId, (Net_AddFollow)msg);
                break;
            case NetOperationCode.RemoveFollow:
                RemoveFollow(connectionId, channelId, recievingHostId, (Net_RemoveFollow)msg);
                break;
            case NetOperationCode.RequestFollow:
                RequestFollow(connectionId, channelId, recievingHostId, (Net_RequestFollow)msg);
                break;
            default:
                break;
        }
    }

    private void RequestFollow(int connectionId, int channelId, int recievingHostId, Net_RequestFollow msg)
    {
        Net_OnRequestFollow rmsg = new Net_OnRequestFollow();

        rmsg.Follows = db.FindAllFollowFrom(msg.Token);

        SendClient(recievingHostId, connectionId, rmsg);
    }

    private void RemoveFollow(int connectionId, int channelId, int recievingHostId, Net_RemoveFollow msg)
    {
        db.RemoveFollow(msg.Token, msg.UsernameDiscriminator);
    }

    private void AddFollow(int connectionId, int channelId, int recievingHostId, Net_AddFollow msg)
    {
        Net_OnAddFollow rmsg = new Net_OnAddFollow();

        if (db.InsertFollow(msg.Token, msg.UsernameDiscriminatorOrEmail))
        {
            if (Utility.IsEmail(msg.UsernameDiscriminatorOrEmail))
            {
                // this is email
                rmsg.Follow = db.FindAccountByEmail(msg.UsernameDiscriminatorOrEmail).GetAccount();
            }
            else
            {
                // this is username
                string[] data = msg.UsernameDiscriminatorOrEmail.Split('#');
                if (data[1] == null)
                {
                    return;
                }

                rmsg.Follow = db.FindAccountByUsernameAndDiscriminator(data[0], data[1]).GetAccount();
            }
        }

        SendClient(recievingHostId, connectionId, rmsg);
    }

    private void CreateAccount(int connectionId, int channelId, int recievingHostId, Net_CreateAccount msg)
    {
        Net_OnCreateAccount rmsg = new Net_OnCreateAccount();

        if (db.InsertAccount(msg.Username, msg.Password, msg.Email))
        {
            rmsg.Success = 1;
            rmsg.Information = "Account was created!";
        }
        else
        {
            rmsg.Success = 0;
            rmsg.Information = "There was an error creating the account!";
        }

        SendClient(recievingHostId, connectionId, rmsg);
    }

    private void LoginRequest(int connectionId, int channelId, int recievingHostId, Net_LoginRequest msg)
    {
        string randomToken = Utility.GenerateRandom(64);
        AccountModel dbAccount = db.LoginAccount(msg.UsernameOrEmail, msg.Password, connectionId, randomToken);
        Net_OnLoginRequest rmsg = new Net_OnLoginRequest();

        if (dbAccount != null)
        {

            rmsg.Success = 1;
            rmsg.Information = "You've been logged in as " + dbAccount.Username;
            rmsg.Username = dbAccount.Username;
            rmsg.Discriminator = dbAccount.Discriminator;
            rmsg.Token = randomToken;
            rmsg.ConnectionId = connectionId;

            // Prepare and send our update message
            Net_FollowUpdate fu = new Net_FollowUpdate();
            fu.Follow = dbAccount.GetAccount();

            foreach (var f in db.FindAllFollowBy(dbAccount.Email))
            {
                if (f.ActiveConnection == 0)
                {
                    continue;
                }

                SendClient(recievingHostId, connectionId, fu);
            }
        }
        else
        {
            rmsg.Success = 0;
        }

        SendClient(recievingHostId, connectionId, rmsg);
    }

    #endregion

    #region Send
    public void SendClient(int recHost, int connectionId, NetMessage msg)
    {
        //  this is where we hold our data
        byte[] buffer = new byte[BYTE_SIZE];

        // this is where yuo will crush your data into byte[]

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        if (recHost == 0)
        {
            NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
        }
        else
        {
            NetworkTransport.Send(webHostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
        }


    }
    #endregion
}
