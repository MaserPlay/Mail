using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdonisUI.Controls;
using Mail.Connection;
using Mail.FormModel;
using Microsoft.Win32;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;

namespace Mail.Xamls;

public partial class Settings : AdonisWindow
{
    public Settings()
    {
        InitializeComponent();
    }

    private void OnlyNumbers(object sender, TextCompositionEventArgs e)
    {
        var regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private void PickSound(object sender, MouseButtonEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = $"{lang.lang.files_wav} (*.wav)|*.wav"
        };
        openFileDialog.ShowDialog();
        emailFile.Content = openFileDialog.FileName;
    }

    private void Save(object sender, MouseButtonEventArgs e)
    {
        ok.IsEnabled = false;
        if (Validation.GetHasError(ConnectionTime) || Validation.GetHasError(MailCheckTime))
        {
            ok.IsEnabled = true;
            MessageBox.Show(lang.lang.adduser_notfilled, null, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Settings1.Default.ConnectionTime = int.Parse(((SettingsViewModel)DataContext).ConnectionTime);
        Settings1.Default.MailCheckTime = int.Parse(((SettingsViewModel)DataContext).MailCheckTime);
        Settings1.Default.Save();
        Close();
    }
}