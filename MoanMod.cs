using System.Reflection;
using MelonLoader;
using MelonLoader.Utils;
using MoanMod.Controllers;
using MoanMod.MoanModPreferences;
using MoanMod.PopupService;
using UnityEngine;

[assembly: MelonInfo(typeof(MoanMod.MoanMod), "Moan Mod", "1.4.2-pre", "IkariDev")]
[assembly: MelonGame("IncontinentCell", "My Dystopian Robot Girlfriend")]

namespace MoanMod;

public class MoanMod : MelonMod
{
    private Il2Cpp.ModelBrain _brain;
    private AudioPlayer _audioPlayer;
    private SemanticVersion _modVersion;

    private IMoanModPreferences _modPreferences;
    private UpdateChecker _updateChecker;
    private IPopupService _popupService;

    private IHeadpatController _headpat;
    private IMouthController _mouth;
    private IBreathController _breathMoan;
    private ISexMoanController _sexMoan;
    private ICummingController _cumMoan;

    // Transition flags for orchestration
    private bool _wasInSexScene;
    private bool _wasBeingHeadpatted;
    private bool _wasCumming;
    private float _sexSceneStartCooldown;

    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("Find MoanMod at https://github.com/IkariDevGIT/MDRGMoanMod.");
        MelonLogger.Msg($"Game Version: {Application.version}");

        InitializeSystems();
        ValidateGameVersion();
        LoadAudioAssets();
    }

    private void InitializeSystems()
    {
        _modVersion = GetModVersionFromAssembly();
        _modPreferences = MelonMoanModPreferences.Instance;
        _modPreferences.Initialize();

        _updateChecker = new UpdateChecker();
        _popupService = new OverlayPopupService();
        _audioPlayer = new AudioPlayer();

        // Initialize Sub-Controllers
        _headpat = new HeadpatController();
        _mouth = new MouthController();
        _breathMoan = new BreathController(_audioPlayer, _mouth);
        _cumMoan = new CummingController(_audioPlayer, _mouth, _breathMoan);
        _sexMoan = new SexMoanController(_audioPlayer, _mouth, _breathMoan, _headpat, _cumMoan);
    }

    private void ValidateGameVersion()
    {
        if (_modVersion.MajorMinorEquals(MoanModConfig.ExpectedGameVersion)) return;

        MelonLogger.Warning("================================================================================");
        MelonLogger.Warning("==================== !!! VERSION MISMATCH WARNING !!! ==========================");
        MelonLogger.Warning($"This mod was made for game version {MoanModConfig.ExpectedGameVersion}.x");
        MelonLogger.Warning($"You are running game version {Application.version}");
        MelonLogger.Warning("================================================================================");
    }

    private void LoadAudioAssets()
    {
        string modFolder = Path.Combine(MelonEnvironment.ModsDirectory, "MoanMod");
        try
        {
            _audioPlayer.LoadAllAudioFiles(modFolder);
            MelonLogger.Msg($"Loaded audio files - {_audioPlayer.GetLoadedFilesList()}");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to load audio files: {ex.Message}");
        }
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
        if (sceneName != "MainScene") return;
        MelonCoroutines.Start(PopupRoutine());
    }

    public System.Collections.IEnumerator PopupRoutine()
    {
        // Wait for menu to show, so we show when actually needed
        Func<bool> menuNotLoaded = () => UnityEngine.Object.FindObjectOfType<Il2Cpp.MenuStaticGui>() is null;
        yield return new WaitWhile(menuNotLoaded);

        var isShowingNoticePopup = true;

        if (!_modPreferences.NoticePopupShown)
        {
            ShowNoticePopup(() => isShowingNoticePopup = false);
            yield return new WaitWhile((Func<bool>)(() => isShowingNoticePopup));
        }

        var showingUpdatePreference = true;
        if (!_modPreferences.UpdateCheckingPopupShown)
        {
            ShowUpdatePreferenceDialog(() => showingUpdatePreference = false);
            yield return new WaitWhile((Func<bool>)(() => showingUpdatePreference));
        }

        if (!_modPreferences.UpdateCheckingEnabled) yield break;

        yield return _updateChecker.CheckForUpdatesCoroutine(_modVersion);
    }

    public override void OnUpdate()
    {
        _audioPlayer.UpdateSoundManager();

        if (_brain == null)
        {
            if (!Il2Cpp.ModelBrain.TryGet("bot", out _brain)) return;

            MelonLogger.Msg("Found ModelBrain!");
            _sexMoan.OnBrainFound();
        }

        var ctx = BrainContext.TryCapture(_brain);
        if (ctx == null) return;

        float deltaTime = Time.deltaTime;


        bool inSexScene = ctx.SceneType != SceneType.None;
        if (inSexScene && !_wasInSexScene) OnSexSceneEntered(ctx);
        else if (!inSexScene && _wasInSexScene) OnSexSceneExited(ctx);

        if (_sexSceneStartCooldown > 0f) _sexSceneStartCooldown -= Time.deltaTime;

        if (!inSexScene) return;

        _mouth.Tick(ctx);
        _mouth.ApplyToLive2D(_brain.ConnectedController?.TryCast<Il2Cpp.ILive2DController_Mouth>());
        _breathMoan.Tick(ctx);

        if (_sexSceneStartCooldown > 0f) return;


        _headpat.Tick(ctx);

        if (_headpat.IsActive && !_wasBeingHeadpatted)
        {
            MelonLogger.Msg("=== Started Headpatting (Applying Penalty) ===");
            _wasBeingHeadpatted = true;
        }
        else if (!_headpat.IsActive && _wasBeingHeadpatted)
        {
            MelonLogger.Msg("=== Stopped Headpatting (Removing Penalty) ===");
            _wasBeingHeadpatted = false;
        }

        if (ctx.IsCumming && !_wasCumming)
        {
            MelonLogger.Msg("=== Cumming Started ===");
            _sexMoan.Reset();
            _breathMoan.Reset();
            _cumMoan.OnStart(ctx);
        }
        else if (!ctx.IsCumming && _wasCumming)
        {
            _cumMoan.OnEnd(ctx);
        }
        _wasCumming = ctx.IsCumming;

        _sexMoan.Tick(ctx);
        _cumMoan.Tick(ctx);
    }

    private void OnSexSceneEntered(BrainContext ctx)
    {
        MelonLogger.Msg($"=== Entered Sex Scene ({ctx.SceneType}) ===");
        _sexSceneStartCooldown = MoanModConfig.SexSceneStartCooldown;
        _wasInSexScene = true;
    }

    private void OnSexSceneExited(BrainContext ctx)
    {
        MelonLogger.Msg("=== Exited Sex Scene ===");
        _wasInSexScene = false;

        var mouthLive2D = _brain?.ConnectedController?.TryCast<Il2Cpp.ILive2DController_Mouth>();
        if (mouthLive2D?.ParamMouthOpen != null) mouthLive2D.ParamMouthOpen.UnclampedValue = 0f;

        _mouth.Reset();
        _sexMoan.Reset();
        _breathMoan.Reset();
        _cumMoan.Reset();
        _headpat.Reset();
    }

    private void ShowNoticePopup(Action dismissCallback = null)
    {
        string title = "MoanMod - Notice";
        string message = "This is the first public release of MoanMod, please be aware that this didn't get thorough testing yet. Please report any bugs via github issues or to IkariDev on discord. You are also welcome to create PR's and make this mod better!\n\nHave fun!";

        _popupService.SimplePopup(title, message, dismissCallback);
        _modPreferences.NoticePopupShown = true;
    }

    private void ShowUpdatePreferenceDialog(Action onDismiss = null)
    {
        string title = "MoanMod - Update Notifications";
        string message = "Would you like to enable automatic update checking for MoanMod? This will notify you when new versions are available.\n\n(This will call the Github API every start of the game with a 30 minute cooldown.)";

        var choices = new[]
        {
            new PopupChoice("Enable", () => {
                _modPreferences.UpdateCheckingEnabled = true;
                onDismiss?.Invoke();
            }),
            new PopupChoice("Disable", () => {
                _modPreferences.UpdateCheckingEnabled = false;
                onDismiss?.Invoke();
            }),
        };

        _popupService.ChoicePopup(title, message, choices);
        _modPreferences.UpdateCheckingPopupShown = true;
    }

    private SemanticVersion GetModVersionFromAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var melonInfoAttr = assembly.GetCustomAttributes(typeof(MelonInfoAttribute), false).FirstOrDefault() as MelonInfoAttribute;

        if (melonInfoAttr?.Version == null)
        {
            MelonLogger.Error("Failed to read mod version from MelonInfo");
            return null;
        }

        return new SemanticVersion(melonInfoAttr!.Version);
    }
}