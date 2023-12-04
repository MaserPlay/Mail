using System.Threading.Tasks;
using Mail.Sqllite;
using MailKit.Net.Smtp;

namespace Mail.Connection;

public class SmtpConnection : SmtpClient
{
    private readonly User _user;

    public SmtpConnection(User user)
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
        base.Connect(_user.HostSmtp, _user.PortSmtp, _user.IsSslSmtp);
        if (_user.IsUseAuthSmtp)
        {
            base.Authenticate(_user.UsernameSmtp, _user.PasswordSmtp);
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
        await base.ConnectAsync(_user.HostSmtp, _user.PortSmtp, _user.IsSslSmtp);
        if (_user.IsUseAuthSmtp)
        {
            await base.AuthenticateAsync(_user.UsernameSmtp, _user.PasswordSmtp);
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