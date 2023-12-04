using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Mail.Connection;
using Mail.Sqllite;
using Mail.Xamls;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Notifications.Wpf.Core;

namespace Mail;

public static class CheckMail
{
    private static SoundPlayer player = new(Settings1.Default.PickMail);

    public static async Task CheckAllWhiteInfiniteDelayAsync(CancellationToken cancellationToken)
    {
        var usersList =
            await SqlContextWrapper<List<User>>.execAsync(func: async context =>
                await context.Users.ToListAsync(cancellationToken));
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(Settings1.Default.MailCheckTime, cancellationToken);
            foreach (var user in usersList)
            {
                await CheckUserAsync(user);
            }
        }
    }

    public static async Task CheckAllAsync()
    {
        var usersList =
            await SqlContextWrapper<List<User>>.execAsync(func: async context =>
                await context.Users.ToListAsync());
        foreach (var user in usersList)
        {
            await CheckUserAsync(user);
        }
    }


    public static async Task CheckUserAsync(User u)
    {
        var notificationManager = new NotificationManager();
        ImapConnection client = new ImapConnection(u);
        await client.OpenAsync();
        var Ifolders = await client.GetFoldersAsync(client.PersonalNamespaces.First());
        var folders = Ifolders.Reverse().ToList();
        var UnreadMails = await SqlContextWrapper<List<Sqllite.Mail>>.execAsync(func: async context =>
            await context.Mails.Where(m => m.Unread).ToListAsync());
        foreach (var folder in folders)
        {
            await folder.OpenAsync(FolderAccess.ReadOnly);
            foreach (var id in (await folder.SearchAsync(SearchQuery.NotSeen)).ToList())
            {
                var unmessage = await folder.GetMessageAsync(id);
                if (UnreadMails.Find(m => m.IdMail == unmessage.MessageId) == null)
                {
                    var notificationContent = new NotificationContent
                    {
                        Title = "New message!",
                        Message = $"{unmessage.Subject}",
                        Type = NotificationType.Information
                    };
                    await SqlContextWrapper.execAsync(func: async context =>
                    {
                        await context.Mails.AddAsync(new Sqllite.Mail
                        {
                            Html = unmessage.HtmlBody,
                            From = unmessage.From.Mailboxes.ToList()[0].Address,
                            Theme = unmessage.Subject,
                            To = unmessage.To.ToString(),
                            ParentFolderId = folder.Name,
                            IdMail = unmessage.MessageId,
                            Unread = true
                        });
                        await context.SaveChangesAsync();
                    });
                    await notificationManager.ShowAsync(notificationContent);
                    player.Play();
                }
            }
        }

        await client.CloseAsync();
    }
}