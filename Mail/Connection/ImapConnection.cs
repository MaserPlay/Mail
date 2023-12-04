using System.Threading.Tasks;
using Mail.Sqllite;
using MailKit.Net.Imap;

namespace Mail.Connection;

public class ImapConnection : ImapClient
{
    private readonly User _user;

    public ImapConnection(User user)
    {
        _user = user;
    }

    /// <summary>
    /// Open Connection
    /// </summary>
    public void Open()
    {
        base.CheckCertificateRevocation = false;
        base.ServerCertificateValidationCallback = (s, c, h, e) => true;
        base.Connect(_user.HostImap, _user.PortImap, _user.IsSslImap);
        if (_user.IsUseAuthImap)
        {
            base.Authenticate(_user.UsernameImap, _user.PasswordImap);
        }
    }

    /// <summary>
    /// Open Connection Async
    /// </summary>
    public async Task OpenAsync()
    {
        //TimeToDisconnect.Start(this, _user.Id);
        base.CheckCertificateRevocation = false;
        base.ServerCertificateValidationCallback = (s, c, h, e) => true;
        await base.ConnectAsync(_user.HostImap, _user.PortImap, _user.IsSslImap);
        if (_user.IsUseAuthImap)
        {
            await base.AuthenticateAsync(_user.UsernameImap, _user.PasswordImap);
        }
    }

    /// <summary>
    /// Close Connection Async
    /// </summary>
    public async Task CloseAsync()
    {
        await base.DisconnectAsync(true);
    }

    /// <summary>
    /// Close Connection
    /// </summary>
    public void Close()
    {
        base.Disconnect(true);
    }
}