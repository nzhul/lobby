using System;

[Serializable]
public class Account
{
    public int ActiveConnection { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public byte Status { get; set; }
}