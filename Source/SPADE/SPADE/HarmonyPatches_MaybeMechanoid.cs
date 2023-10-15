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
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace SPADE
    {
    public static class MaybeMechanoid
        {
        public static readonly MethodInfo getLabelFor = AccessTools.Method(typeof(PawnCapacityDef), nameof(PawnCapacityDef.GetLabelFor), new [] { typeof(bool) , typeof(bool) });
        // Simple overriding method call that can be thrown on by a transpiler without having to clean up stack or some shit.
        public static string GetLabelFor(this PawnCapacityDef def, bool _, bool _D, Pawn pawn)
            {
            return def.GetLabelFor(pawn);
            }
        // Replacer method
        public static bool IsMechanoidOrSpade(this RaceProperties props)
            {
            return props.IsMechanoid || props is RaceProperties_MaybeMechanoid;

            }
        }
    [HarmonyPatch(typeof(StatPart_Age), "ActiveFor")]
    internal static class SPADE_StatPart_Age_ActiveFor_Patch
        {
        private static bool Postfix(bool ret, Pawn pawn)
            {
            if (pawn.HasModExtension<DefExtension_MaybeMechanoid>()) return false;

            return ret;
            }
        }
    
    [HarmonyPatch(typeof(RitualRoleAssignments), nameof(RitualRoleAssignments.PawnNotAssignableReason),
                  new []{typeof(Pawn), typeof(RitualRole),typeof(Precept_Ritual), typeof(RitualRoleAssignments), typeof(TargetInfo), typeof(bool)},
                  new [] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out})]
    internal static class SPADE_StatPart_Age_PawnNotAssignableReason_Patch
        {
        private static string Postfix(string ret, Pawn p)
            {
            if (p.HasModExtension<DefExtension_MaybeMechanoid>()) return "MessageRitualPawnSPADE".Translate(p);

            return ret;
            }
        }
    
    [HarmonyPatch(typeof(PawnCapacityDef), nameof(PawnCapacityDef.GetLabelFor), new [] { typeof(Pawn) })]
    internal static class SPADE_PawnCapacityDef_GetLabelFor_Patch
        {
        private static string Postfix(string ret, Pawn pawn, PawnCapacityDef __instance)
            {
            if (pawn.HasModExtension<DefExtension_MaybeMechanoid>() && !__instance.labelMechanoids.NullOrEmpty())
                return __instance.labelMechanoids;
            return ret;
            }
        }
    
    [HarmonyPatch]
    internal static class SPADE_Check_Replacer
        {
        private static readonly MethodInfo searchFor = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.IsMechanoid));

        private static IEnumerable<MethodBase> TargetMethods()
            {
            yield return AccessTools.Method(typeof(HealthCardUtility), nameof(HealthCardUtility.DrawHealthSummary));
            }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
            return instructions.MethodReplacer(searchFor, AccessTools.Method(typeof(MaybeMechanoid), nameof(MaybeMechanoid.IsMechanoidOrSpade)));
            }
        } 
    
    [HarmonyPatch(typeof(HealthCardUtility))]
    internal static class SPADE_HealthCardUtility_Patch
        {
        [HarmonyPatch("DrawOverviewTab")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DrawOverviewTab_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
