using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace SPADE
    {
    [StaticConstructorOnStartup]
    static class BackCompatibilityInjector
        {
        static BackCompatibilityInjector()
            {
            Traverse.Create(typeof(BackCompatibility)).Field<List<BackCompatibilityConverter>>("conversionChain").Value.Add(new BackCompatibilityConverter_SPADE());
            Log.Message("SPADE back compat:");
            Log.Message(Traverse.Create(typeof(BackCompatibility)).Field<List<BackCompatibilityConverter>>("conversionChain").Value.ToStringSafeEnumerable());
            }
        }
    public class BackCompatibilityConverter_SPADE : BackCompatibilityConverter
        {
        public override bool AppliesToVersion(int majorVer, int minorVer)
            {
            return true;
            }

        public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
            {
            string ret = null;

            ret = parseBetterTurretsOverrides(defType, defName);

            if (ret != null) return ret;
            if (defType == typeof(BodyDef) && defName == "spade_spade")
                return "spade_SPADE_Body";

            if (defType == typeof(PawnKindDef))
                switch (defName)
                    {
                    case "spade_SpadeSimplePlayer": return "spade_SPADE_Ranged";
                    }

            if (defType == typeof(ThingDef))
                switch (defName)
                    {
                    case "spade_BaseEgg": return "spade_SPADE_Ranged_Spawner";
                    }

            if (defType == typeof(ResearchProjectDef))
                switch (defName)
                    {
                    case "spade_Basic": return "spade_SPADE_Research_Basic";
                    }

            return defName == "spade_spade" ? "spade_SPADE_Basic" : ret;
            }

        public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
            {
            return providedClassName switch
                {
                "BetterTurrets.Verb_ShootStatic" => typeof(Verb_ShootStatic),
                "BetterTurrets.Bullet_Tranq"     => typeof(Bullet_Tranq),
                _                                => null
                };
            }

        public override void PostExposeData(object obj)
            {
            }

        private string parseBetterTurretsOverrides(Type defType, string defName)
            {
            if (defType != typeof(ThingDef)) return null;
            return defName switch
                {
                "BetterTurrets_Gun_BattleRifle"          => "spade_BattleRifle",
                "BetterTurrets_Bullet_BattleRifle"       => "spade_Bullet_BattleRifle",
                "BetterTurrets_Gun_BattleRifle_Charge"   => "spade_BattleRifle_Charge",
                "BetterTurrets_Bullet_ChargeRifleBattle" => "spade_Bullet_ChargeRifleBattle",
                "BetterTurrets_Apparel_ArcSidearm"       => "spade_ArcSidearm",
                "BetterTurrets_Bullet_Arc"               => "spade_Bullet_Arc",
                "BetterTurrets_Apparel_TranqRifle"       => "spade_TranqRifle",
                "BetterTurrets_Bullet_Tranq"             => "spade_Bullet_Tranq",
                _                                        => null
                };
            }
        }
    }
