using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using SammiPlugin.Windows;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Encodings;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Game.Gui.FlyText;
using System.Threading;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;
using System.Transactions;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;
using System.Linq;

namespace SammiPlugin;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/psammi";

    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("SammiPlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private IFramework Framework { get; init; }

    private static HttpClient PluginClient = new HttpClient(new HttpClientHandler
    {
        UseProxy = false
    });
    private string? uri;
    private string charWebhookTrigger = "xiv_charUpdate";
    private string flyTextWebhookTrigger = "xiv_flyTextUpdate";
    private string actionWebhookTrigger = "xiv_actionUpdate";

    public Hooks Hooks { get; }
    //Array of FlyTextKinds that should be acted upon/sent to SAMMI
    //TODO: Add to config window to let users decide which kinds to listen for
    FlyTextKind[] flyText;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        PluginInterface.Create<Service>();
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Sends various types of game data to SAMMI via webhook. /psammi to configure."
        });

        //SAMMI Webhook URI, default port 9450
        uri = "http://127.0.0.1:" + Configuration.Port;
        System.Net.ServicePointManager.Expect100Continue = false;

        Hooks = new(this);
        flyText = [FlyTextKind.Buff, FlyTextKind.BuffFading, FlyTextKind.Damage, FlyTextKind.DamageCrit, FlyTextKind.DamageCritDh, FlyTextKind.DamageDh];

        Service.ClientState.Login += OnLogin;
        Service.ClientState.Logout += OnLogout;
        Service.Condition.ConditionChange += OnConditionChange;
        Service.Framework.Update += OnFrameworkUpdate;
        Service.FlyTextGui.FlyTextCreated += OnFlyTextCreated;
    }

    public void OnLogin()
    {

    }

    public void OnLogout()
    {

    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows(); 
        ConfigWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);

        Service.ClientState.Login -= OnLogin;
        Service.ClientState.Logout -= OnLogout; 
        Service.Condition.ConditionChange -= OnConditionChange;
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.FlyTextGui.FlyTextCreated -= OnFlyTextCreated;

        Hooks.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleConfigUI();
    }

    private void OnConditionChange (ConditionFlag flag, bool value)
    {
        //TODO: Send condition info to SAMMI
        //Might be unreliable, e.g., "Unconscious" should mean dead but is
        //used for a bunch of different stuff too
    }

    public void OnFrameworkUpdate(IFramework Framework)
    {
        if (Service.ClientState.LocalPlayer != null && Configuration.charUpdateEnable)
        {
            //Service.PluginLog.Debug("working");
            //Access data in main thread instead of in an async method
            string values = "{\n\"trigger\":\"" + charWebhookTrigger + "\",\n\"name\":\"" + Service.ClientState.LocalPlayer.Name + "\",\n\"hp\":\"" + Service.ClientState.LocalPlayer.CurrentHp + "\"\n}";
            var content = new StringContent(values);
            //Short timeout duration, character data is constantly changing so it's ok if some updates get dropped
            //Also prevents updates from being received out of order
            Sammi.sendWebhook(uri!, content, 100, false);
        }
    }

    private void OnFlyTextCreated (ref FlyTextKind kind, ref int val1, ref int val2, ref SeString text1, ref SeString text2, ref uint color, ref uint icon, ref uint damageTypeIcon, ref float yOffset, ref bool handled)
    {
        if (Service.ClientState.LocalPlayer!= null && Configuration.flyTextEnable)
        {
            if (flyText.Contains(kind))
            {
                //Consider removing some fields since some seem to be unused by the game?
                //string values = "{\n\"trigger\":\"" + flyTextWebhookTrigger + "\",\n\"kind\":\"" + kind + "\",\n\"text1\":\""+text1+"\"\n}";
                string values = "{\n\"trigger\":\"" + flyTextWebhookTrigger + "\",\n\"kind\":\"" + kind + "\",\n\"val1\":\"" + val1 + "\",\n\"val2\":\"" + val2 + "\",\n\"text1\":\"" + text1 + "\",\n\"text2\":\"" + text2 + "\"\n}";
                var content = new StringContent(values);
                Service.PluginLog.Debug(values);
                //Longer timeout duration since each action is only sent once, need to make sure it doesn't drop
                //Updates may arrive out of order if timeout duration is too long
                Sammi.sendWebhook(uri, content, 3000, true);
            }
        }
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
