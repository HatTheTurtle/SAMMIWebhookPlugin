using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;
using Dalamud.Game.ClientState;
using Dalamud.Game;
using System.Dynamic;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace SammiPlugin
{
    public class Service
    {
        [PluginService] public static IPluginLog PluginLog {get; private set; }
        //[PluginService] public static IPlayerCharacter PlayerCharacter {get; private set; }
        [PluginService] public static IClientState ClientState {get; private set; }
        [PluginService] public static ICondition Condition {get; private set; }
        [PluginService] public static IFramework Framework {get; private set; }

        //User IGameInteropProvider to hook into process and grab action info
        //[PluginService] public static IGameInteropProvider GameInteropProvider {get; private set; }
        //or just use flytext and do things poorly lol
        [PluginService] public static IFlyTextGui FlyTextGui {get; private set; }
    }
}
