using System;

[Serializable]
public class Net_CreateAccount : NetMessage
{
    public Net_CreateAccount()
    {
        OperationCode = NetOperationCode.CreateAccount;
    }

    public string Username { get; set; }

    public string Password { get; set; }

    public string Email { get; set; }
}
