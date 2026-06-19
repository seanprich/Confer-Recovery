using System.Windows;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Application.Chapters;
using ConferRecovery.Desktop.Application.Members;
using ConferRecovery.Desktop.Application.Rooms;
using ConferRecovery.Desktop.Contracts.Chapters;
using ConferRecovery.Desktop.Contracts.Members;
using ConferRecovery.Desktop.Contracts.Rooms;
using ConferRecovery.Desktop.Contracts.Session;

namespace ConferRecovery.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AuthenticationService _authenticationService;
    private readonly IAuthenticatedSessionStore _sessionStore;
    private readonly IChaptersApiClient _chaptersApi;
    private readonly IMembersApiClient _membersApi;
    private readonly IRoomsApiClient _roomsApi;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public MainWindow(
        AuthenticationService authenticationService,
        IAuthenticatedSessionStore sessionStore,
        IChaptersApiClient chaptersApi,
        IMembersApiClient membersApi,
        IRoomsApiClient roomsApi,
        HttpClient httpClient)
    {
        _authenticationService = authenticationService;
        _sessionStore = sessionStore;
        _chaptersApi = chaptersApi;
        _membersApi = membersApi;
        _roomsApi = roomsApi;
        _httpClient = httpClient;
        InitializeComponent();

        EmailTextBox.Text = "admin@example.com";
        ShowLoginForm();
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

        SetBusy(true);
        StatusText.Text = "Signing in...";

        try
        {
            var session = await _authenticationService.LoginAsync(new LoginAttempt(email, password), CancellationToken.None);
            if (session is null)
            {
                StatusText.Text = "Login failed: invalid credentials or inactive account.";
                return;
            }

            ShowSessionShell(session);
            SessionStatusText.Text = "Signed in successfully.";
        }
        catch (HttpRequestException)
        {
            StatusText.Text = "Login error: the API is unreachable. Confirm the server is running.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Login error: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void OnLogoutClicked(object sender, RoutedEventArgs e)
    {
        _sessionStore.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = null;
        ShowLoginForm();
        PasswordInput.Password = string.Empty;
        StatusText.Text = "Signed out.";
    }

    private void SetBusy(bool isBusy)
    {
        LoginButton.IsEnabled = !isBusy;
        EmailTextBox.IsEnabled = !isBusy;
        PasswordInput.IsEnabled = !isBusy;
    }

    private void ShowLoginForm()
    {
        LoginPanel.Visibility = Visibility.Visible;
        SessionPanel.Visibility = Visibility.Collapsed;
        EmailTextBox.Focus();
    }

    private void ShowSessionShell(AuthenticatedSession session)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", session.AccessToken);

        WelcomeText.Text = $"Welcome {session.DisplayName} ({session.Role})";
        SessionMetaText.Text = $"Member: {session.MemberId}  |  Chapter: {session.ChapterId}";
        TokenText.Text = $"Token expires at: {session.ExpiresAt:O}";

        MembersChapterIdTextBox.Text = session.ChapterId;
        RoomsChapterIdTextBox.Text = session.ChapterId;

        LoginPanel.Visibility = Visibility.Collapsed;
        SessionPanel.Visibility = Visibility.Visible;
    }

    private async Task RunDashboardActionAsync(string actionName, Func<CancellationToken, Task<object?>> action)
    {
        SessionStatusText.Text = $"Running {actionName}...";
        try
        {
            var result = await action(CancellationToken.None);
            AppendOutput(actionName, result);
            SessionStatusText.Text = $"{actionName} completed.";
        }
        catch (Exception ex)
        {
            AppendOutput(actionName, new { error = ex.Message });
            SessionStatusText.Text = $"{actionName} failed: {ex.Message}";
        }
    }

    private void AppendOutput(string actionName, object? payload)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var content = payload is null ? "null" : JsonSerializer.Serialize(payload, JsonOptions);
        DevOutputTextBox.AppendText($"[{timestamp}] {actionName}{Environment.NewLine}{content}{Environment.NewLine}{Environment.NewLine}");
        DevOutputTextBox.ScrollToEnd();
    }

    private void OnClearOutputClicked(object sender, RoutedEventArgs e)
    {
        DevOutputTextBox.Clear();
        SessionStatusText.Text = "Output cleared.";
    }

    private async void OnChaptersGetActiveClicked(object sender, RoutedEventArgs e)
    {
        await RunDashboardActionAsync("Chapters.GetActive", async ct => await _chaptersApi.GetActiveAsync(ct));
    }

    private async void OnChaptersGetByIdClicked(object sender, RoutedEventArgs e)
    {
        var chapterId = ChapterIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Chapters.GetById", async ct => await _chaptersApi.GetByIdAsync(chapterId, ct));
    }

    private async void OnChaptersCreateClicked(object sender, RoutedEventArgs e)
    {
        var request = BuildChapterRequest();
        await RunDashboardActionAsync("Chapters.Create", async ct => await _chaptersApi.CreateAsync(request, ct));
    }

    private async void OnChaptersUpdateSfuClicked(object sender, RoutedEventArgs e)
    {
        var chapterId = ChapterIdTextBox.Text.Trim();
        var request = BuildChapterRequest();
        await RunDashboardActionAsync("Chapters.UpdateSfu", async ct => await _chaptersApi.UpdateSfuAsync(chapterId, request, ct));
    }

    private async void OnChaptersSetStatusClicked(object sender, RoutedEventArgs e)
    {
        var chapterId = ChapterIdTextBox.Text.Trim();
        var status = ChapterStatusTextBox.Text.Trim();
        await RunDashboardActionAsync("Chapters.SetStatus", async ct => await _chaptersApi.SetStatusAsync(chapterId, status, ct));
    }

    private async void OnMembersGetByChapterClicked(object sender, RoutedEventArgs e)
    {
        var chapterId = MembersChapterIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Members.GetByChapter", async ct => await _membersApi.GetByChapterAsync(chapterId, ct));
    }

    private async void OnMembersGetByIdClicked(object sender, RoutedEventArgs e)
    {
        var memberId = MemberIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Members.GetById", async ct => await _membersApi.GetByIdAsync(memberId, ct));
    }

    private async void OnMembersCreateClicked(object sender, RoutedEventArgs e)
    {
        var request = new CreateMemberRequest(
            MemberDisplayNameTextBox.Text.Trim(),
            MemberEmailTextBox.Text.Trim(),
            MemberPasswordTextBox.Text,
            MembersChapterIdTextBox.Text.Trim(),
            MemberRoleTextBox.Text.Trim());

        await RunDashboardActionAsync("Members.Create", async ct => await _membersApi.CreateAsync(request, ct));
    }

    private async void OnMembersUpdateStatusClicked(object sender, RoutedEventArgs e)
    {
        var memberId = MemberIdTextBox.Text.Trim();
        var request = new UpdateMemberStatusRequest(MemberStatusTextBox.Text.Trim());
        await RunDashboardActionAsync("Members.UpdateStatus", async ct => await _membersApi.UpdateStatusAsync(memberId, request, ct));
    }

    private async void OnMembersUpdateRoleClicked(object sender, RoutedEventArgs e)
    {
        var memberId = MemberIdTextBox.Text.Trim();
        var request = new UpdateMemberRoleRequest(MemberRoleTextBox.Text.Trim());
        await RunDashboardActionAsync("Members.UpdateRole", async ct => await _membersApi.UpdateRoleAsync(memberId, request, ct));
    }

    private async void OnRoomsGetByChapterClicked(object sender, RoutedEventArgs e)
    {
        var chapterId = RoomsChapterIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Rooms.GetByChapter", async ct => await _roomsApi.GetByChapterAsync(chapterId, ct));
    }

    private async void OnRoomsGetByIdClicked(object sender, RoutedEventArgs e)
    {
        var roomId = RoomIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Rooms.GetById", async ct => await _roomsApi.GetByIdAsync(roomId, ct));
    }

    private async void OnRoomsCreateClicked(object sender, RoutedEventArgs e)
    {
        DateTime? scheduledAt = null;
        var scheduledText = RoomScheduledAtTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(scheduledText) && DateTime.TryParse(scheduledText, out var parsedScheduledAt))
        {
            scheduledAt = parsedScheduledAt;
        }

        var request = new CreateRoomRequest(
            RoomNameTextBox.Text.Trim(),
            RoomsChapterIdTextBox.Text.Trim(),
            scheduledAt);

        await RunDashboardActionAsync("Rooms.Create", async ct => await _roomsApi.CreateAsync(request, ct));
    }

    private async void OnRoomsStartClicked(object sender, RoutedEventArgs e)
    {
        var roomId = RoomIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Rooms.Start", async ct => await _roomsApi.StartAsync(roomId, ct));
    }

    private async void OnRoomsEndClicked(object sender, RoutedEventArgs e)
    {
        var roomId = RoomIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Rooms.End", async ct => await _roomsApi.EndAsync(roomId, ct));
    }

    private async void OnRoomsJoinClicked(object sender, RoutedEventArgs e)
    {
        var roomId = RoomIdTextBox.Text.Trim();
        await RunDashboardActionAsync("Rooms.Join", async ct => await _roomsApi.JoinAsync(roomId, ct));
    }

    private CreateChapterRequest BuildChapterRequest()
    {
        return new CreateChapterRequest(
            ChapterNameTextBox.Text.Trim(),
            ChapterSfuUrlTextBox.Text.Trim(),
            ChapterApiKeyTextBox.Text.Trim(),
            ChapterApiSecretTextBox.Text.Trim());
    }
}