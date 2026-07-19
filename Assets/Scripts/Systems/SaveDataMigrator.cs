using System.Collections.Generic;

namespace EchoesOfTheRuins
{
    public static class SaveDataMigrator
    {
        public static SaveData Migrate(SaveData data)
        {
            if (data == null) return SaveData.CreateDefault();
            if (data.Version < 4)
            {
                bool completed = data.Version > 0 && data.CheckpointId == "complete";
                data.ObjectiveStage = DeriveObjectiveStage(data.CollectedCoreIds == null ? 0 : data.CollectedCoreIds.Count, data.CheckpointId, completed);
            }
            data.Version = SaveData.CurrentVersion;
            if (string.IsNullOrWhiteSpace(data.ObjectiveStage))
                data.ObjectiveStage = DeriveObjectiveStage(data.CollectedCoreIds == null ? 0 : data.CollectedCoreIds.Count, data.CheckpointId, false);
            return data;
        }

        public static SaveData MigrateFromV3(int cores, string checkpoint, bool completed)
        {
            var data = SaveData.CreateDefault();
            data.Version = 3;
            data.CheckpointId = checkpoint;
            data.CollectedCoreIds = new List<string>();
            for (int index = 0; index < cores; index++) data.CollectedCoreIds.Add("core-" + index);
            if (completed) data.CheckpointId = "complete";
            return Migrate(data);
        }

        private static string DeriveObjectiveStage(int cores, string checkpoint, bool completed)
        {
            if (completed) return ObjectiveStage.Complete.ToString();
            if (cores >= 3) return ObjectiveStage.ReachExit.ToString();
            if (cores == 0 && (checkpoint == SaveData.StartCheckpointId || checkpoint == "Start Checkpoint")) return ObjectiveStage.Briefing.ToString();
            return ObjectiveStage.CollectCores.ToString();
        }
    }
}
