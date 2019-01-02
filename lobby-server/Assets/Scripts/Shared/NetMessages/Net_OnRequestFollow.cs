using System;
using System.Collections.Generic;

[Serializable]
public class Net_OnRequestFollow : NetMessage
{
    public Net_OnRequestFollow()
    {
        OperationCode = NetOperationCode.OnRequestFollow;
    }

    public List<Account> Follows { get; set; }
}