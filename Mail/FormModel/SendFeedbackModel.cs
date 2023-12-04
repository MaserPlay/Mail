using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Mail.FormModel;

public class SendFeedbackModel : IDataErrorInfo
{
    public string e { get; set; }
    public string Error { get; }

    public string this[string columnName]
    {
        get
        {
            string error = String.Empty;
            switch (columnName)
            {
                case "e":
                    if (string.IsNullOrEmpty(e))
                    {
                        error = "Must be not null";
                    }

                    break;
            }

            return error;
        }
    }
}