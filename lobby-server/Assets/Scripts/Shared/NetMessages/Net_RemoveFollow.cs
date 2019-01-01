using System;

[Serializable]
public class Net_RemoveFollow : NetMessage
{
    public Net_RemoveFollow()
    {
        OperationCode = NetOperationCode.RemoveFollow;
    }

    public string Token { get; set; }

    public string UsernameDiscriminator { get; set; }
}
