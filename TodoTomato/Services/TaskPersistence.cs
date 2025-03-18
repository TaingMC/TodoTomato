using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoTomato.Services
{
    public static class TaskPersistence
    {
        private static readonly string FilePath = "tasks.json";
        private const string ImportantTasksFileName = "important_tasks.json";
        private const string CompletedTasksFileName = "completed_tasks.json";
        private const string CustomCollectionsFileName = "customCollections.json";
        private static readonly string PomodoroCountPath = "pomodoroCount.txt";
        
        public static async Task<ObservableCollection<string>> LoadTasksAsync()
        {
            if (!File.Exists(FilePath))
                return new ObservableCollection<string>();

            var json = await File.ReadAllTextAsync(FilePath);
            var tasks = JsonSerializer.Deserialize<ObservableCollection<string>>(json);
            return tasks ?? new ObservableCollection<string>();
        }

        public static async Task<IEnumerable<string>> LoadCompletedTasksAsync()
        {
            if (!File.Exists(CompletedTasksFileName))
                return Array.Empty<string>();

            var json = await File.ReadAllTextAsync(CompletedTasksFileName);
            return JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? Array.Empty<string>();
        }
        
        public static async Task<IEnumerable<string>> LoadImportantTasksAsync()
        {
            if (!File.Exists(ImportantTasksFileName))
                return Array.Empty<string>();

            var json = await File.ReadAllTextAsync(ImportantTasksFileName);
            return JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? Array.Empty<string>();
        }
        
        public static async Task<Dictionary<string, ObservableCollection<string>>> LoadCustomCollectionsAsync()
        {
            try
            {
                if (!File.Exists(CustomCollectionsFileName))
                {
                    return new Dictionary<string, ObservableCollection<string>>();
                }

                var json = await File.ReadAllTextAsync(CustomCollectionsFileName);
                var temp = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);

                if (temp == null)
                {
                    return new Dictionary<string, ObservableCollection<string>>();
                }

                return temp.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new ObservableCollection<string>(kvp.Value));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading custom collections: {ex.Message}", ex);
            }
        }
        
        public static async Task SaveTasksAsync(ObservableCollection<string> tasks)
        {
            var json = JsonSerializer.Serialize(tasks);
            await File.WriteAllTextAsync(FilePath, json);
        }

        public static async Task SaveImportantTasksAsync(ObservableCollection<string> tasks)
        {
            var json = JsonSerializer.Serialize(tasks);
            await File.WriteAllTextAsync(ImportantTasksFileName, json);
        }

        public static async Task SaveCompletedTasksAsync(ObservableCollection<string> tasks)
        {
            var json = JsonSerializer.Serialize(tasks);
            await File.WriteAllTextAsync(CompletedTasksFileName, json);
        }
        
        public static async Task SaveCustomCollectionsAsync(Dictionary<string, ObservableCollection<string>> collections)
        {
            try
            {
                var serializableCollections = collections.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList());

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(serializableCollections, options);
                await File.WriteAllTextAsync(CustomCollectionsFileName, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save custom collections: {ex.Message}", ex);
            }
        }
        
        public static async Task SavePomodoroCountAsync(int count)
        {
            await File.WriteAllTextAsync(PomodoroCountPath, count.ToString());
        }
        
        public static async Task<int> LoadPomodoroCountAsync()
        {
            if (File.Exists(PomodoroCountPath))
            {
                var countStr = await File.ReadAllTextAsync(PomodoroCountPath);
                if (int.TryParse(countStr, out int count))
                {
                    return count;
                }
            }
            return 0;
        }
    }
}