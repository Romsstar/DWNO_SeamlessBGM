using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace SeamlessBGM;

[BepInPlugin(GUID, PluginName, PluginVersion)]
[BepInProcess("Digimon World Next Order.exe")]
public class Plugin : BasePlugin
{
    internal const string GUID = "SeamlessBGM";
    internal const string PluginName = "SeamlessBGM";
    internal const string PluginVersion = "1.0.0";
    public static ConfigEntry<bool> suppressChaseBGM { get; private set; }

    public override void Load()
    {
        suppressChaseBGM = Config.Bind("General",
            "SuppressChaseBGM",
            true,
            "Set to true to disable the chaseBGM (default setting). Still plays the notification.");
        Harmony.CreateAndPatchAll(typeof(Plugin));
    }

    public static bool is_changing_area = false;

    [HarmonyPatch(typeof(MainGameField), "PlayFieldBgm")]
    [HarmonyPrefix]
    public static bool PlayFieldBgm_Prefix(bool _bSameSkip, bool _bChase, CriSoundManager __instance)
    {
        // Accessing the config setting
        if (_bChase && suppressChaseBGM.Value || is_changing_area)
        {
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(MainGameManager._LoadTask_d__3), "MoveNext")]
    [HarmonyPrefix]
    public static void MainGameManager__LoadTask_d__3_Prefix(MainGameManager._LoadTask_d__3 __instance)
    {
        switch (__instance.__1__state)
        {
            case 0:
                is_changing_area = __instance.__4__this.mapNo == __instance.__4__this.nextMapNo;
                break;
            default:
                break;
        }
    }

    [HarmonyPatch(typeof(CriSoundManager), "MainBgmFade")]
    [HarmonyPrefix]
    public static bool CriSoundManager_MainBgmFade_Prefix()
    {
        if (is_changing_area)
        {
            is_changing_area = false;
            return false;
        }

        return true;
    }
}