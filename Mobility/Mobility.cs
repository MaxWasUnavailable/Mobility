using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace Mobility;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Mobility : BaseUnityPlugin
{
    internal new static ManualLogSource? Logger;
    internal static Mobility? Instance { get; private set; }
    private static Harmony? Harmony { get; set; }
    private static bool IsPatched { get; set; }

    public static ConfigEntry<bool>? EnableStaminaSystem { get; private set; }
    public static ConfigEntry<bool>? EnableStaminaBar { get; private set; }

    private void Awake()
    {
        Instance = this;

        Logger = base.Logger;

        Harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        EnableStaminaSystem = Config.Bind("General", "UseStaminaSystem", true,
            "Whether to use the stamina system. If disabled, you will be able to sprint infinitely.");
        EnableStaminaBar = Config.Bind("General", "UseStaminaBar", true,
            "Whether to show the stamina bar. If disabled, you will not see the stamina bar.");

        PatchAll();

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    public void PatchAll()
    {
        if (IsPatched) return;
        Harmony!.PatchAll();
        IsPatched = true;
    }

    public void UnpatchAll()
    {
        if (!IsPatched) return;
        Harmony.UnpatchAll();
        IsPatched = false;
    }
}