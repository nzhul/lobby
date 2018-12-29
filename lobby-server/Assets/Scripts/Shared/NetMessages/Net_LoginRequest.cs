using System;

[Serializable]
public class Net_LoginRequest : NetMessage
{
    public Net_LoginRequest()
    {
        OperationCode = NetOperationCode.LoginRequest;
    }

    public string UsernameOrEmail { get; set; }

    public string Password { get; set; }
}