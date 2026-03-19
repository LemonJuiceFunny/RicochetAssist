using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using HarmonyLib;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("RicochetAssist")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("RicochetAssist")]
[assembly: AssemblyCopyright("Copyright © 2024")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("128f5857-fe39-43b0-9bb9-71d258a32957")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.7.2", FrameworkDisplayName = ".NET Framework 4.7.2")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("1.0.0.0")]
[module: UnverifiableCode]
namespace RicochetAssist;

[BepInPlugin("ironfarm.uk.ricomorty", "RicochetAssist", "1.0.2")]
[BepInDependency(/*Could not decode attribute arguments.*/)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harm;

    public void Start()
    {
        Debug.Log((object)"We did dang dun it reddit");
        harm = new Harmony("ironfarm.uk.ricomorty");
        harm.PatchAll(typeof(RicochetMorty));
        ConfigManager.Setup();
    }
}

public static class ConfigManager
{
    [Serializable]
    [CompilerGenerated]
    private sealed class <>c
    {
        public static readonly <>c <>9 = new <>c();

        public static BoolValueChangeEventDelegate <>9__9_0;
        public static BoolValueChangeEventDelegate <>9__9_1;
        public static FloatValueChangeEventDelegate <>9__9_2;
        public static BoolValueChangeEventDelegate <>9__9_3;
        public static FloatValueChangeEventDelegate <>9__9_4;
        public static IntValueChangeEventDelegate <>9__9_5;
        public static BoolValueChangeEventDelegate <>9__9_6;
        public static IntValueChangeEventDelegate <>9__9_7;

        internal void <Setup>b__9_0(BoolValueChangeEvent e)
        {
            RicochetMorty.shouldVanillaAimAssist = e.value;
        }

        internal void <Setup>b__9_1(BoolValueChangeEvent e)
        {
            ((ConfigField)RicochetFOV).hidden = !e.value;
            ((ConfigField)TargetCoins).hidden = !e.value;
            RicochetMorty.shouldAimAssist = e.value;
        }

        internal void <Setup>b__9_2(FloatValueChangeEvent e)
        {
            RicochetMorty.ricFOV = e.value;
        }

        internal void <Setup>b__9_3(BoolValueChangeEvent e)
        {
            RicochetMorty.shouldTargetCoin = e.value;
        }

        internal void <Setup>b__9_4(FloatValueChangeEvent e)
        {
            RicochetMorty.ricTimer = e.value;
        }

        internal void <Setup>b__9_5(IntValueChangeEvent e)
        {
            RicochetMorty.ricBounceAmount = e.value;
        }

        internal void <Setup>b__9_6(BoolValueChangeEvent e)
        {
            ((ConfigField)RailcannonExtraRicochet).hidden = !e.value;
            RicochetMorty.shouldRailBounce = e.value;
        }

        internal void <Setup>b__9_7(IntValueChangeEvent e)
        {
            RicochetMorty.railBounceAmount = e.value;
        }
    }

    public static PluginConfigurator config;
    public static BoolField AimAssistEnabled;
    public static BoolField VanillaAimAssistEnabled;
    public static FloatField RicochetFOV;
    public static BoolField TargetCoins;
    public static FloatField RicochetTimer;
    public static IntField SharpshooterExtraRicochet;
    public static BoolField RailcannonRicochetEnable;
    public static IntField RailcannonExtraRicochet;

    public static void Setup()
    {
        config = PluginConfigurator.Create("RicochetAssist", "ironfarm.uk.ricomorty");
        new ConfigHeader(config.rootPanel, "<color=red>Does not confict with modded aim assist</color>", 15);
        VanillaAimAssistEnabled = new BoolField(config.rootPanel, "Enable Vanilla Aim Assist", "bool.vanillaaimassist", false);
        AimAssistEnabled = new BoolField(config.rootPanel, "Enable Aim Assist", "bool.aimassist", true);
        RicochetFOV = new FloatField(config.rootPanel, "Ricochet FOV", "float.ricochetfov", 180f, 0f, 360f);
        TargetCoins = new BoolField(config.rootPanel, "Target Coins", "bool.targetcoins", true);
        new ConfigHeader(config.rootPanel, "<color=red>At 0, shots will ricochet while paused!</color>", 15);
        RicochetTimer = new FloatField(config.rootPanel, "Ricochet Timer", "float.ricochettimer", 0.1f, 0f, 100f);
        SharpshooterExtraRicochet = new IntField(config.rootPanel, "Extra Sharpshooter Ricochet", "int.sharpshooterextraricochet", 5, 0, 100000000);
        RailcannonRicochetEnable = new BoolField(config.rootPanel, "Enable Railcannon Ricochet", "bool.railcannonricochet", false);
        RailcannonExtraRicochet = new IntField(config.rootPanel, "Extra Railcannon Ricochet", "int.railcannonextraricochet", 5, 0, 100000000);

        BoolField vanillaAimAssistEnabled = VanillaAimAssistEnabled;
        object obj = <>c.<>9__9_0;
        if (obj == null)
        {
            BoolValueChangeEventDelegate val = delegate(BoolValueChangeEvent e) { RicochetMorty.shouldVanillaAimAssist = e.value; };
            <>c.<>9__9_0 = val;
            obj = (object)val;
        }
        vanillaAimAssistEnabled.onValueChange += (BoolValueChangeEventDelegate)obj;

        BoolField aimAssistEnabled = AimAssistEnabled;
        object obj2 = <>c.<>9__9_1;
        if (obj2 == null)
        {
            BoolValueChangeEventDelegate val2 = delegate(BoolValueChangeEvent e)
            {
                ((ConfigField)RicochetFOV).hidden = !e.value;
                ((ConfigField)TargetCoins).hidden = !e.value;
                RicochetMorty.shouldAimAssist = e.value;
            };
            <>c.<>9__9_1 = val2;
            obj2 = (object)val2;
        }
        aimAssistEnabled.onValueChange += (BoolValueChangeEventDelegate)obj2;

        FloatField ricochetFOV = RicochetFOV;
        object obj3 = <>c.<>9__9_2;
        if (obj3 == null)
        {
            FloatValueChangeEventDelegate val3 = delegate(FloatValueChangeEvent e) { RicochetMorty.ricFOV = e.value; };
            <>c.<>9__9_2 = val3;
            obj3 = (object)val3;
        }
        ricochetFOV.onValueChange += (FloatValueChangeEventDelegate)obj3;

        BoolField targetCoins = TargetCoins;
        object obj4 = <>c.<>9__9_3;
        if (obj4 == null)
        {
            BoolValueChangeEventDelegate val4 = delegate(BoolValueChangeEvent e) { RicochetMorty.shouldTargetCoin = e.value; };
            <>c.<>9__9_3 = val4;
            obj4 = (object)val4;
        }
        targetCoins.onValueChange += (BoolValueChangeEventDelegate)obj4;

        FloatField ricochetTimer = RicochetTimer;
        object obj5 = <>c.<>9__9_4;
        if (obj5 == null)
        {
            FloatValueChangeEventDelegate val5 = delegate(FloatValueChangeEvent e) { RicochetMorty.ricTimer = e.value; };
            <>c.<>9__9_4 = val5;
            obj5 = (object)val5;
        }
        ricochetTimer.onValueChange += (FloatValueChangeEventDelegate)obj5;

        IntField sharpshooterExtraRicochet = SharpshooterExtraRicochet;
        object obj6 = <>c.<>9__9_5;
        if (obj6 == null)
        {
            IntValueChangeEventDelegate val6 = delegate(IntValueChangeEvent e) { RicochetMorty.ricBounceAmount = e.value; };
            <>c.<>9__9_5 = val6;
            obj6 = (object)val6;
        }
        sharpshooterExtraRicochet.onValueChange += (IntValueChangeEventDelegate)obj6;

        BoolField railcannonRicochetEnable = RailcannonRicochetEnable;
        object obj7 = <>c.<>9__9_6;
        if (obj7 == null)
        {
            BoolValueChangeEventDelegate val7 = delegate(BoolValueChangeEvent e)
            {
                ((ConfigField)RailcannonExtraRicochet).hidden = !e.value;
                RicochetMorty.shouldRailBounce = e.value;
            };
            <>c.<>9__9_6 = val7;
            obj7 = (object)val7;
        }
        railcannonRicochetEnable.onValueChange += (BoolValueChangeEventDelegate)obj7;

        IntField railcannonExtraRicochet = RailcannonExtraRicochet;
        object obj8 = <>c.<>9__9_7;
        if (obj8 == null)
        {
            IntValueChangeEventDelegate val8 = delegate(IntValueChangeEvent e) { RicochetMorty.railBounceAmount = e.value; };
            <>c.<>9__9_7 = val8;
            obj8 = (object)val8;
        }
        railcannonExtraRicochet.onValueChange += (IntValueChangeEventDelegate)obj8;

        VanillaAimAssistEnabled.TriggerValueChangeEvent();
        AimAssistEnabled.TriggerValueChangeEvent();
        RicochetFOV.TriggerValueChangeEvent();
        TargetCoins.TriggerValueChangeEvent();
        RicochetTimer.TriggerValueChangeEvent();
        SharpshooterExtraRicochet.TriggerValueChangeEvent();
        RailcannonRicochetEnable.TriggerValueChangeEvent();
        RailcannonExtraRicochet.TriggerValueChangeEvent();

        string path = Utils.ModDir();
        string text = Path.Combine(Path.Combine(path, "Data"), "icon.png");
        config.SetIconWithURL("file://" + text);
    }
}

public static class Utils
{
    public static bool WithinFOV(Vector3 main, Vector3 target, float fov)
    {
        float num = Mathf.Acos(Vector3.Dot(((Vector3)(ref main)).normalized, ((Vector3)(ref target)).normalized));
        float num2 = fov / 2f % 360f * ((float)Math.PI / 180f);
        return num <= num2 || num >= (float)Math.PI * 2f - num2;
    }

    public static string ModDir()
    {
        return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}

internal class PluginInfo
{
    public const string Name = "RicochetAssist";
    public const string GUID = "ironfarm.uk.ricomorty";
    public const string Version = "1.0.2";
}

public static class RicochetMorty
{
    private static int ricochetCount = 0;
    private static int damCount = 0;

    public static float ricFOV = 180f;
    public static float ricTimer = 0.1f;
    public static bool shouldAimAssist = true;
    public static bool shouldVanillaAimAssist = false;
    public static bool shouldRailBounce = false;
    public static bool shouldTargetCoin = true;
    public static int railBounceAmount = 5;
    public static int ricBounceAmount = 5;

    private static bool eidCheck(EnemyIdentifier eid)
    {
        return (Object)(object)eid != (Object)null && !eid.dead && Object.op_Implicit((Object)(object)((Component)eid).gameObject) && !eid.blessed;
    }

    [HarmonyPatch(typeof(RevolverBeam), "RicochetAimAssist")]
    [HarmonyPrefix]
    public static bool TargetedHarassmentCampaign(RevolverBeam __instance)
    {
        if (!shouldAimAssist)
        {
            return shouldVanillaAimAssist;
        }
        float num = float.PositiveInfinity;
        GameObject val = null;
        Transform val2 = null;
        Vector3 val3 = Vector3.zero;
        EnemyIdentifier val4 = null;
        RevolverBeam revb = __instance;
        Vector3 position = ((Component)revb).transform.position;
        TargetType[] array = new TargetType[5];
        RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
        Vision val5 = new Vision(position, new VisionTypeFilter((TargetType[])(object)array));
        MonoSingleton<PortalManagerV2>.Instance.TargetTracker.RegisterVision(val5, ((MonoBehaviour)revb).destroyCancellationToken);
        MonoSingleton<PortalManagerV2>.Instance.TargetTracker.UpdateData();
        VisionQuery val6 = new VisionQuery("RicPlayer", (TargetPredicate)((TargetDataRef t) => (int)t.target.Type == 1 && !TargetDataExtensions.IsObstructed(t, ((Component)revb).transform.position, LayerMaskDefaults.Get((LMD)1), false)));
        VisionQuery val7 = new VisionQuery("RicEnemy", (TargetPredicate)((TargetDataRef t) => (int)t.target.Type == 2 && ((int)t.target.EID.enemyType == 42 || !TargetDataExtensions.IsObstructed(t, ((Component)revb).transform.position, LayerMaskDefaults.Get((LMD)1), false))));
        VisionQuery val8 = new VisionQuery("RicCoin", (TargetPredicate)((TargetDataRef t) => (int)t.target.Type == 3 && !TargetDataExtensions.IsObstructed(t, ((Component)revb).transform.position, LayerMaskDefaults.Get((LMD)1), false)));
        VisionQuery val9 = new VisionQuery("RicExplosive", (TargetPredicate)((TargetDataRef t) => (int)t.target.Type == 4 && !TargetDataExtensions.IsObstructed(t, ((Component)revb).transform.position, LayerMaskDefaults.Get((LMD)1), false)));
        val5.UpdateSourcePos(((Component)revb).transform.position);
        TargetDataRef val10 = default(TargetDataRef);
        Vector3 val11;
        if (val5.TrySee(val8, ref val10) && shouldTargetCoin)
        {
            Coin component = val10.target.GameObject.GetComponent<Coin>();
            if ((Object)(object)component != (Object)null && !component.shot && !component.shotByEnemy)
            {
                val2 = ((Component)component).transform;
                val3 = ((Matrix4x4)(ref ((TargetDataRef)(ref val10)).portalMatrix)).MultiplyPoint3x4(val2.position);
                Vector3 forward = ((Component)revb).transform.forward;
                val11 = val3 - ((Component)revb).transform.position;
                if (!Utils.WithinFOV(forward, ((Vector3)(ref val11)).normalized, ricFOV))
                {
                    val2 = null;
                }
            }
        }
        if (val5.TrySee(val9, ref val10))
        {
            val2 = val10.target.GameObject.transform;
            val3 = ((Matrix4x4)(ref ((TargetDataRef)(ref val10)).portalMatrix)).MultiplyPoint3x4(val2.position);
            Vector3 forward2 = ((Component)revb).transform.forward;
            val11 = val3 - ((Component)revb).transform.position;
            if (!Utils.WithinFOV(forward2, ((Vector3)(ref val11)).normalized, ricFOV))
            {
                val2 = null;
            }
        }
        if (val5.TrySee(val7, ref val10) && (Object)(object)val2 == (Object)null)
        {
            val4 = val10.target.EID;
            if ((Object)(object)val4 != (Object)null && !val4.dead && Object.op_Implicit((Object)(object)((Component)val4).gameObject) && !val4.blessed)
            {
                if ((Object)(object)val4.weakPoint != (Object)null && val4.weakPoint.activeInHierarchy)
                {
                    val2 = val4.weakPoint.transform;
                    val3 = ((Matrix4x4)(ref ((TargetDataRef)(ref val10)).portalMatrix)).MultiplyPoint3x4(val2.position);
                    Vector3 forward3 = ((Component)revb).transform.forward;
                    val11 = val3 - ((Component)revb).transform.position;
                    if (!Utils.WithinFOV(forward3, ((Vector3)(ref val11)).normalized, ricFOV))
                    {
                        val2 = null;
                    }
                }
                if ((Object)(object)val2 == (Object)null)
                {
                    EnemyIdentifierIdentifier componentInChildren = ((Component)val4).GetComponentInChildren<EnemyIdentifierIdentifier>();
                    if (Object.op_Implicit((Object)(object)componentInChildren) && Object.op_Implicit((Object)(object)componentInChildren.eid) && (Object)(object)componentInChildren.eid == (Object)(object)val4)
                    {
                        val2 = ((Component)componentInChildren).transform;
                        val3 = ((Matrix4x4)(ref ((TargetDataRef)(ref val10)).portalMatrix)).MultiplyPoint3x4(val2.position);
                        Vector3 forward4 = ((Component)revb).transform.forward;
                        val11 = val3 - ((Component)revb).transform.position;
                        if (!Utils.WithinFOV(forward4, ((Vector3)(ref val11)).normalized, ricFOV))
                        {
                            val2 = null;
                        }
                    }
                    if ((Object)(object)val2 == (Object)null)
                    {
                        val2 = ((Component)val4).transform;
                        val3 = ((Matrix4x4)(ref ((TargetDataRef)(ref val10)).portalMatrix)).MultiplyPoint3x4(val2.position);
                        Vector3 forward5 = ((Component)revb).transform.forward;
                        val11 = val3 - ((Component)revb).transform.position;
                        if (!Utils.WithinFOV(forward5, ((Vector3)(ref val11)).normalized, ricFOV))
                        {
                            val2 = null;
                        }
                    }
                }
            }
        }
        if (Object.op_Implicit((Object)(object)val2))
        {
            Transform transform = ((Component)revb).transform;
            val11 = val3 - ((Component)revb).transform.position;
            transform.forward = ((Vector3)(ref val11)).normalized;
        }
        return shouldVanillaAimAssist;
    }

    [HarmonyPatch(typeof(RevolverBeam), "Start")]
    [HarmonyPrefix]
    public static bool RicochetForAll(RevolverBeam __instance)
    {
        if ((Object)(object)__instance != (Object)null && !__instance.aimAssist)
        {
            if ((int)__instance.beamType == 1 && shouldRailBounce)
            {
                __instance.ricochetAmount += railBounceAmount;
                return true;
            }
            if (__instance.canHitProjectiles)
            {
                __instance.ricochetAmount += ricBounceAmount;
                return true;
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(RevolverBeam), "PiercingShotCheck")]
    [HarmonyPrefix]
    public static void RicochetPreCheck(RevolverBeam __instance)
    {
        if ((Object)(object)MonoSingleton<DelayedActivationManager>.instance != (Object)null)
        {
            damCount = MonoSingleton<DelayedActivationManager>.instance.toActivate.Count;
        }
    }

    [HarmonyPatch(typeof(RevolverBeam), "PiercingShotCheck")]
    [HarmonyPostfix]
    public static void RicochetPostCheck(RevolverBeam __instance)
    {
        if (!((Object)(object)MonoSingleton<DelayedActivationManager>.instance != (Object)null) || damCount >= MonoSingleton<DelayedActivationManager>.instance.toActivate.Count)
        {
            return;
        }
        DelayedActivationManager instance = MonoSingleton<DelayedActivationManager>.instance;
        RevolverBeam val = default(RevolverBeam);
        for (int num = instance.toActivate.Count - 1; num >= 0; num--)
        {
            if (instance.toActivate[num].TryGetComponent<RevolverBeam>(ref val) && val.ricochetAmount == __instance.ricochetAmount)
            {
                instance.activateCountdowns[num] = ricTimer;
                break;
            }
        }
    }

    // REMOVED: LeaderboardController patches that blocked SubmitLevelScore,
    // SubmitCyberGrindScore, and SubmitFishSize. These were preventing P-ranks,
    // completion times, and stats from being recorded.
}
