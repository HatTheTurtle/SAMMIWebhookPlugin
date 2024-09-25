using Dalamud.Configuration;
using System;

namespace SammiPlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool charUpdateEnable { get; set; } = false;
    public bool flyTextEnable { get; set; } = false;
    public bool actionUpdateEnable { get; set; } = false;
    //SAMMI Webhook URI, default port 9450
    public string address = "http://127.0.0.1:9450";
    public string password = "";
    public bool debug = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
