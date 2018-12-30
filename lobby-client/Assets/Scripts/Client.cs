using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    public static Client Instance { get; private set; }

    private const int MAX_USER = 100;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;

    private const string SERVER_IP = "127.0.0.1";

    private byte reliableChannel;
    private int connectionId;
    private int hostId;
    private byte error;

    public Account self;
    private string token;
    private bool isStarted;


    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    private void Update()
    {
        UpdatemessagePump();
    }

    public void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig connectionConfig = new ConnectionConfig();
        this.reliableChannel = connectionConfig.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(connectionConfig, MAX_USER);

        // Client only code
        this.hostId = NetworkTransport.AddHost(topo, 0);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Web Client
        NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);
        Debug.Log("Connecting from Web");
#else
        // Standalone Client
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, PORT, 0, out error);
        Debug.Log("Connecting from standalone");
#endif

        Debug.Log(string.Format("Attempting to connect on {0} ...", SERVER_IP));

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

        int recievingHostId; // Is this from Web ? Or standalone
        int connectionId; // Which user is sending me this ?
        int channelId; // Which lane is he sending that message from

        byte[] recBuffer = new byte[BYTE_SIZE];
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
                Debug.Log("We have connected to the server");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("We have been disconnected");
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
    private void OnData(int connectionId, int channelId, int recievingHostId, NetMessage msg)
    {
        switch (msg.OperationCode)
        {
            case NetOperationCode.None:
                Debug.Log("Unexpected NETOperationCode");
                break;
            case NetOperationCode.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)msg);
                break;
            case NetOperationCode.OnLoginRequest:
                OnLoginRequest((Net_OnLoginRequest)msg);
                break;
            default:
                break;
        }
    }

    private void OnLoginRequest(Net_OnLoginRequest msg)
    {
        LobbyScene.Instance.ChangeAuthenticationMessage(msg.Information);

        if (msg.Success != 1)
        {
            // Unable to login
            LobbyScene.Instance.EnableInputs();
        }
        else
        {
            // Successfull login
            // This is where we are going to save data about ourself
            self = new Account();
            self.ActiveConnection = msg.ConnectionId;
            self.Username = msg.Username;
            self.Discriminator = msg.Discriminator;

            token = msg.Token;

            UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
        }
    }

    private void OnCreateAccount(Net_OnCreateAccount msg)
    {
        LobbyScene.Instance.EnableInputs();
        LobbyScene.Instance.ChangeAuthenticationMessage(msg.Information);
    }
    #endregion

    #region Send
    public void SendServer(NetMessage msg)
    {
        //  this is where we hold our data
        byte[] buffer = new byte[BYTE_SIZE];

        // this is where yuo will crush your data into byte[]

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }

    public void SendCreateAccount(string username, string password, string email)
    {
        if (!Utility.IsUsername(username))
        {
            // Invalid username
            LobbyScene.Instance.ChangeAuthenticationMessage("Username is invalid!");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (!Utility.IsUsername(username))
        {
            // Invalid username
            LobbyScene.Instance.ChangeAuthenticationMessage("Email is invalid!");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            // Invalid username
            LobbyScene.Instance.ChangeAuthenticationMessage("Password is empty!");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        Net_CreateAccount msg = new Net_CreateAccount();

        msg.Username = username;
        msg.Password = Utility.Sha256FromString(password);
        msg.Email = email;

        LobbyScene.Instance.ChangeAuthenticationMessage("Sending request ...");
        SendServer(msg);
    }

    public void SendLoginRequest(string usernameOrEmail, string password)
    {
        if (!Utility.IsUsernameAndDiscriminator(usernameOrEmail) && !Utility.IsEmail(usernameOrEmail))
        {
            // Invalid username
            LobbyScene.Instance.ChangeAuthenticationMessage("Email or Username#Discriminator is invalid");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            // Invalid username
            LobbyScene.Instance.ChangeAuthenticationMessage("Password is empty!");
            LobbyScene.Instance.EnableInputs();
            return;
        }

        Net_LoginRequest msg = new Net_LoginRequest();
        msg.UsernameOrEmail = usernameOrEmail;
        msg.Password = Utility.Sha256FromString(password);

        LobbyScene.Instance.ChangeAuthenticationMessage("Sending login request ...");
        SendServer(msg);
    }

    #endregion
}
