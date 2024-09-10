using System;
using System.Collections.Generic;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using actionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;
using Action = Lumina.Excel.GeneratedSheets.Action;
using System.Linq;
using System.Net.Http;


namespace SammiPlugin;

public sealed unsafe class Hooks : IDisposable
{
    private IDataManager dataManager => Service.DataManager;
    public delegate bool UseActionDelegate(ActionManager* manager, actionType actionType, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted);
    public Hook<UseActionDelegate> UseActionHook = null!;
    private Dictionary<uint, Action> CachedActions;
    Configuration Configuration;

    public Hooks(Plugin plugin)
    {
        UseActionHook = Service.GameInteropProvider.HookFromAddress<UseActionDelegate>(ActionManager.MemberFunctionPointers.UseAction, UseActionDetour);
        CachedActions = new Dictionary<uint, Action>();
        CacheActions();
        UseActionHook.Enable();
        Configuration = plugin.Configuration;
    } 

    public void Dispose()
    {
        UseActionHook.Dispose();
    }
    
    public bool UseActionDetour(ActionManager* manager, actionType at, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* optOutAreaTargeted)
    {
        bool ret = UseActionHook.Original(manager, at, actionId, targetId, extraParam, mode, comboRouteId, optOutAreaTargeted);
        if (Configuration.actionUpdateEnable)
        {
            if (at != ActionType.Action)
                return ret;
            //if (!Service.Condition[ConditionFlag.InCombat])
            //    return ret;
            //TODO: debug Unhandled Native Exception from CachedActions[actionId]
            //and add \"name\":\"" + action.Name + "\"\n}";
            //var action = CachedActions[actionId];
            string values = "{\n\"trigger\":\"" + "xiv_actionUpdate" + "\",\n\"actionType\":\"" + at + "\",\n\"actionID\":\"" + actionId + "\"\n}";
            var content = new StringContent(values);
            Service.PluginLog.Debug(values);
            //Longer timeout duration since each action is only sent once, need to make sure it doesn't drop
            //Updates may arrive out of order if timeout duration is too long
            Sammi.sendWebhook("http://127.0.0.1:9450", content, 1000, true);
        }
        return ret;
    }

    private void CacheActions()
    {
        CachedActions = new();
        var actions = Service.DataManager.GetExcelSheet<Action>()!.
            Where(a => !a.IsPvP && a.ClassJob.Value?.Unknown6 > 0 && a.IsPlayerAction && (a.ActionCategory.Row == 4 || a.Recast100ms > 100)).ToList();
        foreach (var action in actions)
        {
            CachedActions[action.RowId] = action;
        }
        var roleActions = Service.DataManager.GetExcelSheet<Action>()!.Where(a => a.IsRoleAction && a.ClassJobLevel != 0).ToList();
        foreach (var roleAction in roleActions)
        {
            CachedActions[roleAction.RowId] = roleAction;
        }
    }
}
