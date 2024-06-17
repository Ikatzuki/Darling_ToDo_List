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

        public bool IsConfigWindowMovable { get; set; } = true;
        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        // Categories and their items
        public Dictionary<string, List<ToDoItem>> Categories { get; set; } = new Dictionary<string, List<ToDoItem>>();

        // Last reset check timestamp
        public DateTime LastResetCheck { get; set; } = DateTime.UtcNow;

        // Save method
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
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
