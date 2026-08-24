using System.Collections;
using System.Text.Json;
using MelonLoader;
using MoanMod.MoanModPreferences;
using MoanMod.PopupService;
using UnityEngine;
using UnityEngine.Networking;

namespace MoanMod;


public class GitHubRelease
{
    public string TagName { get; set; }
    public string HtmlUrl { get; set; }
    public SemanticVersion Version { get; set; }
}

public class UpdateChecker
{
    private bool _isCheckingForUpdates = false;
    private const float UpdateCheckCooldown = 1800f; // 30 minutes

    private readonly IPopupService _popupService = new OverlayPopupService();
    private readonly IMoanModPreferences _modPreferences = MelonMoanModPreferences.Instance;

    public IEnumerator CheckForUpdatesCoroutine(SemanticVersion version)
    {
        var currentTicks = DateTime.UtcNow.Ticks;
        var ticksSinceLast = currentTicks - _modPreferences.LastUpdateCheckTime;
        var secondsSinceLastCheck = ticksSinceLast / 10000000.0;

        if (_isCheckingForUpdates || secondsSinceLastCheck <= UpdateCheckCooldown) yield break;


        _modPreferences.LastUpdateCheckTime = currentTicks;
        var githubRequest = UnityWebRequest.Get("https://api.github.com/repos/IkariDevGIT/MDRGMoanMod/releases?per_page=10");
        githubRequest.downloadHandler = new DownloadHandlerBuffer();
    
        yield return githubRequest.SendWebRequest();

        if (githubRequest.result != UnityWebRequest.Result.Success)
        {
            MelonLogger.Error($"Failed to fetch releases: {githubRequest.error}");
            _isCheckingForUpdates = false;
            yield break;
        }
        
        string json = githubRequest.downloadHandler.text;
        var releases = ParseGitHubReleasesJson(json);

        MaybeSendPopup(version, releases);
    }

    public void MaybeSendPopup(SemanticVersion currentVersion, IList<GitHubRelease> releases)
    {
        var suggestedRelease = releases
            .Where(r => r.Version > currentVersion)
            .Where(r => currentVersion.IsPrerelease || !r.Version.IsPrerelease) // If stable, ignore pre-releases
            .OrderByDescending(r => r.Version)
            .FirstOrDefault();

        if (suggestedRelease == null || suggestedRelease.Version <= currentVersion) return;

        var choices = new[] {
            new PopupChoice("Skip", () => { }),
            new PopupChoice("Open in Browser", () => OpenUrlInBrowser(suggestedRelease.HtmlUrl)),
            new PopupChoice("Disable Notifications", () => _modPreferences.UpdateCheckingEnabled = false)
        };

        var updateMessage = $"Your version \"{currentVersion}\" is outdated, get {suggestedRelease.Version} on github.";
        _popupService.ChoicePopup("MoanMod - Update Available", updateMessage, choices);
    }

    private IList<GitHubRelease> ParseGitHubReleasesJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<GitHubRelease>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return new List<GitHubRelease>();

            return doc.RootElement.EnumerateArray()
                .Select(CreateReleaseFromElement)
                .Where(release => release != null)
                .ToList();
        }
        catch (JsonException ex)
        {
            MelonLogger.Error($"Failed to parse GitHub JSON: {ex.Message}");
            return new List<GitHubRelease>();
        }
    }

    private GitHubRelease CreateReleaseFromElement(JsonElement element)
    {
        if (!element.TryGetProperty("tag_name", out var tag) ||
            !element.TryGetProperty("html_url", out var url))
            return null;

        string tagName = tag.GetString();

        if (!SemanticVersion.TryParse(tagName, out var version)) return null;

        return new GitHubRelease
        {
            TagName = tagName,
            HtmlUrl = url.GetString(),
            Version = version
        };
    }

    private void OpenUrlInBrowser(string url)
    {
        if (string.IsNullOrEmpty(url)) return;

        try { Application.OpenURL(url); }
        catch (Exception ex) { MelonLogger.Error($"Failed to open browser: {ex.Message}"); }
    }
}