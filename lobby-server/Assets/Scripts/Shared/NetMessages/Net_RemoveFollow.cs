using System;

[Serializable]
public class Net_RemoveFollow : NetMessage
{
    public Net_RemoveFollow()
    {
        OperationCode = NetOperationCode.RemoveFollow;
    }
}
