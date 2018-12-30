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

    public const int AddFollow = 5;
    public const int RemoveFollow = 6;
    public const int RequestFollow = 7;

    public const int OnAddFollow = 8;
    public const int OnRequestFollow = 9;
}