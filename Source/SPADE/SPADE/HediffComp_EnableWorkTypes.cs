using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_EnableWorkTypes : HediffComp
        {
        public HediffCompProperties_EnableWorkTypes Props => (HediffCompProperties_EnableWorkTypes)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
            {
            Props.affectedSkills.ForEach(skill => { Pawn.skills.GetSkill(skill).Notify_SkillDisablesChanged(); });
            }
        public override void CompPostPostRemoved()
            {
            Props.affectedSkills.ForEach(skill => { Pawn.skills.GetSkill(skill).Notify_SkillDisablesChanged(); });
            }
        public override string CompTipStringExtra
            {
            get
                {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("EnabledWorkTags".Translate(Props.workTags.ToString()));
                if (Props.workTypes.Any())
                    {
                    stringBuilder.AppendLine("EnabledWorkTypes".Translate());
                    Props.workTypes.ForEach(type => { stringBuilder.AppendLine(" - " + type.labelShort); });
                    }
                if (Props.affectedSkills.Any())
                    {
                    stringBuilder.AppendLine("AffectedSkills".Translate());
                    Props.affectedSkills.ForEach(type => { stringBuilder.AppendLine(" - " + type.skillLabel.CapitalizeFirst()); });
                    }
                return stringBuilder.ToString().TrimEndNewlines(); 
                }
            }
        }

    public class HediffCompProperties_EnableWorkTypes : HediffCompProperties
        {
        //Lists are initialized to ensure the code doesn't get caught on any wayward nulls.
        public List<SkillDef> affectedSkills = new List<SkillDef>();
        public List<WorkTypeDef> workTypes = new List<WorkTypeDef>();
        public WorkTags workTags = WorkTags.None;

        public HediffCompProperties_EnableWorkTypes() 
            {
            compClass = typeof(HediffComp_EnableWorkTypes);
            }
        }
    }