using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Mail.Sqllite;
using Mail.Xamls;
using MailKit.Security;

namespace Mail.FormModel;

public class AddUserModel : IDataErrorInfo
{
    private readonly List<User> _listU;

    public AddUserModel()
    {
        _listU = SqlContextWrapper<List<User>>.exec(func: context => context.Users.ToList());
    }

    public string email { get; set; }
    public string urlmailSmtp { get; set; }
    public string urlmailImap { get; set; }
    public bool auth { get; set; }
    public int ssl { get; set; } = 0;
    public string this[string columnName]
    {
        get
        {
            string error=String.Empty;
            switch (columnName)
            {
                case "email" :
                    if (string.IsNullOrEmpty(email))
                    {
                        error = "To is null";
                    }
                    else if (!Regex.IsMatch(email, "^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$"))
                    {
                        error = "Email TextBox dont have Email";
                    }
                    else if (_listU.FirstOrDefault(u => u.MailAdr == email) != null)
                    {
                        error = "Email not unique";
                    }
                    break;
                case "urlmailImap" :
                    if (string.IsNullOrEmpty(urlmailImap))
                    {
                        error = "Is Null";
                    }
                    break;
                case "urlmailSmtp" :
                    if (string.IsNullOrEmpty(urlmailSmtp))
                    {
                        error = "Is Null";
                    }
                    break;
            }
            return error;
        }
    }
    public string Error
    {
        get;
    }
}