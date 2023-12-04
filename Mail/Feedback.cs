using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using AdonisUI.Controls;
using Mail.Xamls;

namespace Mail;

public static class Feedback
{
    public static void ShowDialog(string msg)
    {
        if (MessageBox.Show(lang.lang.error_descr,
                lang.lang.error_tittle, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
        {
            Send(msg);
        }
    }
    public static void ShowDialogVar()
    {
        new SendFeedback().ShowDialog();
    }

    public static void Error(string message)
    {
        if (MessageBox.Show(
                lang.lang.error_descr,
                lang.lang.error_tittle, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://maserplay.ru/api/crush");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(JsonSerializer.Serialize(new FeedbackRecord(message)));
        }

        httpWebRequest.GetResponse();
    }
    public static void Send(string message)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://maserplay.ru/api/crush");
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(JsonSerializer.Serialize(new FeedbackRecord(message)));
        }

        httpWebRequest.GetResponse();
    }
    
}

public record FeedbackRecord(string Message)
{
    public string AppName { get; } = "Mail";
    public string AppVersion { get; } = Constants.Version;
};