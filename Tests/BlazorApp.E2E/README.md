# Testy E2E

Testy używają xUnit i Selenium WebDriver. Selenium Manager pobiera właściwy driver automatycznie, więc nie trzeba dodawać `chromedriver.exe` do repozytorium.

## Uruchomienie

Z katalogu głównego aplikacji:

```powershell
dotnet test Tests/BlazorApp.E2E/BlazorApp.E2E.csproj
```

Fixture uruchamia aplikację pod `http://127.0.0.1:5205` i zamyka ją po teście. Aby użyć już działającej aplikacji:

```powershell
$env:BASE_URL = "http://127.0.0.1:5205"
dotnet test Tests/BlazorApp.E2E/BlazorApp.E2E.csproj
```

Domyślnie Chrome działa headless. Dla lokalnego debugowania można otworzyć okno przeglądarki:

```powershell
$env:SELENIUM_BROWSER = "chrome-visible"
dotnet test Tests/BlazorApp.E2E/BlazorApp.E2E.csproj
```

Do uruchomienia potrzebna jest zainstalowana przeglądarka Google Chrome.