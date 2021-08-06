using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SPADE
    {
    public class HediffComp_SetSkill : HediffComp
        {
        public HediffCompProperties_Skill Props => (HediffCompProperties_Skill)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
            {
            Props.skills.ForEach(skill => { Pawn.skills.GetSkill(skill).Level = Props.level; Pawn.skills.GetSkill(skill).Notify_SkillDisablesChanged(); });
            }
        public override void CompPostPostRemoved()
            {
            Props.skills.ForEach(skill => { Pawn.skills.GetSkill(skill).Level = 0; Pawn.skills.GetSkill(skill).Notify_SkillDisablesChanged(); });
            }
        public override string CompTipStringExtra
            {
            get
                {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("AffectedSkills".Translate());
                Props.skills.ForEach(type => { stringBuilder.AppendLine(" - " + type.skillLabel.CapitalizeFirst()); });
                stringBuilder.AppendLine("SkillLevel".Translate(Props.level));
                return stringBuilder.ToString().TrimEndNewlines();
                }
            }
        }
    public class HediffComp_AddSkill : HediffComp
        {
        public HediffCompProperties_Skill Props => (HediffCompProperties_Skill)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
            {
            Props.skills.ForEach(skill => { Pawn.skills.GetSkill(skill).Level += Props.level; Pawn.skills.GetSkill(skill).Notify_SkillDisablesChanged(); });
            }

        public override void CompPostPostRemoved()
            {
            Props.skills.ForEach(skill => { Pawn.skills.GetSkill(skill).Level -= Props.level; Pawn.skills.GetSkill(skill).Notify_SkillDisablesChanged(); });
            }
        public override string CompTipStringExtra
            {
            get
                {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("AffectedSkills".Translate());
                Props.skills.ForEach(type => { stringBuilder.AppendLine(" - " + type.skillLabel.CapitalizeFirst()); });
                stringBuilder.AppendLine("SkillLevel".Translate(Props.level));
                return stringBuilder.ToString().TrimEndNewlines();
                }
            }
        }
    public class HediffCompProperties_Skill : HediffCompProperties
        {
        public List<SkillDef> skills;
        public int level = 0;

        public HediffCompProperties_Skill()
            {
            compClass = typeof(HediffComp);
            }
        }
    }