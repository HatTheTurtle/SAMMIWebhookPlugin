using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Game;

namespace SammiPlugin
{
    public class Service
    {
        [PluginService] public static IPluginLog PluginLog {get; private set; }
        [PluginService] public static IClientState ClientState {get; private set; }
        [PluginService] public static ICondition Condition {get; private set; }
        [PluginService] public static IFramework Framework {get; private set; }
        [PluginService] public static ISigScanner SigScanner {get; private set; }
        [PluginService] public static IDataManager DataManager {get; private set; }
        [PluginService] public static IGameInteropProvider GameInteropProvider {get; private set; }
        [PluginService] public static IFlyTextGui FlyTextGui {get; private set; }
        [PluginService] public static IChatGui ChatGui {get; private set; }
    }
}
