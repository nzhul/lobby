using System;

[Serializable]
public class Net_FollowUpdate : NetMessage
{
    public Net_FollowUpdate()
    {
        OperationCode = NetOperationCode.FollowUpdate;
    }

    public Account Follow { get; set; }
}