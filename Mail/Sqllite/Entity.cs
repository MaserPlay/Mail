using MailKit.Security;
using System.ComponentModel.DataAnnotations;
using MailKit;
using Microsoft.EntityFrameworkCore;

namespace Mail.Sqllite
{
    public class User
    {
        public int Id { get; set; }
        public string MailAdr { get; set; }
        public string HostSmtp { get; set; }
        public int PortSmtp { get; set; }
        public bool IsUseAuthSmtp { get; set; }
        public string? UsernameSmtp { get; set; }
        public string? PasswordSmtp { get; set; }
        public SecureSocketOptions IsSslSmtp { get; set; }
        public string HostImap { get; set; }
        public int PortImap { get; set; }
        public bool IsUseAuthImap { get; set; }
        public string? UsernameImap { get; set; }
        public string? PasswordImap { get; set; }
        public SecureSocketOptions IsSslImap { get; set; }
        public bool HasUnread { get; set; }
    }

    public class Mail
    {
        public int Id { get; set; }
        public string IdMail { get; set; }
        public string ParentFolderId { get; set; }
        public bool Unread { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Theme { get; set; }
        public string? Html { get; set; }
    }

    public class Folder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool HasUnread { get; set; }
        public string? ParentId { get; set; }
        public FolderAttributes? Attribute { get; set; }
        public string Name { get; set; }
    }
}