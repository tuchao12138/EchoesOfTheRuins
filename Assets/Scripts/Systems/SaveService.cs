using System;
using System.IO;
using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Small, versioned, offline-only save service. It never blocks play when data is missing or damaged.</summary>
    public sealed class SaveService
    {
        private const string SaveFileName = "echoes-save.json";
        private readonly string directoryPath;

        public SaveService(string overrideDirectory = null)
        {
            directoryPath = string.IsNullOrWhiteSpace(overrideDirectory)
                ? Path.Combine(Application.persistentDataPath, "EchoesOfTheRuins")
                : overrideDirectory;
        }

        private string SavePath => Path.Combine(directoryPath, SaveFileName);

        public bool HasSave => File.Exists(SavePath);

        public SaveData Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return SaveData.CreateDefault();
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if (IsValid(data)) return Sanitize(SaveDataMigrator.Migrate(data));
            }
            catch (Exception)
            {
                return SaveData.CreateDefault();
            }
        }

        public void Save(SaveData data)
        {
            var safeData = Sanitize(data ?? SaveData.CreateDefault());
            safeData.LastPlayedUtc = DateTime.UtcNow.ToString("O");
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(SavePath, JsonUtility.ToJson(safeData, true));
        }

        public void Delete()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        private static bool IsValid(SaveData data) => data != null && data.Version > 0 && data.Version <= SaveData.CurrentVersion;

        private static SaveData Sanitize(SaveData data)
        {
            data.Version = SaveData.CurrentVersion;
            if (string.IsNullOrWhiteSpace(data.CheckpointId)) data.CheckpointId = SaveData.StartCheckpointId;
            if (data.CollectedCoreIds == null) data.CollectedCoreIds = new System.Collections.Generic.List<string>();
            if (data.CollectedRelicIds == null) data.CollectedRelicIds = new System.Collections.Generic.List<string>();
            if (data.Settings == null) data.Settings = new GameSettings();
            data.Settings.MasterVolume = Mathf.Clamp01(data.Settings.MasterVolume);
            data.Settings.MusicVolume = Mathf.Clamp01(data.Settings.MusicVolume);
            data.Settings.SfxVolume = Mathf.Clamp01(data.Settings.SfxVolume);
            data.Settings.MouseSensitivity = Mathf.Clamp(data.Settings.MouseSensitivity, 0.2f, 3f);
            return data;
        }
    }
}
            if (string.IsNullOrWhiteSpace(data.ObjectiveStage)) data.ObjectiveStage = ObjectiveStage.Briefing.ToString();
