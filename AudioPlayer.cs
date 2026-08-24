using Il2CppFungus;
using MelonLoader;
using UnityEngine;

namespace MoanMod;
public enum AudioType
{
    CumStart,
    CumWhile,
    CumEnd,
    Sex,
    Breath
}

class MoanClip
{
    public AudioClip Clip { get; init; }
    public string Name { get; init; }
    public int CooldownCounter { get; set; }

    public bool IsAvailable => CooldownCounter <= 0;
}

class ClipCollection
{
    public List<MoanClip> Clips { get; } = new();
    public MoanClip LastPlayed { get; private set; }

    public int Count => Clips.Count;
    public bool HasAudio => Count > 0;

    public MoanClip SelectNext(System.Random rnd, bool useCooldowns = false)
    {
        if (!HasAudio) return null;

        MoanClip selectedMoan = null;

        // Apply repeat chance if cooldowns are enabled and we get lucky
        var maybeRepeat = useCooldowns && LastPlayed?.IsAvailable == true;
        if (maybeRepeat && rnd.NextDouble() < MoanModConfig.Cluster.RepeatChance)
        {
            selectedMoan = LastPlayed;
        }

        // If we didn't repeat, pick a random available clip
        selectedMoan ??= PickAvailable(rnd, useCooldowns);

        LastPlayed = selectedMoan;
        if (!useCooldowns) return selectedMoan;


        selectedMoan.CooldownCounter = MoanModConfig.Cluster.RepeatCooldown;
        foreach (var clip in Clips.Where(c => c != selectedMoan && c.CooldownCounter > 0))
            clip.CooldownCounter -= 1;
        
        return selectedMoan;
    }

    private MoanClip PickAvailable(System.Random rnd, bool useCooldowns)
    {
        if (!useCooldowns) return Clips[rnd.Next(Clips.Count)];
        
        var available = Clips.Where(c => c.IsAvailable).ToList();
        if (available.Count == 0)
        {
            var leastCooldown = Clips.MinBy(c => c.CooldownCounter)!; // Will never be null because we fail fast in SelectNext if !HasAudio
            leastCooldown.CooldownCounter = 0;
            available.Add(leastCooldown);
        }

        return available[rnd.Next(available.Count)];;
    }

    public void ResetCooldowns()
    {
        foreach (var clip in Clips) clip.CooldownCounter = 0;
        LastPlayed = null;
    }
}

public class AudioPlayer
{
    private readonly Dictionary<AudioType, ClipCollection> _audioCollections = new();
    private  Il2Cpp.SoundSingleton _soundManager;
    private readonly System.Random _random = new();

    public AudioPlayer()
    {
        foreach (AudioType type in Enum.GetValues(typeof(AudioType)))
            _audioCollections[type] = new ClipCollection();
    }

    public int GetCountFor(AudioType type) => _audioCollections[type].Count;
    public bool HasAudioFor(AudioType type) => _audioCollections[type].HasAudio;
    public float LastPlayedLengthFor(AudioType type) => _audioCollections[type].LastPlayed?.Clip?.length ?? 0;
    public string LastPlayedNameFor(AudioType type) => _audioCollections[type].LastPlayed?.Name ?? "Unknown";

    public void LoadAllAudioFiles(string modFolder)
    {

        var loadingMap = new[]
        {
            ( Path.Combine(modFolder, "cumming", "start"), AudioType.CumStart ),
            ( Path.Combine(modFolder, "cumming", "while"), AudioType.CumWhile ),
            ( Path.Combine(modFolder, "cumming", "end"), AudioType.CumEnd ),
            ( Path.Combine(modFolder, "while"), AudioType.Sex ),
            ( Path.Combine(modFolder, "breath"), AudioType.Breath)
        };

        foreach (var (folder, type) in loadingMap)
        {
            LoadAudioFromFolder(folder, type);
        }

        if (!_audioCollections[AudioType.CumWhile].HasAudio)
            throw new Exception("No audio files found in 'cumming/while' folder - this is required!");
    }

    private void LoadAudioFromFolder(string folderPath, AudioType type)
    {
        if (!Directory.Exists(folderPath)) return;

        var collection = _audioCollections[type];
        string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");

        foreach (string filePath in wavFiles)
        {
            try
            {
                AudioClip clip = LoadWavFile(filePath);
                collection.Clips.Add(new MoanClip
                {
                    Clip = clip,
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    CooldownCounter = 0
                });
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"Failed to load {type} moan {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        MelonLogger.Msg($"Loaded {collection.Count} {type} moans");
    }

    private AudioClip LoadWavFile(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);

        int channels = BitConverter.ToUInt16(fileBytes, 22);
        int sampleRate = BitConverter.ToInt32(fileBytes, 24);
        int bitDepth = BitConverter.ToUInt16(fileBytes, 34);

        // standard WAV header is 44 bytes
        int dataOffset = 44;

        float[] audioData = ConvertBytesToFloat(fileBytes, dataOffset, bitDepth);

        int sampleCount = audioData.Length / channels;
        AudioClip clip = AudioClip.Create(Path.GetFileNameWithoutExtension(filePath), sampleCount, channels, sampleRate, false);
        clip.SetData(audioData, 0);
        clip.hideFlags = HideFlags.DontUnloadUnusedAsset;

        return clip;
    }

    private float[] ConvertBytesToFloat(byte[] bytes, int offset, int bitDepth)
    {
        int dataSize = bytes.Length - offset;
        float[] floatData;

        if (bitDepth == 16)
        {
            // 16-bit PCM
            int sampleCount = dataSize / 2;
            floatData = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(bytes, offset + i * 2);
                floatData[i] = sample / 32768f;
            }
        }
        else if (bitDepth == 8)
        {
            // 8-bit PCM
            floatData = new float[dataSize];

            for (int i = 0; i < dataSize; i++)
            {
                floatData[i] = (bytes[offset + i] - 128) / 128f;
            }
        }
        else
        {
            throw new NotSupportedException($"Bit depth {bitDepth} not supported. Use 8 or 16-bit WAV files.");
        }

        return floatData;
    }

    public void UpdateSoundManager()
    {
        if (_soundManager is not null) return;  

        _soundManager = Il2Cpp.SoundSingleton.Instance;
    }
    private float PlayAudioType(AudioType type, bool useCooldowns, float volumeMultiplier = 1.0f)
    {
        var collection = _audioCollections[type];
        var clipToPlay = collection.SelectNext(_random, useCooldowns);

        if (_soundManager == null || clipToPlay == null) return 0f;

        float finalVolume = Mathf.Clamp01(Il2Cpp.OptionsStatic.SfxVolume * volumeMultiplier);
        _soundManager.Play(clipToPlay.Clip, finalVolume, null);

        return clipToPlay.Clip.length;
    }
    public void PlayStartMoan(float volume = 1.0f) => PlayAudioType(AudioType.CumStart, false, volume);
    public void PlayEndMoan(float volume = 1.0f) => PlayAudioType(AudioType.CumEnd, false, volume);
    public float PlayBreath() => PlayAudioType(AudioType.Breath, false);

    public void PlaySexMoan(float volume = 1.0f) => PlayAudioType(AudioType.Sex, true, volume);
    public void PlayRandomMoan(float volume = 1.0f) => PlayAudioType(AudioType.CumWhile, true, volume);

    public void ResetCooldownsFor(AudioType type) => _audioCollections[type].ResetCooldowns();

    public string GetLoadedFilesList()
    {
        return $"Cumming(Start: {GetCountFor(AudioType.CumStart)}, While: {GetCountFor(AudioType.CumWhile)}, End: {GetCountFor(AudioType.CumEnd)}) | Sex: {GetCountFor(AudioType.Sex)} | Breath: {GetCountFor(AudioType.Breath)}";
    }
}