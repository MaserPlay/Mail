using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using System.Windows.Controls;
using System.Windows.Input;
using AdonisUI.Controls;
using Mail.Connection;
using Mail.Sqllite;
using MailKit;
using MimeKit;

namespace Mail.Xamls;

public partial class CreateMessage : AdonisWindow
{
    public CreateMessage(string? to, string? theme, string? body) : this()
    {
        TextBox1.Text = body ?? "";
        this.theme.Text = theme ?? "";
        this.to.Text = to ?? "";
    }

    public CreateMessage()
    {
        InitializeComponent();
        foreach (var user in SqlContextWrapper<List<User>>.exec(func: context => context.Users.ToList()))
        {
            from.Items.Add(new ComboBoxItem() { Content = user.MailAdr });
        }
    }

    private void Bold(object sender, MouseButtonEventArgs e)
    {
        TextBox1.SelectedText = "<B>" + TextBox1.SelectedText + "</B>";
    }

    private void Italic(object sender, MouseButtonEventArgs e)
    {
        TextBox1.SelectedText = "<I>" + TextBox1.SelectedText + "</I>";
    }

    private void Underline(object sender, MouseButtonEventArgs e)
    {
        TextBox1.SelectedText = "<U>" + TextBox1.SelectedText + "</U>";
    }

    private void Send(object sender, MouseButtonEventArgs e)
    {
        SendAsync();
    }

    private async Task SendAsync()
    {
        try
        {
            ok.IsEnabled = false;
            if (Validation.GetHasError(to))
            {
                ok.IsEnabled = true;
                MessageBox.Show(lang.lang.adduser_notfilled, null, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loading = new Loading();
            loading.Show();
            var fromadr = (string)((ComboBoxItem)from.SelectedItem).Content;
            var connection = new SmtpConnection(SqlContextWrapper<User>.exec(func: context =>
                context.Users.Where(u => u.MailAdr == fromadr).FirstOrDefault()));
            await connection.OpenAsync();
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromadr, fromadr));
            message.To.Add(new MailboxAddress(to.Text, to.Text));
            message.Subject = theme.Text;

            message.Body = new TextPart("html")
            {
                Text = TextBox1.Text
            };
            await connection.SendAsync(message);
            await connection.CloseAsync();
            loading.Close();
            ok.IsEnabled = true;
            Close();
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}