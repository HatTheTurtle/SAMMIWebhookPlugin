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
    public ConfigWindow(Plugin plugin) : base("SAMMI Plugin")
    {
        Flags = ImGuiWindowFlags.NoScrollbar;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(380, 250)
        };
        Configuration = plugin.Configuration;
        System.Net.ServicePointManager.Expect100Continue = false;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
    }

    public override void Draw()
    {
        //Might be unnecessary, not sure of cases where the default isn't used
        //Port can be changed but not recommended by SAMMI docs
        if (ImGui.InputText("SAMMI API Address", ref Configuration.address, 128))
        {
            Configuration.Save();
        };
        if (ImGui.InputText("Password (optional)", ref Configuration.password, 512, ImGuiInputTextFlags.Password))
        {
            Configuration.Save();
        };
        if (ImGui.Button("Test Connection (Popup Notification)"))
        {
            string values = "{\n\"request\": \"popupMessage\",\n\"message\": \"FFXIV SAMMI Plugin is working!\"\n}";
            try
            {
                Sammi.sendAPI(Configuration.address, Configuration.password, values, 100000, Configuration.debug);
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
        var conditionUpdateValue = Configuration.conditionUpdateEnable;
        if (ImGui.Checkbox("Enable xiv_conditionUpdate", ref conditionUpdateValue))
        {
            Configuration.conditionUpdateEnable = conditionUpdateValue;
            Configuration.Save();
        }
        if (ImGui.Checkbox("Enable error/debug messages", ref Configuration.debug))
        {
            Configuration.Save();
        }
    }
}
