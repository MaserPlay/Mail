using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mail.Xamls;

public partial class SendFeedback : Window
{
    public SendFeedback()
    {
        InitializeComponent();
    }

    private void Ok(object sender, MouseButtonEventArgs e)
    {
        ok.Focus();
        ok.IsEnabled = false;
        if (Validation.GetHasError(TextBox))
        {
            MessageBox.Show(lang.lang.adduser_notfilled, null, MessageBoxButton.OK, MessageBoxImage.Warning);
            ok.IsEnabled = true;
            return;
        }
        Feedback.Send(TextBox.Text);
        Close();
    }
}