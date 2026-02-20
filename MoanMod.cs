using MelonLoader;
using UnityEngine;
using System.IO;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(MoanMod.MoanMod), "Moan Mod", "1.3", "IkariDev")]
[assembly: MelonGame("IncontinentCell", "My Dystopian Robot Girlfriend")]

namespace MoanMod
{
    public class MoanMod : MelonMod
    {
        private Il2Cpp.ModelBrain brain;
        private AudioPlayer audioPlayer;

        private bool shouldMouthBeOpen = false;
        private bool wasMouthOpen = false;
        private float currentMouthOpenAmount = 0.7f;

        // sex scene tracking
        private bool wasInSexScene = false;
        private float sexSceneStartCooldown = 0f;

        // headpat stuff
        private float lastHeadpatX = 0f;
        private float lastHeadpatY = 0f;
        private bool wasBeingHeadpatted = false;

        private bool wasCumming = false;
        private float moanTimer = 0f;
        private float moanCooldown = 1.4f;

        private bool pendingEndMoan = false;
        private bool playingEndMoan = false;
        private float endMoanTimer = 0f;

        private float pleasureLogTimer = 0f;

        // sex moan tracking
        private float lastPleasure = 0f;
        private float sexMoanTimer = 0f;
        private float sexMoanCooldown = 3.0f;
        private float mouthCloseTimer = 0f;

        // clustering
        private int currentClusterCount = 0;
        private float clusterDelayTimer = 0f;
        private bool isInCluster = false;

        // breath system
        private System.Collections.Generic.Queue<float> moanTimestamps = new System.Collections.Generic.Queue<float>();
        private bool lastActionWasBreath = false;
        private bool breathBeforeMoan = false;
        private bool breathPlaying = false;
        private float breathTimer = 0f;
        private float postBreathDelay = 0f;
        private string pendingMoanType = "";

        public override void OnInitializeMelon()
        {
            string gameVersion = Application.version;
            MelonLogger.Msg($"Game Version: {gameVersion}");

            // extract major version like "0.90" from "0.90.16"
            string[] versionParts = gameVersion.Split('.');
            string majorVersion = "";
            if (versionParts.Length >= 2)
            {
                majorVersion = $"{versionParts[0]}.{versionParts[1]}";
            }

            if (majorVersion != MoanModConfig.ExpectedGameVersion)
            {
                MelonLogger.Warning("================================================================================");
                MelonLogger.Warning("==================== !!! VERSION MISMATCH WARNING !!! =========================");
                MelonLogger.Warning("================================================================================");
                MelonLogger.Warning($"This mod was made for game version {MoanModConfig.ExpectedGameVersion}.x");
                MelonLogger.Warning($"You are running game version {gameVersion}");
                MelonLogger.Warning("The mod may have bugs, crashes, or not work correctly!");
                MelonLogger.Warning("Use at your own risk!");
                MelonLogger.Warning("================================================================================");
            }

            audioPlayer = new AudioPlayer();

            string modFolder = Path.Combine(MelonEnvironment.ModsDirectory, "MoanMod");

            try
            {
                audioPlayer.LoadAllAudioFiles(modFolder);
                MelonLogger.Msg($"Loaded audio files - {audioPlayer.GetLoadedFilesList()}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to load audio files: {ex.Message}");
            }
        }

        public override void OnUpdate()
        {
            audioPlayer.UpdateSoundManager();

            if (brain == null)
            {
                brain = UnityEngine.Object.FindObjectOfType<Il2Cpp.ModelBrain>();
                if (brain != null)
                {
                    MelonLogger.Msg("Found ModelBrain!");
                    pleasureLogTimer = MoanModConfig.Threshold.CheckInterval;
                }
                return;
            }

            bool inSexScene = IsInSexScene();
            if (inSexScene && !wasInSexScene)
            {
                var currentState = brain.CurrentState;
                var cowgirlState = currentState.TryCast<Il2Cpp.CowgirlBrainState>();

                if (cowgirlState != null)
                {
                    MelonLogger.Msg("=== Entered Cowgirl Scene ===");
                }
                else
                {
                    MelonLogger.Msg("=== Entered Sex Scene ===");
                }

                sexSceneStartCooldown = MoanModConfig.SexSceneStartCooldown;
                wasInSexScene = true;
            }
            else if (!inSexScene && wasInSexScene)
            {
                MelonLogger.Msg("=== Exited Sex Scene ===");
                wasInSexScene = false;

                if (brain != null)
                {
                    var controller = brain.ConnectedController;
                    if (controller != null)
                    {
                        var mouthController = controller.TryCast<Il2Cpp.ILive2DController_Mouth>();
                        if (mouthController?.ParamMouthOpen != null)
                        {
                            mouthController.ParamMouthOpen.UnclampedValue = 0.0f;
                        }
                    }
                }

                isInCluster = false;
                currentClusterCount = 0;
                clusterDelayTimer = 0f;
                sexMoanTimer = 0f;
                mouthCloseTimer = 0f;
                shouldMouthBeOpen = false;
                wasMouthOpen = false;
                pendingEndMoan = false;
                playingEndMoan = false;

                moanTimestamps.Clear();
                lastActionWasBreath = false;
                breathBeforeMoan = false;
                breathPlaying = false;
                breathTimer = 0f;
                postBreathDelay = 0f;
                pendingMoanType = "";
            }

            if (sexSceneStartCooldown > 0f)
            {
                sexSceneStartCooldown -= Time.deltaTime;
            }

            if (inSexScene && brain != null)
            {
                var controller = brain.ConnectedController;
                if (controller != null)
                {
                    var mouthController = controller.TryCast<Il2Cpp.ILive2DController_Mouth>();
                    if (mouthController?.ParamMouthOpen != null)
                    {
                        if (shouldMouthBeOpen)
                        {
                            mouthController.ParamMouthOpen.UnclampedValue = currentMouthOpenAmount;
                            wasMouthOpen = true;
                        }
                        else if (wasMouthOpen)
                        {
                            mouthController.ParamMouthOpen.UnclampedValue = 0.0f;
                            wasMouthOpen = false;
                        }
                    }
                }
            }

            UpdateBreathSystem();

            if (!inSexScene || sexSceneStartCooldown > 0f)
            {
                return;
            }

            pleasureLogTimer -= Time.deltaTime;
            if (pleasureLogTimer <= 0f)
            {
                float currentPleasure = brain.Pleasure;

                float pleasureChange = Mathf.Abs(currentPleasure - lastPleasure);

                // calculate threshold - more pleasure = easier to moan
                float pleasureNormalized = Mathf.Clamp01(currentPleasure / MoanModConfig.Threshold.PleasureCap);
                float thresholdRange = MoanModConfig.Threshold.BaseLow - MoanModConfig.Threshold.BaseHigh;
                float requiredChange = MoanModConfig.Threshold.BaseLow - (pleasureNormalized * thresholdRange);

                bool beingHeadpatted = IsBeingHeadpatted();

                if (beingHeadpatted && !wasBeingHeadpatted)
                {
                    MelonLogger.Msg("=== Started Headpatting (Applying +0.01 Penalty) ===");
                    wasBeingHeadpatted = true;
                }
                else if (!beingHeadpatted && wasBeingHeadpatted)
                {
                    MelonLogger.Msg("=== Stopped Headpatting (Removing Penalty) ===");
                    wasBeingHeadpatted = false;
                }

                if (beingHeadpatted)
                {
                    requiredChange += MoanModConfig.Modifiers.HeadpatPenalty;
                }

                var currentState = brain.CurrentState;
                var cowgirlState = currentState.TryCast<Il2Cpp.CowgirlBrainState>();
                if (cowgirlState != null)
                {
                    requiredChange *= MoanModConfig.Modifiers.CowgirlMultiplier;
                }

                if (pleasureChange > requiredChange && !(brain.ConnectedController?.Expression?.IsCumming ?? false) && sexMoanTimer <= 0f && !isInCluster && !pendingEndMoan && !playingEndMoan && audioPlayer.HasSexMoans)
                {
                    if (!brain.IsTalkingWithOverlay)
                    {
                        currentClusterCount = 1;
                        isInCluster = true;
                        PlaySexMoanInCluster();
                    }
                }

                lastPleasure = currentPleasure;
                pleasureLogTimer = MoanModConfig.Threshold.CheckInterval;
            }

            if (isInCluster && clusterDelayTimer > 0f)
            {
                clusterDelayTimer -= Time.deltaTime;

                if (clusterDelayTimer <= 0f && !breathBeforeMoan)
                {
                    bool continueMoaning = ShouldContinueCluster();

                    if (continueMoaning && currentClusterCount < MoanModConfig.Cluster.MaxMoans)
                    {
                        currentClusterCount++;

                        if (ShouldBreatheBeforeMoan())
                        {
                            StartBreathSequence("sex");
                        }
                        else
                        {
                            PlaySexMoanInCluster();
                        }
                    }
                    else
                    {
                        EndCluster();
                    }
                }
            }

            if (mouthCloseTimer > 0f)
            {
                mouthCloseTimer -= Time.deltaTime;
                if (mouthCloseTimer <= 0f)
                {
                    shouldMouthBeOpen = false;

                    // clear playing flag when end moan finishes
                    if (playingEndMoan)
                    {
                        playingEndMoan = false;
                    }
                }
            }

            if (sexMoanTimer > 0f)
            {
                sexMoanTimer -= Time.deltaTime;

                if (sexMoanTimer <= 0f)
                {
                    currentClusterCount = 0;
                    isInCluster = false;
                }
            }

            bool isCumming = brain.ConnectedController?.Expression?.IsCumming ?? false;

            if (isCumming && !wasCumming)
            {
                OnCummingStart();
            }
            else if (!isCumming && wasCumming)
            {
                OnCummingEnd();
            }

            wasCumming = isCumming;

            // pending end moan plays after last while moan
            if (pendingEndMoan)
            {
                endMoanTimer -= Time.deltaTime;
                if (endMoanTimer <= 0f)
                {
                    audioPlayer.PlayEndMoan(1.0f);

                    float endClipLength = audioPlayer.GetLastPlayedClipLength();
                    if (endClipLength > 0f)
                    {
                        currentMouthOpenAmount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
                        shouldMouthBeOpen = true;
                        mouthCloseTimer = endClipLength;
                    }

                    MelonLogger.Msg("Playing end moan!");
                    pendingEndMoan = false;
                    playingEndMoan = true;
                }
            }

            if (isCumming && audioPlayer.HasAudio)
            {
                moanTimer -= Time.deltaTime;

                if (moanTimer <= 0f && !breathBeforeMoan)
                {
                    if (!brain.IsTalkingWithOverlay)
                    {
                        Il2Cpp.GameVariables gameVars = Il2Cpp.GameScript.Instance.GameVariables;
                        if (gameVars != null)
                        {
                            moanCooldown = CalculateMoanFrequency(gameVars.lust, gameVars.sympathy);
                        }

                        if (ShouldBreatheBeforeMoan())
                        {
                            StartBreathSequence("cumming");
                        }
                        else
                        {
                            PlayCummingMoan();
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("Skipping moan - robot is talking");
                        moanTimer = moanCooldown;
                    }
                }
            }
        }

        private void OnCummingStart()
        {
            MelonLogger.Msg("=== Cumming Started ===");

            pendingEndMoan = false;
            playingEndMoan = false;

            isInCluster = false;
            currentClusterCount = 0;
            clusterDelayTimer = 0f;
            sexMoanTimer = 0f;
            mouthCloseTimer = 0f;

            breathBeforeMoan = false;
            breathPlaying = false;
            breathTimer = 0f;
            postBreathDelay = 0f;
            pendingMoanType = "";

            Il2Cpp.StoryBotDialogueStage dialogueStage = Il2Cpp.StorySingleton.Instance.Stage1;
            if (dialogueStage == null || !dialogueStage.IsPrologueFinished())
            {
                MelonLogger.Msg("Prologue not finished - moaning disabled");
                return;
            }

            Il2Cpp.GameVariables gameVars = Il2Cpp.GameScript.Instance.GameVariables;
            if (gameVars == null)
            {
                MelonLogger.Error("GameVariables not found!");
                return;
            }

            int lust = gameVars.lust;
            int sympathy = gameVars.sympathy;

            MelonLogger.Msg($"Stats - Lust: {lust}, Sympathy: {sympathy}");

            if (sympathy <= 5 || lust <= 10)
            {
                MelonLogger.Msg("Stats too low for moaning");
                return;
            }

            float sampleFreq = CalculateMoanFrequency(lust, sympathy);

            float lustNormalized = Mathf.Clamp01((lust - 10f) / 1990f);
            float sympathyNormalized = Mathf.Clamp01((sympathy - 5f) / 1495f);
            float statsFactor = (lustNormalized + sympathyNormalized) / 2f;
            float rangeMin = 1.0f - (statsFactor * 0.9f);
            float rangeMax = 1.8f - (statsFactor * 1.3f);

            MelonLogger.Msg($"Moan frequency range: {rangeMin:F2}s - {rangeMax:F2}s (random each moan)");

            audioPlayer.PlayStartMoan(1.0f);
            AddMoanTimestamp();
            float startClipLength = audioPlayer.GetLastPlayedClipLength();

            if (startClipLength > 0f)
            {
                currentMouthOpenAmount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
                shouldMouthBeOpen = true;
                mouthCloseTimer = startClipLength;

                MelonLogger.Msg($"Playing start moan! Length: {startClipLength:F2}s, Mouth: {currentMouthOpenAmount:F2}");
            }

            audioPlayer.ResetCooldowns();
            moanTimer = startClipLength;
        }

        private void OnCummingEnd()
        {
            MelonLogger.Msg("=== Cumming Ended ===");
            audioPlayer.ResetCooldowns();

            if (moanTimer > 0f)
            {
                endMoanTimer = moanTimer;
                pendingEndMoan = true;
                MelonLogger.Msg($"Scheduled end moan to play in {endMoanTimer:F2}s (after current moan finishes)");
            }
            else
            {
                audioPlayer.PlayEndMoan(1.0f);

                float endClipLength = audioPlayer.GetLastPlayedClipLength();
                if (endClipLength > 0f)
                {
                    currentMouthOpenAmount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
                    shouldMouthBeOpen = true;
                    mouthCloseTimer = endClipLength;
                    playingEndMoan = true;
                }

                MelonLogger.Msg("Playing end moan!");
            }

            // clear all expression modifiers when cumming ends
            if (brain?.ConnectedController?.Expression != null)
            {
                brain.ConnectedController.Expression.ClearExpression();
            }
        }

        private bool IsInSexScene()
        {
            if (brain == null) return false;
            var currentState = brain.CurrentState;
            if (currentState == null) return false;

            var fuckState = currentState.TryCast<Il2Cpp.GenericFuckBrainState>();
            if (fuckState != null) return true;

            var cowgirlState = currentState.TryCast<Il2Cpp.CowgirlBrainState>();
            return cowgirlState != null;
        }

        private bool IsBeingHeadpatted()
        {
            if (brain == null) return false;
            var controller = brain.ConnectedController;
            if (controller == null) return false;

            var headpatController = controller.TryCast<Il2Cpp.ILive2DController_Headpat>();
            if (headpatController == null) return false;

            if (headpatController.ParamHeadpat == null ||
                headpatController.ParamHeadpat.Value < 0.99f)
                return false;

            float currentX = headpatController.ParamHeadpatX?.Value ?? 0f;
            float currentY = headpatController.ParamHeadpatY?.Value ?? 0f;

            float xChange = Mathf.Abs(currentX - lastHeadpatX);
            float yChange = Mathf.Abs(currentY - lastHeadpatY);

            lastHeadpatX = currentX;
            lastHeadpatY = currentY;

            return (xChange >= MoanModConfig.Modifiers.HeadpatMovementMin || yChange >= MoanModConfig.Modifiers.HeadpatMovementMin);
        }

        private float CalculateMoanFrequency(int lust, int sympathy)
        {
            // normalize stats to 0-1
            float lustNormalized = Mathf.Clamp01((lust - 10f) / 1990f);
            float sympathyNormalized = Mathf.Clamp01((sympathy - 5f) / 1495f);
            float statsFactor = (lustNormalized + sympathyNormalized) / 2f;

            // low stats = 1.0-1.8s, high stats = 0.1-0.5s
            float rangeMin = 1.0f - (statsFactor * 0.9f);
            float rangeMax = 1.8f - (statsFactor * 1.3f);

            return UnityEngine.Random.Range(rangeMin, rangeMax);
        }

        private float CalculateSexMoanFrequency(float pleasure, int lust, int sympathy)
        {
            // pleasure weighted 50%, stats 25% each
            float pleasureFactor = pleasure;
            float lustFactor = Mathf.Clamp01((lust - 200f) / 1800f);
            float sympathyFactor = Mathf.Clamp01((sympathy - 150f) / 1350f);
            float combinedFactor = (pleasureFactor * 0.5f) + (lustFactor * 0.25f) + (sympathyFactor * 0.25f);

            // low = 3-5s, high = 0.5-2s
            float rangeMin = 3.0f - (combinedFactor * 2.5f);
            float rangeMax = 5.0f - (combinedFactor * 3.0f);

            return UnityEngine.Random.Range(rangeMin, rangeMax);
        }

        private void ApplyMoanExpressions(float duration)
        {
            if (brain == null) return;

            var expression = brain.ConnectedController?.Expression;
            if (expression == null) return;

            // check current lewdness and set minimum if needed
            float currentLewdness = expression._lastExpressionValues.Lewdness;
            if (currentLewdness < MoanModConfig.Expressions.LewdnessThreshold)
            {
                expression.AddModifier(
                    Il2Cpp.Live2DExpression.ExpressionModifierTypeEnum.Lewdness,
                    MoanModConfig.Expressions.LewdnessThreshold,
                    duration
                );
            }

            // always add happiness
            expression.AddModifier(
                Il2Cpp.Live2DExpression.ExpressionModifierTypeEnum.Happiness,
                MoanModConfig.Expressions.HappinessIncrease,
                duration
            );
        }

        private void PlaySexMoanInCluster()
        {
            lastActionWasBreath = false;

            audioPlayer.PlaySexMoan(1.0f);
            AddMoanTimestamp();

            currentMouthOpenAmount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
            shouldMouthBeOpen = true;

            float clipLength = audioPlayer.GetLastPlayedSexMoanLength();
            string clipName = audioPlayer.GetLastPlayedSexMoanName();

            ApplyMoanExpressions(clipLength);

            mouthCloseTimer = clipLength;

            MelonLogger.Msg($"Sex moan '{clipName}' (cluster #{currentClusterCount})! Clip: {clipLength:F2}s, Mouth: {currentMouthOpenAmount:F2}");

            clusterDelayTimer = clipLength + UnityEngine.Random.Range(MoanModConfig.Cluster.Delay.Min, MoanModConfig.Cluster.Delay.Max);
        }

        private bool ShouldContinueCluster()
        {
            if (currentClusterCount < 1 || currentClusterCount > MoanModConfig.Cluster.Probabilities.Length)
                return false;

            float probability = MoanModConfig.Cluster.Probabilities[currentClusterCount - 1];
            float roll = UnityEngine.Random.Range(0f, 1f);

            return roll < probability;
        }

        private void EndCluster()
        {
            Il2Cpp.GameVariables gameVars = Il2Cpp.GameScript.Instance.GameVariables;
            if (gameVars != null)
            {
                sexMoanCooldown = CalculateSexMoanFrequency(brain.Pleasure, gameVars.lust, gameVars.sympathy);
            }

            sexMoanTimer = sexMoanCooldown;
            isInCluster = false;

            MelonLogger.Msg($"Cluster ended after {currentClusterCount} moans. Cooldown: {sexMoanCooldown:F2}s");
        }

        private void AddMoanTimestamp()
        {
            moanTimestamps.Enqueue(Time.time);
        }

        private int GetMoanCountInWindow()
        {
            float currentTime = Time.time;
            float cutoffTime = currentTime - MoanModConfig.Breath.MoanTrackingWindow;

            while (moanTimestamps.Count > 0 && moanTimestamps.Peek() < cutoffTime)
            {
                moanTimestamps.Dequeue();
            }

            return moanTimestamps.Count;
        }

        private bool ShouldBreatheBeforeMoan()
        {
            if (!audioPlayer.HasBreaths) return false;
            if (lastActionWasBreath) return false;
            if (brain.IsTalkingWithOverlay) return false;

            int moanCount = GetMoanCountInWindow();
            int tier = Mathf.Min(moanCount / 2, MoanModConfig.Breath.Probabilities.Length - 1);
            float probability = MoanModConfig.Breath.Probabilities[tier];

            return UnityEngine.Random.Range(0f, 1f) < probability;
        }

        private void StartBreathSequence(string moanType)
        {
            float breathLength = audioPlayer.PlayBreath();
            if (breathLength > 0f)
            {
                breathBeforeMoan = true;
                breathPlaying = true;
                breathTimer = breathLength;
                pendingMoanType = moanType;
                lastActionWasBreath = true;

                // open mouth for breath
                currentMouthOpenAmount = UnityEngine.Random.Range(MoanModConfig.BreathMouthOpen.Min, MoanModConfig.BreathMouthOpen.Max);
                shouldMouthBeOpen = true;
                mouthCloseTimer = breathLength;

                string breathName = audioPlayer.GetLastPlayedBreathName();
                int moanCount = GetMoanCountInWindow();
                MelonLogger.Msg($"Breath '{breathName}'! Length: {breathLength:F2}s, Moans in last {MoanModConfig.Breath.MoanTrackingWindow:F1}s: {moanCount}");
            }
        }

        private void PlayCummingMoan()
        {
            lastActionWasBreath = false;

            audioPlayer.PlayRandomMoan(1.0f);
            AddMoanTimestamp();

            float clipLength = audioPlayer.GetLastPlayedClipLength();
            string clipName = audioPlayer.GetLastPlayedMoanName();
            moanTimer = clipLength + moanCooldown;

            ApplyMoanExpressions(clipLength);

            currentMouthOpenAmount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
            shouldMouthBeOpen = true;
            mouthCloseTimer = clipLength;

            MelonLogger.Msg($"Played moan '{clipName}'! Clip: {clipLength:F2}s, Cooldown: {moanCooldown:F2}s, Next in: {moanTimer:F2}s, Mouth: {currentMouthOpenAmount:F2}");
        }

        private void UpdateBreathSystem()
        {
            if (breathPlaying)
            {
                breathTimer -= Time.deltaTime;
                if (breathTimer <= 0f)
                {
                    breathPlaying = false;
                    postBreathDelay = UnityEngine.Random.Range(
                        MoanModConfig.Breath.DelayAfterMoan.Min,
                        MoanModConfig.Breath.DelayAfterMoan.Max
                    );
                }
            }

            if (!breathPlaying && postBreathDelay > 0f)
            {
                postBreathDelay -= Time.deltaTime;
                if (postBreathDelay <= 0f)
                {
                    if (pendingMoanType == "sex")
                    {
                        PlaySexMoanInCluster();
                    }
                    else if (pendingMoanType == "cumming")
                    {
                        PlayCummingMoan();
                    }

                    breathBeforeMoan = false;
                    pendingMoanType = "";
                }
            }
        }
    }
}