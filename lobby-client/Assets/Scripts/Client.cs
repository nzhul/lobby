using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{

    private const int MAX_USER = 100;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;

    private const string SERVER_IP = "127.0.0.1";

    private byte reliableChannel;
    private int connectionId;
    private int hostId;
    private byte error;

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
                Debug.Log("Data");
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

    #region Send
    public void SendServer()
    {
        //  this is where we hold our data
        byte[] buffer = new byte[BYTE_SIZE];

        // this is where yuo will crush your data into byte[]
        buffer[0] = 255;

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }
    #endregion
}
