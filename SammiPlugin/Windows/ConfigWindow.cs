using System;
using System.Net.Http;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SammiPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("SAMMI Plugin###psammi")
    {
        Flags = ImGuiWindowFlags.NoScrollbar;
        Size = new Vector2(300, 200);
        Configuration = plugin.Configuration;
        System.Net.ServicePointManager.Expect100Continue = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        ImGui.InputText("SAMMI API Port", ref Configuration.Port, 5);
        if (ImGui.Button("Test Connection (Popup Notification)"))
        {
            string values = "{\n\"request\": \"popupMessage\",\n\"message\": \"FFXIV SAMMI Plugin is working!\"\n}";
            var content = new StringContent(values);
            try
            {
                Sammi.sendAPI("http://127.0.0.1:" + Configuration.Port, content, 100000, true);
                Service.PluginLog.Debug(values);
            }
            catch (Exception e)
            {
                Service.PluginLog.Debug(e, "error");
                Service.PluginLog.Debug(values);
            }
        }
        // can't ref a property, so use a local copy
        var charUpdateValue = Configuration.charUpdateEnable;
        if (ImGui.Checkbox("Enable xiv_charUpdate", ref charUpdateValue))
        {
            Configuration.charUpdateEnable = charUpdateValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var flyTextUpdateValue = Configuration.flyTextEnable;
        if (ImGui.Checkbox("Enable xiv_flyTextUpdate", ref flyTextUpdateValue))
        {
            Configuration.flyTextEnable = flyTextUpdateValue;
            Configuration.Save();
        }
        var actionUpdateValue = Configuration.actionUpdateEnable;
        if (ImGui.Checkbox("Enable xiv_actionUpdate", ref actionUpdateValue))
        {
            Configuration.actionUpdateEnable = actionUpdateValue;
            Configuration.Save();
        }
    }
}
