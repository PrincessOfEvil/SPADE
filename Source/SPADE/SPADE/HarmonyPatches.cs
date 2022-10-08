using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                method => method.Name.Contains("b__1"));

            var healthAI_bestMed_anon_postfix = typeof(SPADE_HealthAIUtility_FindBestMedicine_Anon_Patch).GetMethod("Postfix", AccessTools.all);

            harmony.Patch(healthAI_bestMed_anon_original, postfix: new HarmonyMethod(healthAI_bestMed_anon_postfix));


            var vmu = AccessTools.TypeByName("MVCF.Utilities.VerbManagerUtility");
            if (vmu != null)
                {
                var method = AccessTools.Method(vmu, "AddVerbs", new Type[] { AccessTools.TypeByName("MVCF.VerbManager"), typeof(Hediff) });
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(SPADEHacksIntoModsInTheNameOfChaos_MVCF_Utilities_VerbManagerUtility_AddVerbs_WithHediff_Patch).GetMethod("Prefix", AccessTools.all)));
                }
            }

        /*
        public static void DrawResearchPrereqsPostfix(ResearchProjectDef project, Rect rect, ref float __result)
            {
            //init
            float xMin = rect.xMin;
            float yMin = rect.yMin;
            rect.xMin += 6f;



            //restoring defaults
            GUI.color = Color.white;
            rect.xMin = xMin;
            __result += rect.yMin - yMin;
            }
        */
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

    /* 
     * This patch removes a bit of a vanilla backwards compatibility break in VEF's MVCF Ranged Hediffs.
     * Namely, them making all hediffs capable of being ranged.
     */
    static class SPADEHacksIntoModsInTheNameOfChaos_MVCF_Utilities_VerbManagerUtility_AddVerbs_WithHediff_Patch
        {
        private static Dictionary<string, bool> modCompatDictionary = new Dictionary<string, bool>();
        static bool Prefix(Hediff hediff)
            {
            var mod = hediff?.def?.modContentPack?.Name;
            /*
             * ...it would get ignored anyway and i'm sparing a couple cycles.
             * Checking for self for any 'lets just block them out and ignore it lul' shenanigans.
             */
            if (MVCF.Base.IsIgnoredMod(mod) && mod != "SPADE") return false;

            var comp = hediff.TryGetComp<HediffComp_VerbGiver>();
            if (comp?.VerbTracker?.AllVerbs == null) return false;

            if (comp.VerbTracker.AllVerbs.Any(v => !v.IsMeleeAttack))
                {
                if (!MVCF.Base.Features.HediffVerbs && !MVCF.Base.IgnoredFeatures.HediffVerbs)
                    {
                    Log.ErrorOnce(
                        "[MVCF] Found a hediff with a ranged verb while that feature is not enabled. Enabling now. This is not recommend. Contant the author of " +
                        mod + " and ask them to add a MVCF.ModDef.",
                        mod?.GetHashCode() ?? -1);
                    Log.ErrorOnce(
                        "[SPADE] Found an instance of MVCF being mentally deficient. " +
                        "If you have your own ways of triggering the ranged verb on your hediff, of if you're reading this message from just tossing mods together, just ignore it. " +
                        "If your hediff needs MVCF to work, and you're seeing this message, you should really make the ModDef.",
                        mod?.GetHashCode() + 1 ?? -1);
                    return false;
                    }
                if (notInDatabase(mod))
                    {
                    Log.ErrorOnce(
                        "[SPADE] Found an instance of MVCF being mentally deficient. " +
                        "Ranged hediffs enabled by another mod, but " + mod +
                        " has not opted in to use them. Since vanilla ignores ranged hediffs by default, this is considered breaking behavior. " +
                        "Contact the author of " + mod + " and warn them about their possible incompatibility with Vanilla Expanded Framework and specifically MVCF.",
                        mod?.GetHashCode() + 1 ?? -1);
                    return false;
                    }
                }

            return true;
            }

        private static bool notInDatabase(string mod)
            {
            bool value;
            if (!modCompatDictionary.TryGetValue(mod, out value))
                {
                value = !DefDatabase<MVCF.ModDef>.AllDefs.Any( def => def.modContentPack?.Name == mod);
                modCompatDictionary[mod] = value;
                }
            return value;
            }
        }


    [HarmonyPatch(typeof(JobDriver_BeatFire), "MakeNewToils")]
    static class SPADE_JobDriver_BeatFire_MakeNewToils_Patch
        {
        static IEnumerable<Toil> Postfix(IEnumerable<Toil> ret, JobDriver_BeatFire __instance)
            {
            var input = __instance.pawn.health.hediffSet.GetAllComps().FirstOrFallback(hediff => hediff is HediffComp_VerbGiver diff && !diff.VerbProperties.NullOrEmpty() && diff.VerbProperties.Any(props => props.category == VerbCategory.BeatFire)) as HediffComp_VerbGiver;
            if (input != null)
                return iterator(input, __instance);

            else return ret;
            }

        static IEnumerable<Toil> iterator(HediffComp_VerbGiver input, JobDriver_BeatFire __instance)
            {
            __instance.job.verbToUse = input.VerbTracker.PrimaryVerb;

            Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, maxRangeFactor: 0.65f);
            yield return gotoCastPos;
            Toil jumpIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
            yield return jumpIfCannotHit;
            yield return Toils_Combat.CastVerb(TargetIndex.A);
            yield return Toils_Jump.Jump(jumpIfCannotHit);
            }
        }

    [HarmonyPatch(typeof(FoodTypeFlagsExtension), "ToHumanString")]
    static class SPADE_FoodTypeFlagsExtension_ToHumanString_Patch
        {
        // 2^23
        // Keeping it a calculated constant for copypasting reasons.
        private const int JetFuel = 8388608;
        static string Postfix(string ret, FoodTypeFlags ft)
            {
            if (!Harmony.HasAnyPatches("twoscythe.shipgirls") && ((int)ft & JetFuel) != 0)
                {
                if (ret.Length > 0)
                    {
                    ret += ", ";
                    }
                ret += "FoodTypeFlags_JetFuel".Translate();
                }

            return ret;
            }
        }

    [HarmonyPatch(typeof(Caravan), "get_NightResting")]
    static class SPADE_Caravan_get_NightResting_Patch
        {
        static bool Postfix(bool ret, Caravan __instance)
            {
            if (ret) 
                {
                if (!__instance.PawnsListForReading.Where((Pawn p) => !p.HasModExtension<DefExtension_DoesNotNeedCaravanRest>()).Any())
                    return false;
                }

            return ret;
            }
        }

    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    static class SPADE_MassUtility_Capacity_Patch
        {
        static StatDef def;
        static float Postfix(float ret, Pawn p, StringBuilder explanation)
            {
            ensureInit();

            if (def != null)
                {
                ret *= p.GetStatValue(def);

                if (explanation != null)
                    {
                    explanation.AppendLine();
                    explanation.Append("  - " + p.LabelShortCap + ": x" + p.GetStatValue(def).ToStringPercent());
                    explanation.AppendLine();
                    explanation.Append("  - " + p.LabelShortCap + ": " + ret.ToStringMassOffset());
                    }
                }

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
                if (defext != null && !m.def.thingCategories.Contains(defext.medicine))
                    return false;
                if (defext == null && m.TryGetComp<Comp_NonStandardMedicine>() != null)
                    return false;
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
                int i = (c.x != pawn.Position.x && c.z != pawn.Position.z) ? pawn.TicksPerMoveDiagonal : pawn.TicksPerMoveCardinal;

                if (pawn.CurJob != null && pawn.jobs.curJob.locomotionUrgency == LocomotionUrgency.Sprint)
                    i = Mathf.RoundToInt((float)i * 0.75f); ;

                return i;
                }

            return ret;
            }
        }
    }