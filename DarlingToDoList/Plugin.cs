using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using DarlingToDoList.Windows;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace DarlingToDoList
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/dtd";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }

        public readonly WindowSystem WindowSystem = new("DarlingToDoList");
        private MainWindow MainWindow { get; init; }
        private DebugWindow DebugWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            MainWindow = new MainWindow(this);
            DebugWindow = new DebugWindow(this);

            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(DebugWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open to do UI"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;

            PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();

            MainWindow.Dispose();
            DebugWindow.Dispose();

            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            ToggleMainUI();
        }

        private void DrawUI() => WindowSystem.Draw();

        public void ToggleDebugUI() => DebugWindow.Toggle();
        public void ToggleMainUI() => MainWindow.Toggle();
    }
}
