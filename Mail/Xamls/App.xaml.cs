using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Mail.Sqllite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notifications.Wpf.Core;

namespace Mail.Xamls
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            var options = new DbContextOptionsBuilder<SqlContext>()
                .UseSqlite("Data Source=db.db")
                .Options;
            using var dbContext = new SqlContext(options);
            dbContext.Database.Migrate();
            CheckMail.CheckAllWhiteInfiniteDelayAsync(new CancellationToken());
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        void App_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            if (MessageBox.Show(
                    lang.lang.error_descr,
                    lang.lang.error_tittle, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
            {
                Feedback.Send(e.Exception.ToString());
            }

            Shutdown();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
                foreach (var arg in e.Args)
                {
                    if (arg == "options")
                    {
                        new Settings().Show();
                        break;
                    }
                    else if (arg.Length > 7 && arg.Substring(0, 7) == "mailto:")
                    {
                        var u = new System.Uri(arg);
                        var queryDictionary = System.Web.HttpUtility.ParseQueryString(new System.Uri(arg).Query);
                        new CreateMessage(u.UserInfo + "@" + u.Host, queryDictionary["subject"],
                                queryDictionary["body"])
                            .Show();
                        return;
                    }
                }

                new MainWindow().Show();
        }
    }

    public record Update(string latestVersion, string latestVersionCode, string url, string[] releaseNotes);
}