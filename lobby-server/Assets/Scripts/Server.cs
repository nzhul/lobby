﻿using System.IO;
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

        NetworkEventType type = NetworkTransport.Receive(out int recievingHostId, out int connectionId, out int channelId, recBuffer, BYTE_SIZE, out int dataSize, out error);

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
    #region OnData
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
            default:
                break;
        }
    }

    private void CreateAccount(int connectionId, int channelId, int recievingHostId, Net_CreateAccount msg)
    {
        Debug.Log(string.Format("{0},{1},{2}", msg.Username, msg.Password, msg.Email));
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
