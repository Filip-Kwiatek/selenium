using System.Diagnostics;

namespace BlazorApp.E2E;

public sealed class BlazorAppFixture : IAsyncLifetime
{
    private const string DefaultBaseUrl = "http://127.0.0.1:5205";
    private Process? appProcess;

    public string BaseUrl { get; } =
        Environment.GetEnvironmentVariable("BASE_URL")?.TrimEnd('/') ?? DefaultBaseUrl;

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("BASE_URL") is not null)
        {
            await WaitForAppAsync();
            return;
        }

        var repositoryRoot = FindRepositoryRoot();
        appProcess = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "run --project BlazorApp.csproj --no-launch-profile --urls http://127.0.0.1:5205",
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            Environment =
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            }
        });

        if (appProcess is null)
        {
            throw new InvalidOperationException("Nie udało się uruchomić aplikacji Blazor.");
        }

        await WaitForAppAsync();
    }

    public async Task DisposeAsync()
    {
        if (appProcess is null || appProcess.HasExited)
        {
            return;
        }

        appProcess.Kill(entireProcessTree: true);
        await appProcess.WaitForExitAsync();
        appProcess.Dispose();
    }

    private async Task WaitForAppAsync()
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await client.GetAsync($"{BaseUrl}/Ask");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(250);
        }

        throw new InvalidOperationException($"Aplikacja nie odpowiedziała pod adresem {BaseUrl}.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BlazorApp.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Nie znaleziono katalogu projektu BlazorApp.");
    }
}