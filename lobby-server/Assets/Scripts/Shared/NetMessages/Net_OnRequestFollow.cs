using System.Collections.Generic;

public class Net_OnRequestFollow : NetMessage
{
    public Net_OnRequestFollow()
    {
        OperationCode = NetOperationCode.OnRequestFollow;
    }

    public List<Account> Follow { get; set; }
}