using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdonisUI.Controls;
using Mail.FormModel;
using Mail.Sqllite;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace Mail.Xamls;

public partial class AddUser : AdonisWindow
{
    public AddUser()
    {
        InitializeComponent();
    }

    private void OnlyNumbers(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private void AuthCheck(object sender, RoutedEventArgs e)
    {
        Auth.IsEnabled = true;
    }

    private void AuthUnCheck(object sender, RoutedEventArgs e)
    {
        Auth.IsEnabled = false;
    }

    private void AuthChecksmtp(object sender, RoutedEventArgs e)
    {
        Authsmtp.IsEnabled = true;
    }

    private void AuthUnChecksmtp(object sender, RoutedEventArgs e)
    {
        Authsmtp.IsEnabled = false;
    }

    private void AuthCheckimap(object sender, RoutedEventArgs e)
    {
        Authimap.IsEnabled = true;
    }

    private void AuthUnCheckimap(object sender, RoutedEventArgs e)
    {
        Authimap.IsEnabled = false;
    }

    private async Task okAsync()
    {
        ok.IsEnabled = false;
        var loading = new Loading();
        loading.Show();
        SecureSocketOptions ssoSmtp;
        SecureSocketOptions ssoImap;
        if (Validation.GetHasError(email) || Validation.GetHasError(smtp) || Validation.GetHasError(IsSslsmtp) ||
            Validation.GetHasError(imap) || Validation.GetHasError(IsSslimap))
        {
            loading.Close();
            ok.IsEnabled = true;
            MessageBox.Show(lang.lang.adduser_notfilled, null, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            ssoSmtp = IsSslsmtp.SelectedValuePath switch
            {
                "None" => SecureSocketOptions.None,
                "Auto" => SecureSocketOptions.Auto,
                "Ssl" => SecureSocketOptions.SslOnConnect,
                "StartTls" => SecureSocketOptions.StartTls,
                "StartTls or Tls" => SecureSocketOptions.StartTlsWhenAvailable,
                _ => SecureSocketOptions.Auto
            };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (mysender, certificate, chain, sslPolicyErrors) => { return true; };
            await client.ConnectAsync(smtp.Text, int.Parse(portsmtp.Text), ssoSmtp);
            if (IsAuthsmtp.IsChecked ?? false)
            {
                await client.AuthenticateAsync(UserNameSmtp.Text, Passwordsmtp.Password);
            }

            await client.DisconnectAsync(true);
        }
        catch (Exception exception)
        {
            loading.Close();
            ok.IsEnabled = true;
            MessageBox.Show( lang.lang.adduser_smtperror + exception.Message, null, MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            ssoImap = IsSslsmtp.SelectedValuePath switch
            {
                "Auto" => SecureSocketOptions.Auto,
                "None" => SecureSocketOptions.None,
                "Ssl" => SecureSocketOptions.SslOnConnect,
                "StartTls" => SecureSocketOptions.StartTls,
                "StartTls or Tls" => SecureSocketOptions.StartTlsWhenAvailable,
                _ => SecureSocketOptions.Auto
            };

            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (mysender, certificate, chain, sslPolicyErrors) => { return true; };
            await client.ConnectAsync(imap.Text, int.Parse(portimap.Text), ssoImap);
            if (IsAuthimap.IsChecked ?? false)
            {
                await client.AuthenticateAsync(UserNameImap.Text, Passwordimap.Password);
            }

            await client.DisconnectAsync(true);
        }
        catch (Exception exception)
        {
            loading.Close();
            ok.IsEnabled = true;
            MessageBox.Show(lang.lang.adduser_imaperror + exception.Message, null, MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        SqlContextWrapper.exec(context =>
        {
            context.Users.Add(new User()
            {
                MailAdr = email.Text,
                HostSmtp = smtp.Text,
                PortSmtp = int.Parse(portsmtp.Text),
                IsUseAuthSmtp = IsAuthsmtp.IsChecked ?? false,
                UsernameSmtp = UserNameSmtp.Text,
                PasswordSmtp = Passwordsmtp.Password,
                IsSslSmtp = ssoSmtp,
                HostImap = imap.Text,
                PortImap = int.Parse(portimap.Text),
                IsUseAuthImap = IsAuthimap.IsChecked ?? false,
                UsernameImap = UserNameImap.Text,
                PasswordImap = Passwordimap.Password,
                IsSslImap = ssoImap
            });
            context.SaveChanges();
        });
        loading.Close();
        ok.IsEnabled = true;
        foreach (var window in App.Current.Windows)
        {
            if (window is MainWindow)
            {
                ((MainWindow)window).CheckUsersAsync();
            }
        }
        Close();
    }

    private void Ok(object sender, MouseButtonEventArgs e)
    {
        ok.Focus();
        okAsync();
    }

    private void PasswordChanged(object sender, RoutedEventArgs e)
    {   
        Passwordimap.Password = ((PasswordBox)sender).Password;
        Passwordsmtp.Password = ((PasswordBox)sender).Password;
    }
}