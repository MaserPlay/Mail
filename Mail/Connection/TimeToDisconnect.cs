using System;
using System.Collections.Generic;
using System.Threading;
using MailKit.Net.Imap;

namespace Mail.Connection;

public abstract class TimeToDisconnect
{
    private static Dictionary<int, ImapClient> _dictionary = new();

    public static void Start(ImapClient client, int UserId)
    {
        if (!_dictionary.ContainsKey(UserId))
        {
            var tc = new TimerCallback(End);
            var t = new Timer(tc, new UIdClient(client, UserId), Settings1.Default.ConnectionTime, -1);
        }
        else
        {
            
        }
    }

    public static void End(object obj)
    {
        var objj = (UIdClient)obj;
        objj.client.DisconnectAsync(true);
    }
}

internal record class UIdClient(ImapClient client, int UserId);