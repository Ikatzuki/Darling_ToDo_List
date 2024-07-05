using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace DarlingToDoList
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        // Categories and their items
        public Dictionary<string, List<ToDoItem>> Categories { get; set; } = new Dictionary<string, List<ToDoItem>>();

        // Last reset check timestamp
        public DateTime LastResetCheck { get; set; } = DateTime.UtcNow;

        // Window lock state
        public bool IsWindowLocked { get; set; } = false; // Default to unlocked

        // Save method
        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface!.SavePluginConfig(this);
        }
    }

    [Serializable]
    public class ToDoItem
    {
        public string Name { get; set; }
        public bool IsCompleted { get; set; }
        public bool ResetDaily { get; set; } // New property to indicate if the item should reset daily
        public bool ResetWeekly { get; set; } // New property to indicate if the item should reset weekly
    }
}
