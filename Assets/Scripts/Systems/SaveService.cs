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
                PreserveCorruptSave();
                return SaveData.CreateDefault();
            }
            catch (Exception)
            {
                PreserveCorruptSave();
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

        private void PreserveCorruptSave()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
                string backupPath = Path.Combine(directoryPath, $"echoes-save.corrupt-{stamp}.json");
                int suffix = 1;
                while (File.Exists(backupPath))
                    backupPath = Path.Combine(directoryPath, $"echoes-save.corrupt-{stamp}-{suffix++}.json");
                File.Move(SavePath, backupPath);
            }
            catch (Exception)
            {
                // Recovery must never stop a player from starting a safe new run.
            }
        }

        private static SaveData Sanitize(SaveData data)
        {
            data.Version = SaveData.CurrentVersion;
            if (string.IsNullOrWhiteSpace(data.CheckpointId)) data.CheckpointId = SaveData.StartCheckpointId;
            if (data.CollectedCoreIds == null) data.CollectedCoreIds = new System.Collections.Generic.List<string>();
            if (data.CollectedRelicIds == null) data.CollectedRelicIds = new System.Collections.Generic.List<string>();
            if (data.RecentRuns == null) data.RecentRuns = new System.Collections.Generic.List<RunRecord>();
            if (data.RecentRuns.Count > 5) data.RecentRuns.RemoveRange(5, data.RecentRuns.Count - 5);
            if (data.Settings == null) data.Settings = new GameSettings();
            if (string.IsNullOrWhiteSpace(data.ObjectiveStage)) data.ObjectiveStage = ObjectiveStage.Briefing.ToString();
            data.Settings.MasterVolume = Mathf.Clamp01(data.Settings.MasterVolume);
            data.Settings.MusicVolume = Mathf.Clamp01(data.Settings.MusicVolume);
            data.Settings.SfxVolume = Mathf.Clamp01(data.Settings.SfxVolume);
            data.Settings.MouseSensitivity = Mathf.Clamp(data.Settings.MouseSensitivity, 0.2f, 3f);
            return data;
        }
    }
}
