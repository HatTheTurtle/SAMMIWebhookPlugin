using System;
using System.Net.Http;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace SammiPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;


    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        //GoatImagePath = goatImagePath;
        Plugin = plugin;
    }

    public void Dispose() { }

    public override async void Draw()
    {
        ImGui.Text($"The random config bool is {Plugin.Configuration.webhookEnable}");
        ImGui.InputText("SAMMI API Port", ref Plugin.Configuration.Port, 5);
        if (ImGui.Button("Config"))
        {
            Plugin.ToggleConfigUI();
        }
        if (ImGui.Button("Connect"))
        {
            Plugin.ToggleDeckStatus();
        }

        if (ImGui.Button("Trigger Button"))
        {
            Plugin.TriggerButton();
        }

        ImGui.Spacing();

    }
}
