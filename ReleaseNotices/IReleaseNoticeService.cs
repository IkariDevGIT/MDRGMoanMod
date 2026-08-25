namespace MoanMod.ReleaseNotices;

/// <summary>Shows the first-launch notice or the "what's new" changelog, whichever applies.</summary>
public interface IReleaseNoticeService
{
    System.Collections.IEnumerator ShowIfNeeded();
}
