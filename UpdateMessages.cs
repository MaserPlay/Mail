using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mail.Connection;
using Mail.Sqllite;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;

namespace Mail;

public static class UpdateMessages
{
    public static async IAsyncEnumerable<Progress> UpdateAllAsync()
    {
        yield return new Progress(0, 0);
        var usersList =
            await SqlContextWrapper<List<User>>.execAsync(func: async context => await context.Users.ToListAsync());
        foreach (var user in usersList)
        {
            await foreach (var number in UpdateFromAsync(user))
            {
                yield return number;
            }
        }
    }

    public async static IAsyncEnumerable<Progress> UpdateFromAsync(User u)
    {
        yield return new Progress(0, 0);
        ImapConnection connection = new ImapConnection(u);
        await connection.OpenAsync();

        var Ifolders = await connection.GetFoldersAsync(connection.PersonalNamespaces.First());
        var folders = Ifolders.Reverse().ToList();
        string? ParentFolderId = null;
        foreach (var folder in folders)
        {
            yield return new Progress(folder.Count, null);
            await SqlContextWrapper.execAsync(func: async context =>
            {
                ParentFolderId = folder.ParentFolder.Name;
                var dbf = await context.Folders.Where(f => f.Name == folder.Name).FirstOrDefaultAsync();
                if (dbf == null)
                {
                    await context.Folders.AddAsync(new Folder()
                    {
                        Name = folder.Name,
                        ParentId = ParentFolderId,
                        UserId = u.Id,
                        HasUnread = folder.Unread > 1,
                        Attribute = folder.Attributes
                    });
                }
                else
                {
                    dbf.Name = folder.Name;
                    dbf.ParentId = ParentFolderId;
                    dbf.UserId = u.Id;
                    dbf.HasUnread = folder.Unread > 1;
                }

                await context.SaveChangesAsync();
            });
            await folder.OpenAsync(FolderAccess.ReadOnly);
            List<string> unreadmes = new List<string>();
            foreach (var uid in await folder.SearchAsync(SearchQuery.NotSeen))
            {
                unreadmes.Add((await folder.GetMessageAsync(uid)).MessageId);
            }

            for (var i = 0; i < folder.Count; i++)
            {
                yield return new Progress(null, 1);
                var message = await folder.GetMessageAsync(i);
                await SqlContextWrapper.execAsync(func: async context =>
                {
                    ParentFolderId = folder.Name;
                    var dbm = await context.Mails.Where(m => m.IdMail == message.MessageId).FirstOrDefaultAsync();
                    if (dbm == null)
                    {
                        await context.Mails.AddAsync(new Sqllite.Mail()
                        {
                            Html = message.HtmlBody,
                            From = message.From.Mailboxes.ToList()[0].Address,
                            Theme = message.Subject,
                            To = message.To.ToString(),
                            ParentFolderId = ParentFolderId,
                            IdMail = message.MessageId,
                            Unread = unreadmes.Find(s => s == message.MessageId) != null
                        });
                    }
                    else
                    {
                        dbm.Html = message.HtmlBody;
                        dbm.From = message.From.Mailboxes.ToList()[0].Address;
                        dbm.Theme = message.Subject;
                        dbm.To = message.To.ToString();
                        dbm.ParentFolderId = ParentFolderId;
                        dbm.IdMail = message.MessageId;
                    }

                    await context.SaveChangesAsync();
                });
            }
        }

        await connection.CloseAsync();
    }
}