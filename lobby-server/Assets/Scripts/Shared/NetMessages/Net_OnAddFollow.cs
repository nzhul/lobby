using System;

[Serializable]
public class Net_OnAddFollow : NetMessage
{
    public Net_OnAddFollow()
    {
        OperationCode = NetOperationCode.OnAddFollow;
    }

    public Account Follow { get; set; }
}