using System.IO;
using UnityEngine;

namespace Game.Core
{
    public class SaveSystem
    {
        private const string FileName = "save.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Saved data to {SavePath}");
        }

        public SaveData LoadOrCreate()
        {
            if (!File.Exists(SavePath))
            {
                return new SaveData();
            }

            var json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }
    }
}
