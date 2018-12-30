using System;

[Serializable]
public class Net_RequestFollow : NetMessage
{
    public Net_RequestFollow()
    {
        OperationCode = NetOperationCode.RequestFollow;
    }

    public string Token { get; set; }
}
