using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MoanMod
{
    public class AudioPlayer
    {
        private class MoanClip
        {
            public AudioClip Clip { get; set; }
            public string Name { get; set; }
            public int CooldownCounter { get; set; }

            public bool IsAvailable => CooldownCounter == 0;
        }

        private List<MoanClip> startMoans = new List<MoanClip>();
        private List<MoanClip> whileMoans = new List<MoanClip>();
        private List<MoanClip> endMoans = new List<MoanClip>();
        private List<MoanClip> sexMoans = new List<MoanClip>();
        private List<MoanClip> breathClips = new List<MoanClip>();
        private MoanClip lastPlayedMoan = null;
        private MoanClip lastPlayedSexMoan = null;
        private MoanClip lastPlayedBreath = null;
        private Il2Cpp.SoundSingleton soundManager;
        private System.Random random = new System.Random();

        public int StartMoansCount => startMoans.Count;
        public int WhileMoansCount => whileMoans.Count;
        public int EndMoansCount => endMoans.Count;
        public int SexMoansCount => sexMoans.Count;
        public int BreathCount => breathClips.Count;
        public bool HasAudio => whileMoans.Count > 0;
        public bool HasSexMoans => sexMoans.Count > 0;
        public bool HasBreaths => breathClips.Count > 0;

        public float GetLastPlayedClipLength()
        {
            if (lastPlayedMoan != null && lastPlayedMoan.Clip != null)
            {
                return lastPlayedMoan.Clip.length;
            }
            return 0f;
        }

        public float GetLastPlayedSexMoanLength()
        {
            if (lastPlayedSexMoan != null && lastPlayedSexMoan.Clip != null)
            {
                return lastPlayedSexMoan.Clip.length;
            }
            return 0f;
        }

        public string GetLastPlayedMoanName()
        {
            if (lastPlayedMoan != null)
            {
                return lastPlayedMoan.Name;
            }
            return "unknown";
        }

        public string GetLastPlayedSexMoanName()
        {
            if (lastPlayedSexMoan != null)
            {
                return lastPlayedSexMoan.Name;
            }
            return "unknown";
        }

        public string GetLastPlayedBreathName()
        {
            if (lastPlayedBreath != null)
            {
                return lastPlayedBreath.Name;
            }
            return "unknown";
        }

        public float GetLastPlayedBreathLength()
        {
            if (lastPlayedBreath != null && lastPlayedBreath.Clip != null)
            {
                return lastPlayedBreath.Clip.length;
            }
            return 0f;
        }

        public void LoadAllAudioFiles(string modFolder)
        {
            string startFolder = Path.Combine(modFolder, "cumming", "start");
            string whileFolder = Path.Combine(modFolder, "cumming", "while");
            string endFolder = Path.Combine(modFolder, "cumming", "end");
            string sexFolder = Path.Combine(modFolder, "while");
            string breathFolder = Path.Combine(modFolder, "breath");

            if (Directory.Exists(startFolder))
            {
                LoadAudioFromFolder(startFolder, startMoans, "start");
            }

            if (Directory.Exists(whileFolder))
            {
                LoadAudioFromFolder(whileFolder, whileMoans, "while");
            }

            if (Directory.Exists(endFolder))
            {
                LoadAudioFromFolder(endFolder, endMoans, "end");
            }

            if (Directory.Exists(sexFolder))
            {
                LoadAudioFromFolder(sexFolder, sexMoans, "sex");
            }

            if (Directory.Exists(breathFolder))
            {
                LoadAudioFromFolder(breathFolder, breathClips, "breath");
            }

            if (whileMoans.Count == 0)
            {
                throw new Exception("No audio files found in 'cumming/while' folder - this is required!");
            }
        }

        private void LoadAudioFromFolder(string folderPath, List<MoanClip> targetList, string category)
        {
            string[] wavFiles = Directory.GetFiles(folderPath, "*.wav");

            foreach (string filePath in wavFiles)
            {
                try
                {
                    AudioClip clip = LoadWavFile(filePath);
                    targetList.Add(new MoanClip
                    {
                        Clip = clip,
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        CooldownCounter = 0
                    });
                }
                catch (Exception ex)
                {
                    MelonLoader.MelonLogger.Warning($"Failed to load {category} moan {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            MelonLoader.MelonLogger.Msg($"Loaded {targetList.Count} {category} moans");
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
            if (soundManager == null)
            {
                soundManager = GameObject.FindObjectOfType<Il2Cpp.SoundSingleton>();
            }
        }

        public void PlayStartMoan(float volume = 1.0f)
        {
            if (soundManager == null || startMoans.Count == 0) return;

            int index = random.Next(0, startMoans.Count);
            MoanClip moan = startMoans[index];

            float gameSfxVolume = Il2Cpp.OptionsStatic.SfxVolume;
            soundManager.Play(moan.Clip, gameSfxVolume, null);
            lastPlayedMoan = moan;
        }

        public void PlayEndMoan(float volume = 1.0f)
        {
            if (soundManager == null || endMoans.Count == 0) return;

            int index = random.Next(0, endMoans.Count);
            MoanClip moan = endMoans[index];

            float gameSfxVolume = Il2Cpp.OptionsStatic.SfxVolume;
            soundManager.Play(moan.Clip, gameSfxVolume, null);
            lastPlayedMoan = moan;
        }

        public float PlayBreath()
        {
            if (soundManager == null || breathClips.Count == 0) return 0f;

            int index = random.Next(0, breathClips.Count);
            MoanClip breath = breathClips[index];

            float gameSfxVolume = Il2Cpp.OptionsStatic.SfxVolume;
            soundManager.Play(breath.Clip, gameSfxVolume, null);
            lastPlayedBreath = breath;

            return breath.Clip.length;
        }

        public void PlaySexMoan(float volume = 1.0f)
        {
            if (soundManager == null || sexMoans.Count == 0) return;

            MoanClip selectedMoan = null;

            if (lastPlayedSexMoan != null && random.NextDouble() < MoanModConfig.Cluster.RepeatChance)
            {
                if (lastPlayedSexMoan.IsAvailable)
                {
                    selectedMoan = lastPlayedSexMoan;
                }
            }

            if (selectedMoan == null)
            {
                selectedMoan = SelectRandomAvailableSexMoan();
            }

            if (selectedMoan != null)
            {
                float gameSfxVolume = Il2Cpp.OptionsStatic.SfxVolume;
                soundManager.Play(selectedMoan.Clip, gameSfxVolume, null);

                selectedMoan.CooldownCounter = MoanModConfig.Cluster.RepeatCooldown;
                lastPlayedSexMoan = selectedMoan;

                foreach (var moan in sexMoans)
                {
                    if (moan != selectedMoan && moan.CooldownCounter > 0)
                    {
                        moan.CooldownCounter--;
                    }
                }
            }
        }

        private MoanClip SelectRandomAvailableSexMoan()
        {
            var availableMoans = sexMoans.Where(m => m.IsAvailable).ToList();

            // if all on cooldown, free the one with lowest counter
            if (availableMoans.Count == 0)
            {
                var leastCooldown = sexMoans.OrderBy(m => m.CooldownCounter).First();
                leastCooldown.CooldownCounter = 0;
                availableMoans.Add(leastCooldown);
            }

            int index = random.Next(0, availableMoans.Count);
            return availableMoans[index];
        }

        public void PlayRandomMoan(float volume = 1.0f)
        {
            if (soundManager == null || whileMoans.Count == 0) return;

            MoanClip selectedMoan = null;

            if (lastPlayedMoan != null && random.NextDouble() < MoanModConfig.Cluster.RepeatChance)
            {
                if (lastPlayedMoan.IsAvailable)
                {
                    selectedMoan = lastPlayedMoan;
                }
            }

            if (selectedMoan == null)
            {
                selectedMoan = SelectRandomAvailableMoan();
            }

            if (selectedMoan != null)
            {
                float gameSfxVolume = Il2Cpp.OptionsStatic.SfxVolume;
                soundManager.Play(selectedMoan.Clip, gameSfxVolume, null);

                selectedMoan.CooldownCounter = MoanModConfig.Cluster.RepeatCooldown;
                lastPlayedMoan = selectedMoan;

                foreach (var moan in whileMoans)
                {
                    if (moan != selectedMoan && moan.CooldownCounter > 0)
                    {
                        moan.CooldownCounter--;
                    }
                }
            }
        }

        private MoanClip SelectRandomAvailableMoan()
        {
            var availableMoans = whileMoans.Where(m => m.IsAvailable).ToList();

            // if all on cooldown, free the one with lowest counter
            if (availableMoans.Count == 0)
            {
                var leastCooldown = whileMoans.OrderBy(m => m.CooldownCounter).First();
                leastCooldown.CooldownCounter = 0;
                availableMoans.Add(leastCooldown);
            }

            int index = random.Next(0, availableMoans.Count);
            return availableMoans[index];
        }

        public void ResetCooldowns()
        {
            foreach (var moan in whileMoans)
            {
                moan.CooldownCounter = 0;
            }
            lastPlayedMoan = null;
        }

        public void ResetSexMoanCooldowns()
        {
            foreach (var moan in sexMoans)
            {
                moan.CooldownCounter = 0;
            }
            lastPlayedSexMoan = null;
        }

        public string GetLoadedFilesList()
        {
            return $"Cumming(Start: {startMoans.Count}, While: {whileMoans.Count}, End: {endMoans.Count}) | Sex: {sexMoans.Count} | Breath: {breathClips.Count}";
        }
    }
}