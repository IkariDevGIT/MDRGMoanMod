namespace MoanMod.MoanModPreferences;

public interface IMoanModPreferences
{
    /// <summary>
    /// Triggers when a preference is updated, the parameter is the preference name
    /// </summary>
    event Action<string> OnPreferencesUpdated;


    /// <summary>
    /// Whether we've shown the MoanMod notice
    /// </summary>
    bool NoticePopupShown { get; set; }
    /// <summary>
    /// Whether the user has accepted to check for updates
    /// </summary>
    bool UpdateCheckingEnabled { get; set; }
    /// <summary>
    /// Whether we've shown the update check popup
    /// </summary>
    bool UpdateCheckingPopupShown { get; set; }
    /// <summary>
    /// The last update check time (Windows filetime)
    /// </summary>
    long LastUpdateCheckTime { get; set; }
    
    /// <summary>
    /// Initializes the storage bucket
    /// </summary>
    void Initialize();

    /// <summary>
    /// Flushes the data
    /// </summary>
    void Save();
}
