using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using actionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;
using Action = Lumina.Excel.GeneratedSheets.Action;
using System.Net.Http;
using System.Numerics;


namespace SammiPlugin;

public sealed unsafe class Hooks : IDisposable
{
    private IDataManager dataManager => Service.DataManager;
    public delegate bool UseActionLocationDelegate(ActionManager* manager, actionType actionType, uint actionId, ulong targetId, Vector3* location, uint extraParam);
    public Hook<UseActionLocationDelegate> UseActionLocationHook = null!;
    Plugin plugin;
    Configuration Configuration;

    public Hooks(Plugin plugin)
    {
        UseActionLocationHook = Service.GameInteropProvider.HookFromAddress<UseActionLocationDelegate>(ActionManager.MemberFunctionPointers.UseActionLocation, UseActionLocationDetour);
        UseActionLocationHook.Enable();
        Configuration = plugin.Configuration;
        this.plugin = plugin;
    } 

    public void Dispose()
    {
        UseActionLocationHook.Dispose();
    }

    public bool UseActionLocationDetour(ActionManager* manager, actionType at, uint actionId, ulong targetId, Vector3* location, uint extraParam)
    {
        bool ret = UseActionLocationHook.Original(manager, at, actionId, targetId, location, extraParam);
        if (Configuration.actionUpdateEnable)
        {
            if (at != ActionType.Action)
                return ret;
            //if (!Service.Condition[ConditionFlag.InCombat])
            //    return ret;
            string values = "{\n\"trigger\":\"" + this.plugin.actionWebhookTrigger +
                "\",\n\"actionType\":\"" + at +
                "\",\n\"actionID\":\"" + actionId +
                "\",\n\"actionName\":\"" + Service.DataManager.Excel.GetSheet<Action>()!.GetRow(actionId)!.Name.RawString + "\"\n}";
            var content = new StringContent(values);
            Service.PluginLog.Debug(values);
            //Longer timeout duration since each action is only sent once, need to make sure it doesn't drop
            //Updates may arrive out of order if timeout duration is too long
            Sammi.sendWebhook(Configuration.address, Configuration.password, content, 1000, Configuration.debug);
        }
        return ret;
    }
}
