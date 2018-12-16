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

        // SERVER only code
        this.hostId = NetworkTransport.AddHost(topo, PORT, null);
        this.webHostId = NetworkTransport.AddWebsocketHost(topo, WEB_PORT, null);

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
                Debug.Log(string.Format("User {0} has connected throught host {1}", connectionId, recievingHostId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected :(", connectionId));
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
}
