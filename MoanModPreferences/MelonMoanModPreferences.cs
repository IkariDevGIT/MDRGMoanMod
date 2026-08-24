using System.Runtime.CompilerServices;
using MelonLoader;

namespace MoanMod.MoanModPreferences;

public class MelonMoanModPreferences : IMoanModPreferences
{
    private MelonMoanModPreferences() { }

    private static readonly Lazy<MelonMoanModPreferences> _lazy =
        new Lazy<MelonMoanModPreferences>(() => new MelonMoanModPreferences());

    private static bool _initialized = false;
    public static IMoanModPreferences Instance => _lazy.Value;



    private MelonPreferences_Category _category;

    private void _setPref<T> (MelonPreferences_Entry<T> entry, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(entry.Value, value)) return;

        entry.Value = value;
        Save();
        OnPreferencesUpdated?.Invoke(propertyName);
    }

    private MelonPreferences_Entry<bool> _noticePopupShown;
    public bool NoticePopupShown { 
        get => _noticePopupShown.Value;
        set => _setPref(_noticePopupShown, value);
    }
    
    private MelonPreferences_Entry<bool> _updateCheckingEnabled;
    public bool UpdateCheckingEnabled { 
        get => _updateCheckingEnabled.Value; 
        set => _setPref(_updateCheckingEnabled, value);
    }
    
    private MelonPreferences_Entry<bool> _askedAboutUpdateChecking;
    public bool UpdateCheckingPopupShown { 
        get => _askedAboutUpdateChecking.Value; 
        set => _setPref(_askedAboutUpdateChecking, value);
    }
    
    private MelonPreferences_Entry<long> _lastUpdateCheckTime;
    public long LastUpdateCheckTime { 
        get => _lastUpdateCheckTime.Value; 
        set => _setPref(_lastUpdateCheckTime, value);
    }

    public event Action<string> OnPreferencesUpdated;

    public void Initialize()
    {
        if (_initialized) return;

        _category = MelonPreferences.CreateCategory("MoanMod");
        _noticePopupShown = _category.CreateEntry("NoticePopupShown", false, "Notice popup has been shown");
        _updateCheckingEnabled = _category.CreateEntry("UpdateCheckingEnabled", true, "Enable automatic update checking");
        _askedAboutUpdateChecking = _category.CreateEntry("AskedAboutUpdateChecking", false, "User has been asked about update checking preference");
        _lastUpdateCheckTime = _category.CreateEntry("LastUpdateCheckTime", 0L, "Timestamp of last update check (ticks)");

        Save(); // First flush to disk

        _initialized = true;
    }

    public void Save() => _category.SaveToFile(printmsg: false);
}
