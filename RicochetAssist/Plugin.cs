using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;
using UnityEngine;

namespace RicochetAssist;

[BepInPlugin("ironfarm.uk.ricomorty", "RicochetAssist", "1.0.2")]
[BepInDependency("com.eternalUnion.pluginConfigurator")]
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

        new ConfigHeader(config.rootPanel, "<color=red>Does not conflict with modded aim assist</color>", 15);

        VanillaAimAssistEnabled   = new BoolField(config.rootPanel,  "Enable Vanilla Aim Assist",      "bool.vanillaaimassist",           false);
        AimAssistEnabled          = new BoolField(config.rootPanel,  "Enable Aim Assist",               "bool.aimassist",                  true);
        RicochetFOV               = new FloatField(config.rootPanel, "Ricochet FOV",                    "float.ricochetfov",               180f, 0f, 360f);
        TargetCoins               = new BoolField(config.rootPanel,  "Target Coins",                    "bool.targetcoins",                true);

        new ConfigHeader(config.rootPanel, "<color=red>At 0, shots will ricochet while paused!</color>", 15);

        RicochetTimer             = new FloatField(config.rootPanel, "Ricochet Timer",                  "float.ricochettimer",             0.1f, 0f, 100f);
        SharpshooterExtraRicochet = new IntField(config.rootPanel,   "Extra Sharpshooter Ricochet",     "int.sharpshooterextraricochet",   5, 0, 100000000);
        RailcannonRicochetEnable  = new BoolField(config.rootPanel,  "Enable Railcannon Ricochet",      "bool.railcannonricochet",         false);
        RailcannonExtraRicochet   = new IntField(config.rootPanel,   "Extra Railcannon Ricochet",       "int.railcannonextraricochet",     5, 0, 100000000);

        VanillaAimAssistEnabled.onValueChange += e => RicochetMorty.shouldVanillaAimAssist = e.value;

        AimAssistEnabled.onValueChange += e =>
        {
            ((ConfigField)RicochetFOV).hidden = !e.value;
            ((ConfigField)TargetCoins).hidden = !e.value;
            RicochetMorty.shouldAimAssist     = e.value;
        };

        RicochetFOV.onValueChange             += e => RicochetMorty.ricFOV          = e.value;
        TargetCoins.onValueChange             += e => RicochetMorty.shouldTargetCoin = e.value;
        RicochetTimer.onValueChange           += e => RicochetMorty.ricTimer         = e.value;
        SharpshooterExtraRicochet.onValueChange += e => RicochetMorty.ricBounceAmount = e.value;

        RailcannonRicochetEnable.onValueChange += e =>
        {
            ((ConfigField)RailcannonExtraRicochet).hidden = !e.value;
            RicochetMorty.shouldRailBounce = e.value;
        };

        RailcannonExtraRicochet.onValueChange += e => RicochetMorty.railBounceAmount = e.value;

        VanillaAimAssistEnabled.TriggerValueChangeEvent();
        AimAssistEnabled.TriggerValueChangeEvent();
        RicochetFOV.TriggerValueChangeEvent();
        TargetCoins.TriggerValueChangeEvent();
        RicochetTimer.TriggerValueChangeEvent();
        SharpshooterExtraRicochet.TriggerValueChangeEvent();
        RailcannonRicochetEnable.TriggerValueChangeEvent();
        RailcannonExtraRicochet.TriggerValueChangeEvent();

        string modDir   = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string iconPath = Path.Combine(modDir, "Data", "icon.png");
        config.SetIconWithURL("file://" + iconPath);
    }
}

public static class Utils
{
    public static bool WithinFOV(Vector3 main, Vector3 target, float fov)
    {
        float angle   = Mathf.Acos(Vector3.Dot(main.normalized, target.normalized));
        float halfFov = fov / 2f % 360f * Mathf.Deg2Rad;
        return angle <= halfFov || angle >= Mathf.PI * 2f - halfFov;
    }
}

public static class RicochetMorty
{
    private static int damCount = 0;

    public static float ricFOV               = 180f;
    public static float ricTimer             = 0.1f;
    public static bool  shouldAimAssist      = true;
    public static bool  shouldVanillaAimAssist = false;
    public static bool  shouldRailBounce     = false;
    public static bool  shouldTargetCoin     = true;
    public static int   railBounceAmount     = 5;
    public static int   ricBounceAmount      = 5;

    // ── Aim Assist ────────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(RevolverBeam), "RicochetAimAssist")]
    [HarmonyPrefix]
    public static bool TargetedHarassmentCampaign(RevolverBeam __instance)
    {
        if (!shouldAimAssist)
            return shouldVanillaAimAssist;

        RevolverBeam revb       = __instance;
        Transform    bestTarget   = null;
        Vector3      bestPosition = Vector3.zero;
        Vector3      pos          = ((Component)revb).transform.position;

        TargetType[] types = new TargetType[]
        {
            (TargetType)0, (TargetType)1, (TargetType)2, (TargetType)3, (TargetType)4
        };

        Vision vision = new Vision(pos, new VisionTypeFilter(types));
        MonoSingleton<PortalManagerV2>.Instance.TargetTracker.RegisterVision(vision, ((MonoBehaviour)revb).destroyCancellationToken);
        MonoSingleton<PortalManagerV2>.Instance.TargetTracker.UpdateData();

        var queryCoin      = new VisionQuery("RicCoin",      (TargetPredicate)(t => (int)t.target.Type == 3 && !TargetDataExtensions.IsObstructed(t, pos, LayerMaskDefaults.Get((LMD)1), false)));
        var queryExplosive = new VisionQuery("RicExplosive",  (TargetPredicate)(t => (int)t.target.Type == 4 && !TargetDataExtensions.IsObstructed(t, pos, LayerMaskDefaults.Get((LMD)1), false)));
        var queryEnemy     = new VisionQuery("RicEnemy",      (TargetPredicate)(t => (int)t.target.Type == 2 && ((int)t.target.EID.enemyType == 42 || !TargetDataExtensions.IsObstructed(t, pos, LayerMaskDefaults.Get((LMD)1), false))));

        vision.UpdateSourcePos(pos);

        TargetDataRef hit = default;

        // Coins
        if (shouldTargetCoin && vision.TrySee(queryCoin, ref hit))
        {
            Coin coin = hit.target.GameObject.GetComponent<Coin>();
            if (coin != null && !coin.shot && !coin.shotByEnemy)
            {
                bestTarget   = ((Component)coin).transform;
                bestPosition = hit.portalMatrix.MultiplyPoint3x4(bestTarget.position);
                if (!Utils.WithinFOV(((Component)revb).transform.forward, (bestPosition - pos).normalized, ricFOV))
                    bestTarget = null;
            }
        }

        // Explosives
        if (vision.TrySee(queryExplosive, ref hit))
        {
            bestTarget   = hit.target.GameObject.transform;
            bestPosition = hit.portalMatrix.MultiplyPoint3x4(bestTarget.position);
            if (!Utils.WithinFOV(((Component)revb).transform.forward, (bestPosition - pos).normalized, ricFOV))
                bestTarget = null;
        }

        // Enemies
        if (bestTarget == null && vision.TrySee(queryEnemy, ref hit))
        {
            EnemyIdentifier eid = hit.target.EID;
            if (eid != null && !eid.dead && (bool)(Object)(object)((Component)eid).gameObject && !eid.blessed)
            {
                // Weak point
                if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
                {
                    bestTarget   = eid.weakPoint.transform;
                    bestPosition = hit.portalMatrix.MultiplyPoint3x4(bestTarget.position);
                    if (!Utils.WithinFOV(((Component)revb).transform.forward, (bestPosition - pos).normalized, ricFOV))
                        bestTarget = null;
                }

                // EnemyIdentifierIdentifier
                if (bestTarget == null)
                {
                    var eii = ((Component)eid).GetComponentInChildren<EnemyIdentifierIdentifier>();
                    if (eii != null && eii.eid != null && (Object)(object)eii.eid == (Object)(object)eid)
                    {
                        bestTarget   = ((Component)eii).transform;
                        bestPosition = hit.portalMatrix.MultiplyPoint3x4(bestTarget.position);
                        if (!Utils.WithinFOV(((Component)revb).transform.forward, (bestPosition - pos).normalized, ricFOV))
                            bestTarget = null;
                    }
                }

                // Root fallback
                if (bestTarget == null)
                {
                    bestTarget   = ((Component)eid).transform;
                    bestPosition = hit.portalMatrix.MultiplyPoint3x4(bestTarget.position);
                    if (!Utils.WithinFOV(((Component)revb).transform.forward, (bestPosition - pos).normalized, ricFOV))
                        bestTarget = null;
                }
            }
        }

        if (bestTarget != null)
            ((Component)revb).transform.forward = (bestPosition - pos).normalized;

        return shouldVanillaAimAssist;
    }

    // ── Extra Ricochet Bounces ────────────────────────────────────────────────

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

    // ── Ricochet Timer ────────────────────────────────────────────────────────

    [HarmonyPatch(typeof(RevolverBeam), "PiercingShotCheck")]
    [HarmonyPrefix]
    public static void RicochetPreCheck(RevolverBeam __instance)
    {
        if (MonoSingleton<DelayedActivationManager>.instance != null)
            damCount = MonoSingleton<DelayedActivationManager>.instance.toActivate.Count;
    }

    [HarmonyPatch(typeof(RevolverBeam), "PiercingShotCheck")]
    [HarmonyPostfix]
    public static void RicochetPostCheck(RevolverBeam __instance)
    {
        var dam = MonoSingleton<DelayedActivationManager>.instance;
        if (dam == null || damCount >= dam.toActivate.Count)
            return;

        RevolverBeam beam = default;
        for (int i = dam.toActivate.Count - 1; i >= 0; i--)
        {
            if (dam.toActivate[i].TryGetComponent<RevolverBeam>(ref beam) && beam.ricochetAmount == __instance.ricochetAmount)
            {
                dam.activateCountdowns[i] = ricTimer;
                break;
            }
        }
    }

    // NOTE: The original mod contained three patches here that blocked
    // LeaderboardController.SubmitLevelScore, SubmitCyberGrindScore, and
    // SubmitFishSize. These have been intentionally removed so that P-ranks,
    // completion times, and scores are recorded normally.
}
