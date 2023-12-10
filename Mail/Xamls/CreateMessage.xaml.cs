using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using AdonisUI.Controls;
using Mail.Connection;
using Mail.Sqllite;
using MailKit;
using MimeKit;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace Mail.Xamls;

public partial class CreateMessage : AdonisWindow
{
    public CreateMessage(string? to = null, string? theme = null, string? body = null) : this()
    {
        //TextBox1.Text = body ?? "";
        this.theme.Text = theme ?? "";
        this.to.Text = to ?? "";
    }

    private CreateMessage()
    {
        InitializeComponent();
        foreach (var user in SqlContextWrapper<List<User>>.exec(func: context => context.Users.ToList()))
        {
            from.Items.Add(new ComboBoxItem() { Content = user.MailAdr });
        }
    }

    private void Bold(object sender, MouseButtonEventArgs e)
    {
        //string Richtextvalue = new TextRange(TextBox1.Document.ContentStart, TextBox1.Document.ContentEnd).Text;
        string text = TextBox1.Selection.Text;
        TextBox1.Selection.Text = "";
        TextBox1.BeginChange();
        new Run(text, TextBox1.Selection.Start)
        {
            FontWeight = FontWeights.Bold
        };

        TextBox1.EndChange();
        //TextBox1.SelectedText = "<B>" + TextBox1.SelectedText + "</B>";
    }

    private void Italic(object sender, MouseButtonEventArgs e)
    {
        string text = TextBox1.Selection.Text;
        TextBox1.Selection.Text = "";
        TextBox1.BeginChange();
        new Run(text, TextBox1.Selection.Start)
        {
            FontStyle = FontStyles.Italic
        };

        TextBox1.EndChange();
        //TextBox1.SelectedText = "<I>" + TextBox1.SelectedText + "</I>";
    }

    private void Underline(object sender, MouseButtonEventArgs e)
    {
        string text = TextBox1.Selection.Text;
        TextBox1.Selection.Text = "";
        TextBox1.BeginChange();
        new Run(text, TextBox1.Selection.Start)
        {
            TextDecorations = TextDecorations.Underline
        };

        TextBox1.EndChange();
        //TextBox1.SelectedText = "<U>" + TextBox1.SelectedText + "</U>";
    }

    private void Normal(object sender, MouseButtonEventArgs e)
    {
        string text = TextBox1.Selection.Text;
        TextBox1.Selection.Text = "";
        TextBox1.BeginChange();
        new Run(text, TextBox1.Selection.Start);

        TextBox1.EndChange();
    }

    private void Send(object sender, MouseButtonEventArgs e)
    {
        SendAsync();
    }

    private async Task SendAsync()
    {
        try
        {
            ok.Focus();
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
            TextRange range = new TextRange(TextBox1.Document.ContentStart, TextBox1.Document.ContentEnd);
            MemoryStream stream = new MemoryStream();
            range.Save(stream, DataFormats.Xaml);
            string xamlText = Encoding.UTF8.GetString(stream.ToArray());
            XDocument xDocument = new XDocument(XDocument.Parse(xamlText));
            xDocument.Root.Name = "p";
            xDocument.Root.RemoveAttributes();
            if (xDocument.Root.Elements().Count() > 0)
            {
                xDocument.Root.Elements().First().Name = "p";
                foreach (var xElement in xDocument.Root.Elements().First().Elements())
                {
                    if (xElement.Attribute("FontWeight") != null &&
                        xElement.Attribute("FontWeight").Value.ToString() == "Bold")
                    {
                        xElement.Attributes().Remove();
                        xElement.Name = "B";
                    }
                    else if (xElement.Attribute("TextDecorations") != null &&
                             xElement.Attribute("TextDecorations").Value.ToString() == "Underline")
                    {
                        xElement.Attributes().Remove();
                        xElement.Name = "U";
                    }
                    else
                    {
                        xElement.Attributes().Remove();
                        xElement.Name = "span";
                    }
                }
            }

            message.Body = new TextPart("html")
            {
                Text = xDocument.Document.ToString()
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


    private void CreateMessage_OnContentRendered(object? sender, EventArgs e)
    {
        if (from.Items.Count > 0) return;
        MessageBox.Show("Need to add user!", "Need to add user!", MessageBoxButton.OK,
            MessageBoxImage.Warning);
        Close();
    }
}