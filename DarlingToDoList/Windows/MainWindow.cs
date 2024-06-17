using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace DarlingToDoList.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private Plugin Plugin;
        private DebugWindow DebugWindow;

        private string newCategoryName = string.Empty;
        private string newTodoItem = string.Empty;
        private bool newTodoItemResetDaily = false;
        private bool newTodoItemResetWeekly = false;
        private bool isEditMode = false;

        public MainWindow(Plugin plugin)
            : base("Darling To Do List###Main Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            Plugin = plugin;
            DebugWindow = new DebugWindow(plugin);

            // Set the initial lock state based on configuration
            Flags = Plugin.Configuration.IsWindowLocked ? ImGuiWindowFlags.NoMove : ImGuiWindowFlags.None;
        }

        public void Dispose()
        {
            DebugWindow.Dispose();
        }

        public override void Draw()
        {
            // Ensure the reset checks are performed
            CheckResets();

            // To-do list section
            ImGui.Spacing();
            ImGui.Text("To-Do List");
            ImGui.SameLine();
            if (ImGui.Button(isEditMode ? "Exit Edit Mode" : "Edit Mode"))
            {
                isEditMode = !isEditMode;
            }

            ImGui.SameLine();

            if (ImGui.Button("Debug Info"))
            {
                Plugin.ToggleDebugUI();
            }

            ImGui.SameLine();

            // Lock/Unlock button
            if (ImGui.Button(Plugin.Configuration.IsWindowLocked ? "Unlock Window" : "Lock Window"))
            {
                Plugin.Configuration.IsWindowLocked = !Plugin.Configuration.IsWindowLocked;
                Flags = Plugin.Configuration.IsWindowLocked ? ImGuiWindowFlags.NoMove : ImGuiWindowFlags.None;
                Plugin.Configuration.Save();
            }

            // Separator line below the top bar
            DrawSeparatorLine();

            ImGui.Spacing();
            ImGui.Spacing();

            // Display categories and their to-do items
            foreach (var category in Plugin.Configuration.Categories)
            {
                ImGui.Text(category.Key);

                // Add dummy space to ensure consistent spacing
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(0, 25));

                if (isEditMode)
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"X##DeleteCategory_{category.Key}"))
                    {
                        Plugin.Configuration.Categories.Remove(category.Key);
                        Plugin.Configuration.Save();
                        break;
                    }
                }

                ImGui.Indent();
                foreach (var item in category.Value)
                {
                    bool isCompleted = item.IsCompleted;
                    if (ImGui.Checkbox($"##{item.Name}", ref isCompleted))
                    {
                        item.IsCompleted = isCompleted;
                        Plugin.Configuration.Save();
                    }

                    ImGui.SameLine();
                    if (item.IsCompleted)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        ImGui.TextDisabled($"{item.Name}");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.85f, 0, 1.0f)); // #ffd800 color
                        ImGui.Text($"{item.Name}");
                        ImGui.PopStyleColor();
                    }

                    if (isEditMode)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"X##DeleteItem_{category.Key}_{item.Name}"))
                        {
                            category.Value.Remove(item);
                            Plugin.Configuration.Save();
                            break;
                        }
                    }
                }

                // Add new item button at the end of the items list
                if (isEditMode)
                {
                    ImGui.Spacing();
                    if (ImGui.Button($"+##{category.Key}"))
                    {
                        ImGui.OpenPopup($"Add To-Do Item##{category.Key}");
                    }

                    if (ImGui.BeginPopup($"Add To-Do Item##{category.Key}"))
                    {
                        ImGui.InputText("To-Do Item", ref newTodoItem, 100);
                        ImGui.Checkbox("Reset Daily", ref newTodoItemResetDaily);
                        ImGui.Checkbox("Reset Weekly", ref newTodoItemResetWeekly);
                        if (ImGui.Button("Add"))
                        {
                            if (!string.IsNullOrEmpty(newTodoItem))
                            {
                                Plugin.Configuration.Categories[category.Key].Add(new ToDoItem { Name = newTodoItem, IsCompleted = false, ResetDaily = newTodoItemResetDaily, ResetWeekly = newTodoItemResetWeekly });
                                newTodoItem = string.Empty; // Clear the input text
                                newTodoItemResetDaily = false; // Reset the checkbox
                                newTodoItemResetWeekly = false; // Reset the checkbox
                                Plugin.Configuration.Save();
                                ImGui.CloseCurrentPopup();
                            }
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Cancel"))
                        {
                            newTodoItem = string.Empty;
                            newTodoItemResetDaily = false; // Reset the checkbox
                            newTodoItemResetWeekly = false; // Reset the checkbox
                            ImGui.CloseCurrentPopup();
                        }
                        ImGui.EndPopup();
                    }
                }
                ImGui.Unindent();
            }

            // Show "New Category" button in edit mode at the end of the categories list
            if (isEditMode)
            {
                ImGui.Spacing();
                if (ImGui.Button("New Category"))
                {
                    ImGui.OpenPopup("New Category Popup");
                }

                if (ImGui.BeginPopup("New Category Popup"))
                {
                    ImGui.InputText("Category Name", ref newCategoryName, 100);
                    if (ImGui.Button("Create"))
                    {
                        if (!string.IsNullOrEmpty(newCategoryName) && !Plugin.Configuration.Categories.ContainsKey(newCategoryName))
                        {
                            Plugin.Configuration.Categories.Add(newCategoryName, new List<ToDoItem>());
                            newCategoryName = string.Empty; // Clear the input text
                            Plugin.Configuration.Save();
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        newCategoryName = string.Empty;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }
            }
        }

        private void DrawSeparatorLine()
        {
            ImGui.Spacing(); // Adding some spacing for better visual separation
            ImGui.Spacing();
            var drawList = ImGui.GetWindowDrawList();
            var windowPos = ImGui.GetWindowPos();
            var cursorPos = ImGui.GetCursorScreenPos();
            var windowSize = ImGui.GetWindowSize();
            var lineStartPos = new Vector2(windowPos.X + 10, cursorPos.Y);
            var lineLength = windowSize.X * 0.75f; // Extend the line length

            for (int i = 0; i < lineLength; i++)
            {
                // Calculate alpha value to create a fade effect at both ends
                float alpha = 1.0f;
                if (i < lineLength * 0.1f) // Fade in at the start
                {
                    alpha = i / (lineLength * 0.1f);
                }
                else if (i > lineLength * 0.6f) // Fade out at the end
                {
                    alpha = 1.0f - (i - lineLength * 0.6f) / (lineLength * 0.4f);
                }
                uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, alpha));
                drawList.AddLine(new Vector2(lineStartPos.X + i, lineStartPos.Y), new Vector2(lineStartPos.X + i + 1, lineStartPos.Y), color);
            }
        }

        private void CheckResets()
        {
            var now = DateTime.UtcNow;
            var lastResetCheck = Plugin.Configuration.LastResetCheck;

            var dailyResetTime = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0, DateTimeKind.Utc); // 3 PM UTC, equivalent to 11 AM EDT / 5 PM CEST
            var weeklyResetTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc); // 8 AM UTC, equivalent to 4 AM EDT / 10 AM CEST

            // Check if a daily reset has occurred
            if (now >= dailyResetTime && lastResetCheck < dailyResetTime)
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
            }

            // Check if a weekly reset has occurred
            if (now.DayOfWeek == DayOfWeek.Tuesday && now >= weeklyResetTime && lastResetCheck < weeklyResetTime)
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
            }

            Plugin.Configuration.LastResetCheck = now;
            Plugin.Configuration.Save();
        }
    }
}
