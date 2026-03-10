using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using ULTRAKILL.Enemy;
using ULTRAKILL.Portal;

namespace RicochetAssist
{
    public static class RicochetMorty
    {
        static int ricochetCount = 0;
        static int damCount = 0;
        public static float ricFOV = 180;
        public static float ricTimer = 0.1f;
        public static bool shouldAimAssist = true;
        public static bool shouldVanillaAimAssist = false;
        public static bool shouldRailBounce = false;
        public static bool shouldTargetCoin = true;
        public static int railBounceAmount = 5;
        public static int ricBounceAmount = 5;
        static bool eidCheck(EnemyIdentifier eid) //never used :sob emoticon:
        {
            return (eid != null && !eid.dead && (eid.gameObject) && !(eid.blessed));
        }
        
        [HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.RicochetAimAssist))]
        [HarmonyPrefix]
        public static bool TargetedHarassmentCampaign(RevolverBeam __instance)
        {
            if (!shouldAimAssist) return shouldVanillaAimAssist;
            float minDist = float.PositiveInfinity;
            GameObject mainObject = null;
            Transform target = null;
            Vector3 aimPos = Vector3.zero;
            EnemyIdentifier eid = null;
            RevolverBeam revb = __instance;

            // Based on the coin vision implementation
            Vision vision = new Vision(revb.transform.position, new VisionTypeFilter(new TargetType[]
            {
                TargetType.PLAYER,
                TargetType.COIN,
                TargetType.ENEMY,
                TargetType.EXPLOSIVE,
                TargetType.GLASS
            }));
            MonoSingleton<PortalManagerV2>.Instance.TargetTracker.RegisterVision(vision, revb.destroyCancellationToken);
            // This is needed to give an index to the vision
            // Don't know how the coin imp get's away with not doing this.
            MonoSingleton<PortalManagerV2>.Instance.TargetTracker.UpdateData();
            // Based on the condition checks, It seems that the target data predicate information checks for conditions relating to the query beforehand for convinence / optimization (citations needed), but these could be checked later on if one so pleased. I think.

            VisionQuery playerQuery = new VisionQuery("RicPlayer", (TargetDataRef t) => t.target.Type == TargetType.PLAYER && !t.IsObstructed(revb.transform.position, LayerMaskDefaults.Get(LMD.Environment), false));
            // Don't know why they have a check just for gayrod specifically but I won't judge
            VisionQuery enemyQuery = new VisionQuery("RicEnemy", (TargetDataRef t) => t.target.Type == TargetType.ENEMY && (t.target.EID.enemyType == EnemyType.Geryon || !t.IsObstructed(revb.transform.position, LayerMaskDefaults.Get(LMD.Environment), false)));
            VisionQuery coinQuery = new VisionQuery("RicCoin", (TargetDataRef t) => t.target.Type == TargetType.COIN && !t.IsObstructed(revb.transform.position, LayerMaskDefaults.Get(LMD.Environment), false));
            VisionQuery explosiveQuery = new VisionQuery("RicExplosive", (TargetDataRef t) => t.target.Type == TargetType.EXPLOSIVE && !t.IsObstructed(revb.transform.position, LayerMaskDefaults.Get(LMD.Environment), false));
            /*VisionQuery glassQuery = new VisionQuery("RicGlass", delegate (TargetDataRef t)
            {
                PhysicsCastResult hit;
                PortalTraversalV2[] array2;
                return t.target.Type == TargetType.GLASS && (!t.IsObstructed(base.transform.position, this.env_lm, false, out hit, out array2) || this.GlassHitCheck(t, hit));
            });*/

            TargetDataRef targetDataRef;

            vision.UpdateSourcePos(revb.transform.position);

            
            //Debug.LogError(vision.visionIndex + " GRAAAAAAAAAAAAAAAAAAH");

            // Seems to prefer the closest target, Which is fine for what im doing, but a bit limiting.
            if (vision.TrySee(coinQuery, out targetDataRef) && shouldTargetCoin)
            {
                //revb.gameObject.transform.forward = Vector3.up;
                Coin coin = targetDataRef.target.GameObject.GetComponent<Coin>();
                if (coin != null && !coin.shot && !coin.shotByEnemy)
                {
                    target = coin.transform;
                    aimPos = targetDataRef.portalMatrix.MultiplyPoint3x4(target.position);
                    if (!Utils.WithinFOV(revb.transform.forward, (aimPos - revb.transform.position).normalized, ricFOV))
                    {
                        target = null;
                    }
                }
            }

            if (vision.TrySee(explosiveQuery, out targetDataRef))
            {
                target = targetDataRef.target.GameObject.transform;
                aimPos = targetDataRef.portalMatrix.MultiplyPoint3x4(target.position);
                if (!Utils.WithinFOV(revb.transform.forward, (aimPos - revb.transform.position).normalized, ricFOV))
                {
                    target = null;
                }
            }

            if (vision.TrySee(enemyQuery, out targetDataRef) && target == null)
            {
                eid = targetDataRef.target.EID;

                if (eid != null && !eid.dead && (eid.gameObject) && !(eid.blessed))
                {
                    if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
                    {
                        target = eid.weakPoint.transform;
                        aimPos = targetDataRef.portalMatrix.MultiplyPoint3x4(target.position);
                        if (!Utils.WithinFOV(revb.transform.forward, (aimPos - revb.transform.position).normalized, ricFOV))
                        {
                            target = null;
                        }
                    }
                    if (target == null)
                    {
                        EnemyIdentifierIdentifier eidid = eid.GetComponentInChildren<EnemyIdentifierIdentifier>();
                        if (eidid && eidid.eid && eidid.eid == eid)
                        {
                            target = eidid.transform;
                            aimPos = targetDataRef.portalMatrix.MultiplyPoint3x4(target.position);
                            if (!Utils.WithinFOV(revb.transform.forward, (aimPos - revb.transform.position).normalized, ricFOV))
                            {
                                target = null;
                            }
                        }
                        if (target == null)
                        {
                            target = eid.transform;
                            aimPos = targetDataRef.portalMatrix.MultiplyPoint3x4(target.position);
                            if (!Utils.WithinFOV(revb.transform.forward, (aimPos - revb.transform.position).normalized, ricFOV))
                            {
                                target = null;
                            }
                        }
                    }
                }
                // Im assuming this converts from whereitliterallyis-space to whereitisrelitivetotheportalsaroundit-space
                // It kinda makes sense when you think about it
                //aimPos = targetDataRef.portalMatrix.MultiplyPoint3x4(target.position);

                //revb.transform.forward = (aimPos - revb.transform.position).normalized;
                //Debug.LogError("Epic Vector Battles of History! " + target.position + " vershish " + aimPos + ". Begin!");
            }

            if (target)
            {
                revb.transform.forward = (aimPos - revb.transform.position).normalized;
            }

            return shouldVanillaAimAssist;
            /*if (CoinTracker.Instance.revolverCoinsList.Count > 0 && shouldTargetCoin)
            {
                foreach (Coin coin in CoinTracker.Instance.revolverCoinsList)
                {
                    if (coin != null && (!coin.shot || coin.shotByEnemy))
                    {
                        PhysicsCastResult phzHit;
                        if (PortalPhysicsV2.Raycast(revb.transform.position, coin.transform.position, out phzHit, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
                        {
                            if (!Utils.WithinFOV(revb.transform.forward, phzHit.direction, ricFOV))
                            {
                                continue;
                            }
                            
                            if (phzHit.distance < minDist)
                            {
                                mainObject = coin.gameObject;
                                minDist = phzHit.distance;
                            }
                        } else
                        {
                            continue;//
                        }
                    }
                }
            }
            if (mainObject == null)
            {
                minDist = float.PositiveInfinity;
                foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    if (eid != null && !eid.dead && (eid.gameObject) && !(eid.blessed))
                    {
                        if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
                        {
                            target = eid.weakPoint.transform;
                        }
                        else
                        {
                            EnemyIdentifierIdentifier eidid = eid.GetComponentInChildren<EnemyIdentifierIdentifier>();
                            if (eidid && eidid.eid && eidid.eid == eid)
                            {
                                target = eidid.transform;
                            }
                            else
                            {
                                target = eid.transform;
                            }
                        }
                        PhysicsCastResult phzHit;
                        if (PortalPhysicsV2.Raycast(revb.transform.position, target.position - revb.transform.position, out phzHit, float.PositiveInfinity, LayerMaskDefaults.Get(LMD.Environment)))
                        {
                            if (phzHit.distance < minDist)
                            {
                                mainObject = target.gameObject;
                                minDist = phzHit.distance;
                            }
                        }
                        else
                        {
                            eid = null;
                        }
                    }



*//*
                    Vector3 thing2fromthing1 = enemy.transform.position - revb.transform.position;
                    float dist = (thing2fromthing1).sqrMagnitude;
                    if (dist < minDist)
                    {
                        if (!Utils.WithinFOV(revb.transform.forward, (thing2fromthing1), ricFOV))
                        {
                            continue;
                        }//
                        eid = enemy.GetComponent<EnemyIdentifier>();
                        if (eid != null && !eid.dead && (eid.gameObject) && !(eid.blessed))
                        {
                            if (eid.weakPoint != null && eid.weakPoint.activeInHierarchy)
                            {
                                target = eid.weakPoint.transform;
                            }
                            else
                            {
                                EnemyIdentifierIdentifier eidid = eid.GetComponentInChildren<EnemyIdentifierIdentifier>();
                                if (eidid && eidid.eid && eidid.eid == eid)
                                {
                                    target = eidid.transform;
                                }
                                else
                                {
                                    target = eid.transform;
                                }
                            }
                            RaycastHit rayHit;
                            if (!Physics.Raycast(revb.transform.position, target.position - revb.transform.position, out rayHit, Vector3.Distance(revb.transform.position, target.position) - 0.5f, LayerMaskDefaults.Get(LMD.Environment)))
                            {
                                mainObject = target.gameObject;
                                minDist = dist;
                            }
                            else
                            {
                                eid = null;
                            }
                        }
                        else
                        {
                            eid = null;
                        }
                    }*//*
                }
            }
            if (mainObject != null)
            {
                revb.gameObject.transform.LookAt(mainObject.transform.position);
            }
            return shouldVanillaAimAssist;*/
        }
        [HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.Start))]
        [HarmonyPrefix]
        public static bool RicochetForAll(RevolverBeam __instance)
        {
            if (__instance != null && !__instance.aimAssist) // The first shot doesn't have aim assist
            {
                if (__instance.beamType == BeamType.Railgun && shouldRailBounce)
                {
                    __instance.ricochetAmount += railBounceAmount;
                    return true;
                }
                if (__instance.canHitProjectiles)//
                {
                    __instance.ricochetAmount += ricBounceAmount;
                    return true;
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.PiercingShotCheck))]
        [HarmonyPrefix]
        public static void RicochetPreCheck(RevolverBeam __instance)
        {
            //ricochetCount = __instance.ricochetAmount;
            if (DelayedActivationManager.instance != null)
            {
                damCount = DelayedActivationManager.instance.toActivate.Count;
            }
        }
        [HarmonyPatch(typeof(RevolverBeam), nameof(RevolverBeam.PiercingShotCheck))]
        [HarmonyPostfix]
        public static void RicochetPostCheck(RevolverBeam __instance)
        {
            if (DelayedActivationManager.instance != null && damCount < DelayedActivationManager.instance.toActivate.Count) // lazior has been added
            {
                DelayedActivationManager dam = DelayedActivationManager.instance;
                RevolverBeam revb;
                for (int i = dam.toActivate.Count-1; i >= 0; i--)
                {
                    if (dam.toActivate[i].TryGetComponent(out revb))
                    {
                        if (revb.ricochetAmount == __instance.ricochetAmount)
                        {
                            /*dam.toActivate.RemoveAt(i);
                            dam.activateCountdowns.RemoveAt(i);
                            break;*/
                            //revb.ricochetAmount += 100;
                            dam.activateCountdowns[i] = ricTimer;
                            break;
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(LeaderboardController), "SubmitCyberGrindScore")]
        [HarmonyPrefix]
        public static bool no(LeaderboardController __instance)
        {
            return false;
        }

        [HarmonyPatch(typeof(LeaderboardController), "SubmitLevelScore")]
        [HarmonyPrefix]
        public static bool nope(LeaderboardController __instance)
        {
            return false;
        }

        [HarmonyPatch(typeof(LeaderboardController), "SubmitFishSize")]
        [HarmonyPrefix]
        public static bool notevenfish(LeaderboardController __instance)
        {
            return false;
        }
    }
}
