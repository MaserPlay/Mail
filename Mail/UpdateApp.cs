using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Mail.Xamls;
using Notifications.Wpf.Core;

namespace Mail;

public class UpdateApp
{
    private SoundPlayer player = new(Settings1.Default.PickMail);

    public async Task UpdateASync()
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(Constants.UpdateString);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        var update = JsonSerializer.Deserialize<Update>(responseBody);
        if (update != null)
        {
            if (int.Parse(update.LatestVersionCode) > Constants.VersionCode)
            {
                var notificationManager = new NotificationManager();
                var notificationContent = new NotificationContent
                {
                    Title = lang.lang.version_available,
                    Message = string.Format(lang.lang.version_availablem, update.LatestVersion),
                    Type = NotificationType.Warning
                };
                await notificationManager.ShowAsync(notificationContent,
                    onClick: async () => await DownloadAndOpenExeAsync(update.url));
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

    private async Task DownloadAndOpenExeAsync(string updateurl)
    {
        System.IO.Directory.CreateDirectory("update");
        var httpClient = new HttpClient();

        await using (var stream = await httpClient.GetStreamAsync(updateurl))
        {
            await using (var fileStream = new FileStream("update/" + updateurl.Split("/").Last(), FileMode.CreateNew))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
        Process.Start("update/" + updateurl.Split("/").Last());
        System.Windows.Application.Current.Shutdown();
    }
}