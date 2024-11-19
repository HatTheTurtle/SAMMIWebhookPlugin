using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
using SammiPlugin.Windows;
using System.Net.Http;
using System;
using Dalamud.Game.Gui.FlyText;
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
    public string charWebhookTrigger = "xiv_charUpdate";
    public string flyTextWebhookTrigger = "xiv_flyTextUpdate";
    public string actionWebhookTrigger = "xiv_actionUpdate";
    public string conditionWebhookTrigger = "xiv_conditionUpdate";

    uint prevHp = 0, prevMaxHp = 0, prevMp = 0, prevMaxMp = 0;

    public Hooks Hooks { get; }
    //Array of FlyTextKinds that should be acted upon/sent to SAMMI
    //TODO?: Add to config window to let users decide which kinds to listen for
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

        System.Net.ServicePointManager.Expect100Continue = false;

        Hooks = new(this);
        flyText = [FlyTextKind.Buff, FlyTextKind.BuffFading, FlyTextKind.Damage, FlyTextKind.DamageCrit, FlyTextKind.DamageCritDh, FlyTextKind.DamageDh,
        FlyTextKind.Debuff, FlyTextKind.DebuffFading];

        Service.Condition.ConditionChange += OnConditionChange;
        Service.Framework.Update += OnFrameworkUpdate;
        Service.FlyTextGui.FlyTextCreated += OnFlyTextCreated;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows(); 
        ConfigWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);

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
        string values = "{\n\"trigger\":\"" + conditionWebhookTrigger +
            "\",\n\"condition\":\"" + flag + "\"\n}";
        Sammi.sendWebhook(Configuration.address, Configuration.password, values, 300, Configuration.debug);
    }

    public void OnFrameworkUpdate(IFramework Framework)
    {
        if (Configuration.charUpdateEnable && Service.ClientState.LocalPlayer != null )           
        {
            //Only send a new webhook if the data has changed
            if (Service.ClientState.LocalPlayer.CurrentHp != prevHp ||
                Service.ClientState.LocalPlayer.MaxHp != prevMaxHp ||
                Service.ClientState.LocalPlayer.CurrentMp != prevMp ||
                Service.ClientState.LocalPlayer.MaxMp != prevMaxMp)
            {
                prevHp = Service.ClientState.LocalPlayer.CurrentHp;
                prevMaxHp = Service.ClientState.LocalPlayer.MaxHp;
                prevMp = Service.ClientState.LocalPlayer.CurrentMp;
                prevMaxMp = Service.ClientState.LocalPlayer.MaxMp;

                //Access data in main thread instead of in an async method
                string values = "{\n\"trigger\":\"" + charWebhookTrigger +
                    "\",\n\"name\":\"" + Service.ClientState.LocalPlayer.Name +
                    "\",\n\"maxHp\":\"" + Service.ClientState.LocalPlayer.MaxHp +
                    "\",\n\"hp\":\"" + Service.ClientState.LocalPlayer.CurrentHp +
                    "\",\n\"maxMp\":\"" + Service.ClientState.LocalPlayer.MaxMp +
                    "\",\n\"mp\":\"" + Service.ClientState.LocalPlayer.CurrentMp + "\"\n}";
                //Short timeout duration, character data is constantly changing so it's ok if some updates get dropped
                //Also prevents updates from being received out of order
                Sammi.sendWebhook(Configuration.address, Configuration.password, values, 300, Configuration.debug);
            }
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
                string values = "{\n\"trigger\":\"" + flyTextWebhookTrigger + 
                    "\",\n\"kind\":\"" + kind + 
                    "\",\n\"val1\":\"" + val1 + 
                    "\",\n\"val2\":\"" + val2 + 
                    "\",\n\"text1\":\"" + text1 + 
                    "\",\n\"text2\":\"" + text2 + "\"\n}";

                //Longer timeout duration since each action is only sent once, need to make sure it doesn't drop
                //Updates may arrive out of order if timeout duration is too long
                Sammi.sendWebhook(Configuration.address, Configuration.password, values, 1000, Configuration.debug);
            }
        }
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
