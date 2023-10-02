using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SPADE
	{
	public class Bullet_Laser : Bullet
        {
		protected bool impacted = false;

		private Material mat;

		public override Vector3 ExactPosition => destination.Yto0() + Vector3.up * def.Altitude;

		public override void Draw()
			{
			if (mat == null)
				mat = new Material(def.graphic.MatSingle);
			mat.color = mat.color.ToTransparent(GenMath.InverseParabola(def.graphicData.drawSize.y * ticksToImpact / def.projectile.speed));
			GenDraw.DrawLineBetween(origin, destination, mat, def.graphicData.drawSize.x);
			Comps_PostDraw();
			}

		public override void Tick()
			{
			if (!impacted)
				{
				impacted = true;
				Position = DestinationCell;
				ticksToImpact = Mathf.CeilToInt(def.projectile.speed);
				ImpactSomething();
				}
			this.ticksToImpact--;
			if (!this.ExactPosition.InBounds(Map))
				{
				ticksToImpact++;
				Position = ExactPosition.ToIntVec3();
				base.Destroy();
				return;
				}
			if (ticksToImpact == 60 && Find.TickManager.CurTimeSpeed == TimeSpeed.Normal && def.projectile.soundImpactAnticipate != null)
				{
				def.projectile.soundImpactAnticipate.PlayOneShot(this);
				}
			if (ticksToImpact <= 0)
				{
				if (DestinationCell.InBounds(Map))
					{
					Position = DestinationCell;
					}
				base.Destroy();
				}
			}
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
			{
			//jk
			}
		}
    }
