using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mail.Connection;
using Mail.Sqllite;
using MailKit;
using MailKit.Search;

namespace Mail.Xamls;

public partial class MoveToFolder : Window
{
    private readonly Sqllite.Mail _mail;
    private readonly Sqllite.User _user;
    private List<string> listindexes = new();

    public MoveToFolder(Sqllite.Mail mail, User user)
    {
        InitializeComponent();
        _mail = mail;
        _user = user;
    }

    private void MoveToFolder_OnLoaded(object sender, RoutedEventArgs e)
    {
        foreach (var folder in SqlContextWrapper<List<Folder>>.exec(func: context =>
                     context.Folders.Where(f => f.UserId == _user.Id).ToList()))
        {
            var name = folder.Name;
            if ((folder.Attribute & FolderAttributes.Inbox) == FolderAttributes.Inbox)
            {
                name = lang.lang.folder_inbox;
            }
            else if ((folder.Attribute & FolderAttributes.Drafts) == FolderAttributes.Drafts)
            {
                name = lang.lang.folder_draft;
            }
            else if ((folder.Attribute & FolderAttributes.Archive) == FolderAttributes.Archive)
            {
                name = lang.lang.folder_archive;
            }
            else if ((folder.Attribute & FolderAttributes.Trash) == FolderAttributes.Trash)
            {
                name = lang.lang.folder_trash;
            }
            else if ((folder.Attribute & FolderAttributes.Junk) == FolderAttributes.Junk)
            {
                name = lang.lang.folder_junk;
            }
            View.Items.Add(name);
            listindexes.Add(folder.Name);
        }
    }

    private void Ok(object sender, MouseButtonEventArgs e)
    {
        if (View.SelectedValue != null)
        {
            ImapConnection connection = new ImapConnection(_user);
            connection.Open();
            var folderTo = connection.GetFolder(listindexes[View.SelectedIndex]);
            var folder = connection.GetFolder(_mail.ParentFolderId);
            folder.Open(FolderAccess.ReadWrite);
            var uids = folder.Search(SearchQuery.HeaderContains("Message-Id", _mail.IdMail));
            folder.MoveTo(uids, folderTo);
            connection.Close();
            SqlContextWrapper.exec(func: context =>
            {
                context.Mails.Where(m => m.IdMail == _mail.IdMail).FirstOrDefault().ParentFolderId = folderTo.Name;
                context.SaveChanges();
            });
        }
        foreach (var window in App.Current.Windows)
        {
            if (window is MainWindow mainWindow)
            {
                mainWindow.MessageBox.Items.Clear();
            }
        }

        Close();
    }
}