using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public static class MaybeMechanoid
        {
        public static MethodInfo getLabelFor = AccessTools.Method(typeof(PawnCapacityDef), nameof(PawnCapacityDef.GetLabelFor), new Type[] { typeof(bool) , typeof(bool) });
        // Simple overriding method call that can be thrown on by a transpiler without having to clean up stack or some shit.
        public static string GetLabelFor(this PawnCapacityDef def, bool _, bool _D, Pawn pawn)
            {
            return def.GetLabelFor(pawn);
            }
        }

    [HarmonyPatch(typeof(PawnCapacityDef), "GetLabelFor", new Type[] { typeof(Pawn) })]
    static class SPADE_PawnCapacityDef_GetLabelFor_Patch
        {
        static string Postfix(string ret, Pawn pawn, PawnCapacityDef __instance)
            {
            if (pawn.HasModExtension<DefExtension_MaybeMechanoid>() && !__instance.labelMechanoids.NullOrEmpty())
                return __instance.labelMechanoids;
            return ret;
            }
        }
    /*
    [HarmonyPatch(typeof(HealthCardUtility))]
    static class SPADE_HealthCardUtility_Patch
        {
        [HarmonyPatch("DrawOverviewTab")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DrawOverviewTab_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
            foreach (var instruction in instructions)
                {
                if (instruction.Calls(MaybeMechanoid.getLabelFor))
                    {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return CodeInstruction.Call(typeof(MaybeMechanoid), nameof(MaybeMechanoid.GetLabelFor));
                    }

                else yield return instruction;
                }
            }
        }
    */
    /*
     * Last resort patch (runs too often)
    [HarmonyPatch(typeof(RaceProperties), nameof(RaceProperties.IsMechanoid), MethodType.Getter)]
    static class SPADE_RaceProperties_IsMechanoid_Patch
        {
        private static Dictionary<MethodBase, bool> methods = new()
            {
            [AccessTools.Method(typeof(HealthCardUtility), nameof(HealthCardUtility.DrawHealthSummary))] = true
            };

        private static StackTrace stack;
        static bool Postfix(bool ret, RaceProperties __instance)
            {
            if (__instance is RaceProperties_MaybeMechanoid)
                {
                stack = new();

                if (stack.GetFrames().Any(sf => methods.TryGetValue(sf.GetMethod(), out bool val) && val))
                    {
                    return true;
                    }
                }

            return ret;
            }
        }
    */
    }
