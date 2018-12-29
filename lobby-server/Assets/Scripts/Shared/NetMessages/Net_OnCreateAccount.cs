using System;

[Serializable]
public class Net_OnCreateAccount : NetMessage
{
    public Net_OnCreateAccount()
    {
        OperationCode = NetOperationCode.OnCreateAccount;
    }

    public byte Success { get; set; }

    public string Information { get; set; }
}
