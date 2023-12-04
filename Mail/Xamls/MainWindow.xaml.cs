using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AdonisUI.Controls;
using AdonisUI.Extensions;
using Mail.FormModel;
using Mail;
using Mail.Connection;
using Mail.Sqllite;
using Mail.Xamls;
using Mail.Sqllite;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32;
using Notifications.Wpf.Core;
using Mail1 = Mail.Sqllite.Mail;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace Mail.Xamls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        private readonly Sqllite.Mail _defaultMail = new Sqllite.Mail
        {
            Id = 0,
            IdMail = "default",
            ParentFolderId = "defaultmailbox",
            Unread = true,
            From = "admin@maserplay.ru",
            To = "user@maserplay.ru",
            Theme = "Welcome to MailApp!",
            Html = "<h1>Welcome to MailAppWpf!</h1>\n<p>\n  This is a <b> beta </b> version of the email client based on wpf. All emails\n  open in Internet Explorer, I know that, I want to fix it. But it's very\n  difficult, and I think it's impossible. You can add your mailbox by clicking\n  on Users -> add a user. If you find any errors, just send them to help -> send\n  feedback.\n</p>\n"
        };

        private readonly Sqllite.User _defaultUser = new Sqllite.User
        {
            Id = 0,
            MailAdr = "user@maserplay.ru",
            HostSmtp = "maserplay.ru",
            PortSmtp = 993,
            IsUseAuthSmtp = false,
            UsernameSmtp = null,
            PasswordSmtp = null,
            IsSslSmtp = SecureSocketOptions.None,
            HostImap = "maserplay.ru",
            PortImap = 776,
            IsUseAuthImap = false,
            UsernameImap = "user",
            PasswordImap = null,
            IsSslImap = SecureSocketOptions.None,
            HasUnread = true
        };

        private SoundPlayer player = new(Settings1.Default.PickMail);

        public MainWindow()
        {
            InitializeComponent();
            Update();
        }

        private void OpenAddUser(object sender, object e)
        {
            var addUser = new AddUser();
            addUser.ShowDialog();
        }

        private void RefreshFolders(object? sender, object? e)
        {
            usersList.Items.Clear();
            var button = new Button()
            {
                Content = lang.lang.message_write
            };
            button.PreviewMouseDown += CreateMessage;
            usersList.Items.Add(button);
            var listUser = SqlContextWrapper<List<User>>.exec(context => context.Users.ToList());
            var listFolder = SqlContextWrapper<List<Folder>>.exec(context => context.Folders.ToList());
            foreach (var u in listUser)
            {
                var item = new TreeViewItem
                {
                    Header = u.MailAdr
                };
                usersList.Items.Add(item);
                foreach (var folder in listFolder.Where(folder => folder.UserId == u.Id))
                {
                    TreeViewItem itemlast;
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

                    if (folder.HasUnread)
                    {
                        itemlast = new TreeViewItem
                        {
                            Header = new TextBlock { Text = name }
                        };
                    }
                    else
                    {
                        itemlast = new TreeViewItem
                        {
                            Header = new TextBlock { FontWeight = FontWeights.Bold, Text = name },
                            /*ContextMenu = new ContextMenu { Items = { new MenuItem { Header = "yes" } } }*/
                        };
                    }

                    itemlast.PreviewMouseDown += (_, _) => UsersList_OnSelectionChanged(folder, u);
                    item.Items.Add(itemlast);
                }
            }

            MessageBox.Items.Clear();
        }

        private void CreateMessage(object sender, MouseButtonEventArgs e)
        {
            new CreateMessage().ShowDialog();
        }

        private void CheckUsers(object? sender, object? e)
        {
            CheckUsersAsync();
        }

        public async void CheckUsersAsync()
        {
            ProgressBar.Visibility = Visibility.Visible;
            await foreach (var i in UpdateMessages.UpdateAllAsync())
            {
                ProgressBar.Maximum += i.All ?? 0;
                ProgressBar.Value += i.Current ?? 0;
                ProgressBar.SetValue(ProgressBarExtension.ContentProperty,
                    string.Format(lang.lang.record_mess, ProgressBar.Value, ProgressBar.Maximum)
                );
            }

            ProgressBar.Visibility = Visibility.Hidden;
            RefreshFolders(null, null);
        }

        private void UsersList_OnSelectionChanged(Folder folder, User user)
        {
            MessageBox.Items.Clear();
            var listMail = SqlContextWrapper<List<Sqllite.Mail>>.exec(context =>
                context.Mails.Where(m => m.ParentFolderId == folder.Name).ToList());
            foreach (var mail in listMail)
            {
                if (mail != null)
                {
                    ListBoxItem listBoxItem = new ListBoxItem();
                    if (mail.Unread)
                    {
                        listBoxItem.Content = new TextBlock
                        {
                            FontWeight = FontWeights.Bold,
                            Text = string.Format(lang.lang.message_namelist, mail.Theme ?? "")
                        };
                    }
                    else
                    {
                        listBoxItem.Content = new TextBlock
                            { Text = string.Format(lang.lang.message_namelist, mail.Theme ?? "") };
                    }

                    listBoxItem.PreviewMouseDown += (o, _) => SelectMail(mail, (ListBoxItem)o);
                    listBoxItem.ContextMenu = new ContextMenu();
                    MenuItem menuitem = new MenuItem();
                    menuitem = new MenuItem();
                    menuitem.Header = lang.lang.mail_menu_delete;
                    menuitem.PreviewMouseDown += (o, _) => DeleteMessage(mail, user, (ListBoxItem)listBoxItem);
                    listBoxItem.ContextMenu.Items.Add(menuitem);
                    menuitem = new MenuItem();
                    menuitem.Header = lang.lang.mail_menu_resent;
                    menuitem.PreviewMouseDown += (_, _) => Resent(mail, user);
                    listBoxItem.ContextMenu.Items.Add(menuitem);
                    menuitem = new MenuItem();
                    menuitem.Header = lang.lang.mail_menu_reply;
                    menuitem.PreviewMouseDown += (_, _) => Reply(mail, user);
                    listBoxItem.ContextMenu.Items.Add(menuitem);
                    menuitem = new MenuItem();
                    menuitem.Header = lang.lang.mail_menu_moveto;
                    menuitem.PreviewMouseDown += (_, _) => MoveTo(mail, user);
                    listBoxItem.ContextMenu.Items.Add(menuitem);

                    MessageBox.Items.Add(listBoxItem);
                }
                else
                {
                    Feedback.ShowDialog("Message not found. ");
                }
            }
        }

        private void SelectMail(Sqllite.Mail mail, ListBoxItem listBoxItem, bool MarkUnread = true)
        {
            if (lang.lang.langName != "en")
            {
                View.NavigateToString("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">" + mail.Html);
            }
            else
            {
                View.NavigateToString(mail.Html);
            }

            to.Text = mail.From;
            if (MarkUnread)

            {
                MarkAsReadAsync(mail, listBoxItem);
            }
        }

        private async Task MarkAsReadAsync(Sqllite.Mail mail, ListBoxItem listBoxItem)
        {
            ((TextBlock)listBoxItem.Content).FontWeight = FontWeights.Normal;
            await SqlContextWrapper.execAsync(func: async context =>
            {
                (await context.Mails.Where(m => m.Id == mail.Id).FirstAsync()).Unread = false;
                await context.SaveChangesAsync();
            });
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CheckMail.CheckAllAsync();
            RefreshFolders(null, null);
        }

        private void DeleteMessage(Sqllite.Mail mail, User user, ListBoxItem listBoxItem)
        {
            SqlContextWrapper.exec(func: context => context.Mails.Remove(mail));
            ImapConnection connection = new ImapConnection(user);
            connection.Open();
            var f = connection.GetFolder(mail.ParentFolderId);
            f.Open(FolderAccess.ReadWrite);
            var uids = f.Search(SearchQuery.HeaderContains("Message-Id", mail.IdMail));
            f.AddFlags(uids, MessageFlags.Deleted, silent: true);
            MessageBox.Items.Remove(listBoxItem);
            connection.Close();
        }

        private void Resent(Sqllite.Mail mail, User user)
        {
            new CreateMessage(null, "Fwd: " + mail.Theme,
                "<div>\u00a0</div><div>02.12.2023, 22:03, \"" + mail.To + "\" &lt;" + mail.To +
                "&gt;:</div><div>\u00a0</div><div>\u00a0</div><div>-------- Пересылаемое сообщение --------</div><div>\\u00a0</div><div>02.12.2023, 22:03, \"" +
                mail.To + "\" &lt;" + mail.To + "&gt;:</div> " + mail.Html +
                " <div>-------- Конец пересылаемого сообщения --------</div>").Show();
        }

        private void Reply(Sqllite.Mail mail, User user)
        {
            new CreateMessage(mail.From, "Re: " + mail.Theme,
                "<div>\u00a0</div><div>02.12.2023, 22:03, \"" + mail.To + "\" &lt;" + mail.To +
                "&gt;:</div><blockquote>" + mail.Html + "</blockquote>").Show();
        }

        private void MoveTo(Sqllite.Mail mail, User user)
        {
            new MoveToFolder(mail, user).ShowDialog();
        }

        private void OpenOptions(object sender, MouseButtonEventArgs e)
        {
            new Settings().ShowDialog();
        }

        async Task Update()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(Constants.UpdateString);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var update = JsonSerializer.Deserialize<Update>(responseBody);
            if (update != null)
            {
                if (int.Parse(update.latestVersionCode) > Constants.VersionCode)
                {
                    var notificationManager = new NotificationManager();
                    var notificationContent = new NotificationContent
                    {
                        Title = lang.lang.version_available,
                        Message = string.Format(lang.lang.version_availablem, update.latestVersion),
                        Type = NotificationType.Warning
                    };
                    await notificationManager.ShowAsync(notificationContent,
                        onClick: () =>
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = update.url,
                                UseShellExecute = true
                            }));
                    player.Play();
                }
            }
            else
            {
                var notificationManager = new NotificationManager();
                var notificationContent = new NotificationContent
                {
                    Title = lang.lang.version_error,
                    Type = NotificationType.Error
                };
                await notificationManager.ShowAsync(notificationContent);
            }
        }

        private void About(object sender, MouseButtonEventArgs e)
        {
            AdonisUI.Controls.MessageBox.Show(
                $"Mail v.{Constants.Version}, v.code {Constants.VersionCode}",
                "Mail", icon: MessageBoxImage.Information);
        }

        private void ClearUsers(object sender, MouseButtonEventArgs e)
        {
            SqlContextWrapper.exec(func: context => context.Database.ExecuteSqlRaw("DELETE FROM Users"));
            CheckUsers(null, null);
        }

        private void ClearMails(object sender, MouseButtonEventArgs e)
        {
            SqlContextWrapper.exec(func: context => context.Database.ExecuteSqlRaw("DELETE FROM Mails"));
            MessageBox.Items.Clear();
        }

        private void ClearFolders(object sender, MouseButtonEventArgs e)
        {
            SqlContextWrapper.exec(func: context => context.Database.ExecuteSqlRaw("DELETE FROM Folders"));
            CheckUsers(null, null);
        }

        private void UpdatesCheck(object sender, MouseButtonEventArgs e)
        {
            Update();
        }

        private void SendFeedback(object sender, MouseButtonEventArgs e)
        {
            new SendFeedback().ShowDialog();
        }

        private void WebBrowser_loaded(object sender, RoutedEventArgs e)
        {
            SelectMail(_defaultMail, new ListBoxItem(), true);
        }

        private void ListView_loaded(object sender, RoutedEventArgs e)
        {
            var mail = _defaultMail;
            var user = _defaultUser;
            ListBoxItem listBoxItem = new ListBoxItem();
            if (mail.Unread)
            {
                listBoxItem.Content = new TextBlock
                {
                    FontWeight = FontWeights.Bold,
                    Text = string.Format(lang.lang.message_namelist, mail.Theme ?? "")
                };
            }
            else
            {
                listBoxItem.Content = new TextBlock
                    { Text = string.Format(lang.lang.message_namelist, mail.Theme ?? "") };
            }

            listBoxItem.PreviewMouseDown += (o, _) => SelectMail(mail, (ListBoxItem)o);
            listBoxItem.ContextMenu = new ContextMenu();
            MenuItem menuitem = new MenuItem();
            menuitem = new MenuItem();
            menuitem.Header = lang.lang.mail_menu_delete;
            menuitem.PreviewMouseDown += (o, _) => DeleteMessage(mail, user, (ListBoxItem)listBoxItem);
            listBoxItem.ContextMenu.Items.Add(menuitem);
            menuitem = new MenuItem();
            menuitem.Header = lang.lang.mail_menu_resent;
            menuitem.PreviewMouseDown += (_, _) => Resent(mail, user);
            listBoxItem.ContextMenu.Items.Add(menuitem);
            menuitem = new MenuItem();
            menuitem.Header = lang.lang.mail_menu_reply;
            menuitem.PreviewMouseDown += (_, _) => Reply(mail, user);
            listBoxItem.ContextMenu.Items.Add(menuitem);
            menuitem = new MenuItem();
            menuitem.Header = lang.lang.mail_menu_moveto;
            menuitem.PreviewMouseDown += (_, _) => MoveTo(mail, user);
            listBoxItem.ContextMenu.Items.Add(menuitem);

            MessageBox.Items.Add(listBoxItem);
            MessageBox.SelectedIndex = 0;
        }
    }
}