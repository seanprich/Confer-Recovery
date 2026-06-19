using System.Windows;
using ConferRecovery.Desktop.Application.Auth;

namespace ConferRecovery.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AuthenticationService _authenticationService;

    public MainWindow(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        InitializeComponent();

        EmailTextBox.Text = "admin@chapter.org";
    }

    private async void OnLoginClicked(object sender, RoutedEventArgs e)
    {
        var email = EmailTextBox.Text.Trim();
        var password = PasswordInput.Password;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            StatusText.Text = "Email and password are required.";
            return;
        }

        LoginButton.IsEnabled = false;
        StatusText.Text = "Signing in...";

        try
        {
            var session = await _authenticationService.LoginAsync(new LoginAttempt(email, password), CancellationToken.None);
            if (session is null)
            {
                StatusText.Text = "Login failed: invalid credentials or inactive account.";
                return;
            }

            StatusText.Text =
                $"Welcome {session.DisplayName} ({session.Role}). Token expires at {session.ExpiresAt:O}.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Login error: {ex.Message}";
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }
}