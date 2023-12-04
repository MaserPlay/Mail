using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Mail.FormModel;

public class CreateModel : IDataErrorInfo
{
    public string to { get; set; }

    public string this[string columnName]
    {
        get
        {
            string error = String.Empty;
            switch (columnName)
            {
                case "to":
                    if (string.IsNullOrEmpty(to))
                    {
                        error = "To is null";
                    }
                    else if (!Regex.IsMatch(to, "^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$"))
                    {
                        error = "To TextBox dont have Email";
                    }

                    break;
            }

            return error;
        }
    }

    public string Error { get; }
}