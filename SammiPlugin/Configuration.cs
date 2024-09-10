using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Net.Http;

namespace SammiPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool charUpdateEnable { get; set; } = false;
    public bool flyTextEnable { get; set; } = false;
    public bool actionUpdateEnable { get; set; } = false;
    public string Port = "9450";

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
