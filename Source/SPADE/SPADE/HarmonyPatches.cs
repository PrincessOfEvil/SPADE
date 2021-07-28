using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SPADE
    {
#pragma warning disable IDE0051 // Remove unused private members

    [StaticConstructorOnStartup]
    static class HarmonyPatches
        {
        static HarmonyPatches()
            {
            var harmony = new Harmony("princess.spade");
            Log.Message("SPADE patching...");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Targeting lambdas by name, yeah, yeah.
            var healthAI_bestMed_anon_original = AccessTools.FirstMethod(
                AccessTools.FirstInner(typeof(HealthAIUtility), inner => inner.GetField("patient") != null),
                method => method.Name.Contains("b__1")); ;

            var healthAI_bestMed_anon_postfix = typeof(SPADE_HealthAIUtility_FindBestMedicine_Anon_Patch).GetMethod("Postfix", AccessTools.all);

            harmony.Patch(healthAI_bestMed_anon_original, postfix: new HarmonyMethod(healthAI_bestMed_anon_postfix));
            }
        }

    /// <summary>
    /// This patches the Hatch method so when no parent can be found (which is the case when a pawn is spawned from a crafted item), it is set so the player faction. 
    /// https://github.com/ISOR3X/communityframework/blob/main/Source/communityframework/communityframework/Harmony%20patches/CompHatcher/Hatch.cs
    /// </summary>

    [HarmonyPatch(typeof(CompHatcher))]
    [HarmonyPatch("Hatch")]
    class Hatch
        {
        public static void Prefix(CompHatcher __instance)
            {
            if (__instance.hatcheeParent == null) //If no parent is found for the hatchee, set the hatchee's faction to that of the player.
                {
                __instance.hatcheeFaction = Faction.OfPlayer;
                }
            }
        }

    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    static class SPADE_MassUtility_Capacity_Patch
        {
        static StatDef def;
        static float Postfix(float ret, Pawn p)
            {
            ensureInit();

            if (def != null)
                ret *= p.GetStatValue(def);

            return ret;
            }
        private static void ensureInit()
            {
            if (def == null)
                def = DefDatabase<StatDef>.GetNamed("spade_CaravanCarryingCapacityFactor");
            }
        }

    [HarmonyPatch(typeof(JobDriver), "ModifyCarriedThingDrawPos")]
    static class SPADE_JobDriver_ModifyCarriedThingDrawPos_Patch
        {
        static bool Postfix(bool ret, ref Vector3 drawPos, JobDriver __instance)
            {
            var defExt = __instance.GetActor().GetModExtension<DefExtension_CarriesThingsOffset>();
            if (defExt != null)
                {
                drawPos += defExt.offsets.GetFor(__instance.GetActor().Rotation);

                return true;
                }
            return ret;
            }
        }

    [HarmonyPatch(typeof(PawnRenderer), "CarryWeaponOpenly")]
    static class SPADE_PawnRenderer_CarryWeaponOpenly_Patch
        {
        static bool Postfix(bool ret, Pawn ___pawn)
            {
            if (___pawn.HasModExtension<DefExtension_CarriesWeaponOpenly>())
                return true;
            return ret;
            }
        }

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    static class SPADE_PawnRenderer_DrawEquipmentAiming_Patch 
        {
        static void Prefix(Thing eq, ref Vector3 drawLoc, ref float aimAngle, Pawn ___pawn, PawnRenderer __instance)
            {
            var defExt = ___pawn.GetModExtension<DefExtension_CarriesWeaponStraight>();
            if (defExt != null)
                {
                Stance_Busy stance_Busy = ___pawn.stances.curStance as Stance_Busy;
                if (!(stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid))
                    {
                    aimAngle = ___pawn.Rotation.AsAngle;

                    if (defExt.offsets != null)
                        {
                        drawLoc += defExt.offsets.GetFor(___pawn.Rotation);
                        }
                    }
                }
            }
        }

    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", new Type[] { typeof(Thing), typeof(Pawn), typeof(string), typeof(bool) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal})]
    static class SPADE_EquipmentUtility_CanEquip_Patch
        {
        static bool Postfix(bool ret, Thing thing, Pawn pawn, ref string cantReason, bool checkBonded)
            {
            if (cantReason is null)
                {
                if (thing.def.IsMeleeWeapon && pawn.health.hediffSet.GetAllComps().Any(comp => { return comp is HediffComp_CannotUseMelee; }))
                    {   
                    cantReason = "CannotEquipMeleeWeapons".Translate();

                    return false;
                    }
                if (thing.def.IsRangedWeapon && pawn.health.hediffSet.GetAllComps().Any(comp => { return comp is HediffComp_CannotUseRanged; }))
                    {
                    cantReason = "CannotEquipRangedWeapons".Translate();

                    return false;
                    }
                }
            return ret;
            }
        }
    static class SPADE_HealthAIUtility_FindBestMedicine_Anon_Patch
        {
        static bool Postfix(bool ret, Thing m, Pawn ___patient)
            {
            /*
            if (patient.HasModExtension<DefExtension_NonStandardMedicine>())
                {
                Predicate<Thing> validator = (Thing m) => (m.def.thingCategories.Contains(patient.GetModExtension<DefExtension_NonStandardMedicine>().medicine) && !m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 10, 1)) ? true : false;
                Thing thing = GenClosest.ClosestThing_Global_Reachable(patient.Position, patient.Map, patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer), 9999f, validator, priorityGetter);
                if (DebugViewSettings.logTutor)
                    {
                    Log.Message("Found non-standard med" + thing.def.defName);
                    }
                return thing;
                }
            if (ret.TryGetComp<Comp_NonStandardMedicine>() != null)
                {
                if (DebugViewSettings.logTutor)
                    {
                    Log.Message("Found non-standard meds, searching for standard ones...");
                    }

                Predicate<Thing> validator = (Thing m) => (m.TryGetComp<Comp_NonStandardMedicine>() == null && !m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 10, 1)) ? true : false;
                return GenClosest.ClosestThing_Global_Reachable(patient.Position, patient.Map, patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer), 9999f, validator, priorityGetter);
                }
            */

            if (ret)
                {
                if (DebugViewSettings.logTutor)
                    {
                    Log.Message("entered the anon patch");
                    }

                var defext = ___patient.GetModExtension<DefExtension_NonStandardMedicine>();
                if (defext != null && m.def.thingCategories.Contains(defext.medicine))
                    return true;
                else if (m.TryGetComp<Comp_NonStandardMedicine>() != null) return false;
                }
            return ret;
            }
        }

    [HarmonyPatch(typeof(BodyPartDef), "GetMaxHealth")]
    static class SPADE_BodyPartDef_GetMaxHealth_Patch
        {
        static float Postfix(float ret, Pawn pawn, BodyPartDef __instance)
            {
            return Mathf.CeilToInt(ret + pawn.health.hediffSet.GetAllComps().Where(hediff => { return hediff is HediffComp_ChangePartHealth && hediff.parent.Part.def == __instance; }).Cast<HediffComp_ChangePartHealth>().Sum(hediff => { return hediff.Props.health; }) * pawn.HealthScale);
            }
        }
    [HarmonyPatch(typeof(Pawn_StoryTracker), "get_DisabledWorkTagsBackstoryAndTraits")]
    static class SPADE_Pawn_StoryTracker_get_DisabledWorkTagsBackstoryAndTraits_Patch
        {
        static WorkTags Postfix(WorkTags ret, Pawn ___pawn)
            {
            return ret & (WorkTags)___pawn.health.hediffSet.GetAllComps().Where(hediff => { return hediff is HediffComp_EnableWorkTypes; }).Cast<HediffComp_EnableWorkTypes>().Sum(hediff => { return (int)hediff.Props.workTags; });
            }
        }
    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    static class SPADE_Pawn_GetDisabledWorkTypes_Patch
        {
        static List<WorkTypeDef> Postfix(List<WorkTypeDef> ret, Pawn __instance)
            {
            IEnumerable<HediffComp_EnableWorkTypes> list = __instance.health.hediffSet.GetAllComps().Where(hediff => { return hediff is HediffComp_EnableWorkTypes; }).Cast<HediffComp_EnableWorkTypes>();
            int allowedTags = list.Sum(hediff => { return (int)hediff.Props.workTags; });
            return ret.Except(
                    list.SelectMany(hediff => { return hediff.Props.workTypes; }))
                .Where(work => { return (allowedTags & (int)work.workTags) == 0; })
                .ToList();
            }
        }
    [HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell", typeof(Pawn), typeof(IntVec3))]
    static class SPADE_Pawn_PathFollower_CostToMoveIntoCell_Patch
        {
        static int Postfix(int ret, Pawn pawn, IntVec3 c)
            {
            if (pawn.health.hediffSet.GetAllComps().Where(hediff => { return hediff is HediffComp_IgnoresTerrain; }).Any())
                {
                return (c.x != pawn.Position.x && c.z != pawn.Position.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal;
                }
            return ret;
            }
        }
    }