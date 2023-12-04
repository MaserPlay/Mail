using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Mail.FormModel;

public class SettingsViewModel : IDataErrorInfo
{
    public string MailCheckTime { get; set; } = "10000";
    public string ConnectionTime { get; set; } = "5000";

    public string this[string columnName]
    {
        get
        {
            var error = string.Empty;
            switch (columnName)
            {
                case "MailCheckTime":
                    if (string.IsNullOrEmpty(MailCheckTime))
                    {
                        error = "Is Null";
                    }
                    else if (int.Parse(ConnectionTime) >= int.Parse(MailCheckTime))
                    {
                        error = "Must be < MailCheckTime";
                    }

                    break;
                case "ConnectionTime":
                    if (string.IsNullOrEmpty(ConnectionTime))
                    {
                        error = "Is Null";
                    }
                    else if (int.Parse(ConnectionTime) >= int.Parse(MailCheckTime))
                    {
                        error = "Must be > MailCheckTime";
                    }

                    break;
            }

            return error;
        }
    }

    public string Error { get; }
}