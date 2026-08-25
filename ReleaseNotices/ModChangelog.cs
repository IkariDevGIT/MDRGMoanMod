namespace MoanMod.ReleaseNotices;

public readonly struct ChangelogEntry
{
    public readonly SemanticVersion Version;
    public readonly string[] Highlights;

    public ChangelogEntry(string version, params string[] highlights)
    {
        Version = new SemanticVersion(version);
        Highlights = highlights;
    }
}

public static class ModChangelog
{
    // Newest last. Add one entry per future release that has user-facing changes worth mentioning.
    public static readonly ChangelogEntry[] Entries =
    {
        new ChangelogEntry("2.0.0",
            "- Added an in-game settings menu (Mod Settings Menu) to tune sensitivity, clustering, breathing, and more without editing files.",
            "- Fixed moaning sometimes starting before Advanced AI was actually unlocked.",
            "",
            "You can find the new settings menu here:",
            "Options > Mod Setting Menu > MoanMod"
        ),
    };
}
