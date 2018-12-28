using System;

[Serializable]
public abstract class NetMessage
{
    public byte OperationCode { get; set; }

    public NetMessage()
    {
        OperationCode = NetOperationCode.None;
    }
}

public static class NetOperationCode
{
    public const int None = 0;

    public const int CreateAccount = 1;

    public const int LoginRequest = 2;

    public const int OnCreateAccount = 3;

    public const int OnLoginRequest = 4;
}