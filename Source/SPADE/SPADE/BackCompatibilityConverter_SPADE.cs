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

            if (ret == null)
                {
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

                if (defName == "spade_spade")
                    return "spade_SPADE_Basic";
                }

            return ret;
            }

        public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
            {
            switch (providedClassName)
                {
                case "BetterTurrets.Verb_ShootStatic":
                    return typeof(Verb_ShootStatic);
                case "BetterTurrets.Bullet_Tranq":
                    return typeof(Bullet_Tranq);
                }
            return null;
            }

        public override void PostExposeData(object obj)
            {
            }

        private string parseBetterTurretsOverrides(Type defType, string defName)
            {
            if (defType == typeof(ThingDef))
                {
                switch (defName)
                    {
                    case "BetterTurrets_Gun_BattleRifle":
                        return "spade_BattleRifle";
                    case "BetterTurrets_Bullet_BattleRifle":
                        return "spade_Bullet_BattleRifle";

                    case "BetterTurrets_Gun_BattleRifle_Charge":
                        return "spade_BattleRifle_Charge";
                    case "BetterTurrets_Bullet_ChargeRifleBattle":
                        return "spade_Bullet_ChargeRifleBattle";


                    case "BetterTurrets_Apparel_ArcSidearm":
                        return "spade_ArcSidearm";
                    case "BetterTurrets_Bullet_Arc":
                        return "spade_Bullet_Arc";

                    case "BetterTurrets_Apparel_TranqRifle":
                        return "spade_TranqRifle";
                    case "BetterTurrets_Bullet_Tranq":
                        return "spade_Bullet_Tranq";
                    }
                }
            return null;
            }
        }
    }
