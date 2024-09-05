using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Text.SeStringHandling;
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
    private MainWindow MainWindow { get; init; }
    private IFramework Framework { get; init; }

    private static HttpClient PluginClient = new HttpClient(new HttpClientHandler
    {
        UseProxy = false
    });
    private string? uri;

    private string charWebhookTrigger = "xiv_charUpdate";
    private string flyTextWebhookTrigger = "xiv_flyTextUpdate";

    //private IPlayerCharacter? player = Service.ClientState.LocalPlayer;
    
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        //var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        PluginInterface.Create<Service>();
        uri = "http://127.0.0.1:9450";
        //PluginClient.Timeout = TimeSpan.FromMilliseconds(100);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        Service.Condition.ConditionChange += OnConditionChange;
        Service.Framework.Update += OnFrameworkUpdate;
        Service.FlyTextGui.FlyTextCreated += onFlyTextCreated;
        //Service.ClientState.Login += OnLogin;
        //Service.ClientState.Logout += OnLogout;
        //Service.ActionManager.Delegates.UseAction += OnUseAction;

    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        Service.Condition.ConditionChange -= OnConditionChange;
        Service.Framework.Update -= OnFrameworkUpdate;
        Service.FlyTextGui.FlyTextCreated -= onFlyTextCreated;

        ConfigWindow.Dispose();
        MainWindow.Dispose(); 


        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    public async void GetDeckStatus()
    {
        try
        {
            using HttpResponseMessage response = await PluginClient.GetAsync(uri + "?request=getDeckStatus&deckID=20240422201033548168059");

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Service.PluginLog.Debug(jsonResponse);
        }
        catch (Exception e) 
        { 
            Service.PluginLog.Debug(e, "error");
        }
    }

    public async void ToggleDeckStatus()
    {
        string values = "{\n\"request\": \"changeDeckStatus\",\n\"deckID\": \"20240422201033548168059\",\n\"status\": \"2\"\n}";
        var content = new StringContent(values);

        //using StringContent jsonContent = new(
        //JsonSerializer.Serialize("{request: changeDeckStatus, deckID: 20240422201033548168059,status: 2 }"));

        try
        {
            using var response = await PluginClient.PostAsync(uri, content);
            response.EnsureSuccessStatusCode();
            GetDeckStatus();
        }
        catch (Exception e)
        {
            Service.PluginLog.Debug(e, "error", values);
            Service.PluginLog.Debug(values);
        };
    }

    public async void TriggerButton()
    {
        string values = "{\n\"request\": \"triggerButton\",\n\"buttonID\": \"DeathTrigger\"\n}";
        var content = new StringContent(values);
        try
        {
            using var response = await PluginClient.PostAsync(uri + "/webhook", content);
            Service.PluginLog.Debug(values);
            //Service.PluginLog.Debug($$"""{\n\"trigger\":\"{{charWebhookTrigger}}\",\n\"name\":\"{{Service.ClientState.LocalPlayer.Name}}\"\n}""");
        }
        catch (Exception e)
        {
            Service.PluginLog.Debug(e, "error");
            Service.PluginLog.Debug(values);
        }
    }

    private void OnConditionChange (ConditionFlag flag, bool value)
    {
        try
        {
            if (Service.ClientState.LocalPlayer.IsDead)
            {
                //TriggerButton();
            }
        }
        catch (Exception e)
        {
            Service.PluginLog.Debug (e, "error");
        }
    }

    public void OnFrameworkUpdate(IFramework Framework)
    {
        if (Service.ClientState.LocalPlayer != null && Configuration.charUpdateEnable)
        {
            //Service.PluginLog.Debug("working");
            //Access data in main thread instead of in an async method
            string values = "{\n\"trigger\":\"" + charWebhookTrigger + "\",\n\"name\":\"" + Service.ClientState.LocalPlayer.Name + "\",\n\"hp\":\"" + Service.ClientState.LocalPlayer.CurrentHp + "\"\n}";
            var content = new StringContent(values);
            sendCharacterUpdate(content);
        }
    }

    private void onFlyTextCreated (ref FlyTextKind kind, ref int val1, ref int val2, ref SeString text1, ref SeString text2, ref uint color, ref uint icon, ref uint damageTypeIcon, ref float yOffset, ref bool handled)
    {
        if (Service.ClientState.LocalPlayer!= null && Configuration.flyTextEnable)
        {
            //PluginClient.Timeout = TimeSpan.FromMilliseconds(200);
            string values = "{\n\"trigger\":\"" + flyTextWebhookTrigger + "\",\n\"kind\":\"" + kind + "\",\n\"val1\":\"" + val1 + "\",\n\"val2\":\"" + val2 + "\",\n\"text1\":\"" + text1 + "\",\n\"text2\":\"" + text2 + "\"\n}";
            //string values = "{\n\"trigger\":\"" + flyTextWebhookTrigger + "\",\n\"kind\":\"" + kind + "\",\n\"text1\":\""+text1+"\"\n}";

            var content = new StringContent(values);
            Service.PluginLog.Debug(values);
            sendWebhook(content);
            //PluginClient.Timeout = TimeSpan.FromMilliseconds(100);
        }
    }

    public async void sendWebhook (StringContent content)
    {
        try
        {
            using var response = await PluginClient.PostAsync(uri + "/webhook", content);

            //Service.PluginLog.Debug("JSON sent");
        }
        catch (Exception e)
        {
            Service.PluginLog.Debug(e, "error");
            //Service.PluginLog.Debug();
        }
    }

    //Separate method for sending character update webhooks to use a different timeout length
    //Maybe a better way to do this with just a single method, unfortunately I am dumb
    public async void sendCharacterUpdate (StringContent content)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));
            using var response = await PluginClient.PostAsync(uri + "/webhook", content, cts.Token);

            //Service.PluginLog.Debug("JSON sent");
        }
        catch (Exception e)
        {
            //Service.PluginLog.Debug(e, "error");
            //Service.PluginLog.Debug();
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
