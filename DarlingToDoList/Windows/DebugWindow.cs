using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace DarlingToDoList.Windows
{
    public class DebugWindow : Window, IDisposable
    {
        private Plugin Plugin;

        public DebugWindow(Plugin plugin)
            : base("Debug Info###Debug Info Window")
        {
            Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoScrollWithMouse;

            Size = new Vector2(375, 300);
            SizeCondition = ImGuiCond.Appearing;

            Plugin = plugin;
        }

        public void Dispose() { }

        public override void Draw()
        {
            var nextDailyReset = GetNextDailyReset();
            var nextWeeklyReset = GetNextWeeklyReset();

            ImGui.Text($"Next Daily Reset: {nextDailyReset:G}");
            ImGui.Text($"Time Until Daily Reset: {GetTimeUntil(nextDailyReset)}");

            ImGui.Text($"Next Weekly Reset: {nextWeeklyReset:G}");
            ImGui.Text($"Time Until Weekly Reset: {GetTimeUntil(nextWeeklyReset)}");

            if (ImGui.Button("Simulate Daily Reset"))
            {
                SimulateDailyReset();
            }

            if (ImGui.Button("Simulate Weekly Reset"))
            {
                SimulateWeeklyReset();
            }
        }

        private DateTime GetNextDailyReset()
        {
            var now = DateTime.UtcNow;
            var resetTimeUtc = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0, DateTimeKind.Utc); // 11 AM EDT / 5 PM CEST is 3 PM UTC

            if (resetTimeUtc <= now)
                resetTimeUtc = resetTimeUtc.AddDays(1);

            return TimeZoneInfo.ConvertTimeFromUtc(resetTimeUtc, TimeZoneInfo.Local);
        }

        private DateTime GetNextWeeklyReset()
        {
            var now = DateTime.UtcNow;
            var resetTimeUtc = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc); // 4 AM EDT / 10 AM CEST is 8 AM UTC

            // Move to the next Tuesday
            while (resetTimeUtc.DayOfWeek != DayOfWeek.Tuesday || resetTimeUtc <= now)
            {
                resetTimeUtc = resetTimeUtc.AddDays(1);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(resetTimeUtc, TimeZoneInfo.Local);
        }

        private string GetTimeUntil(DateTime resetTime)
        {
            var timeSpan = resetTime - DateTime.Now;
            return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        private void SimulateDailyReset()
        {
            foreach (var category in Plugin.Configuration.Categories.Values)
            {
                foreach (var item in category)
                {
                    if (item.ResetDaily)
                    {
                        item.IsCompleted = false;
                    }
                }
            }
            Plugin.Configuration.LastResetCheck = DateTime.UtcNow;
            Plugin.Configuration.Save();
        }

        private void SimulateWeeklyReset()
        {
            foreach (var category in Plugin.Configuration.Categories.Values)
            {
                foreach (var item in category)
                {
                    if (item.ResetWeekly)
                    {
                        item.IsCompleted = false;
                    }
                }
            }
            Plugin.Configuration.LastResetCheck = DateTime.UtcNow;
            Plugin.Configuration.Save();
        }
    }
}
