using Verse;
using Harmony;
using RimWorld;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace BeepBoop
{
	[StaticConstructorOnStartup]
	class Main
	{
		static Main()
		{
			var harmony = HarmonyInstance.Create("org.rd.beepboop");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch]
	static class Patcher
	{
		/* SameSpot's code
		 * static bool DestinationIsReserved(this PawnDestinationManager instance, IntVec3 c, Pawn searcher)
		{
			if (Find.Selector.SelectedObjects.Count() == 1) return false;
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return false;
			return instance.DestinationIsReserved(c, searcher);
		}*/

		static MethodBase TargetMethod()
		{
			/* SameSpot's code
			 * var predicateClass = typeof(RCellFinder).GetNestedTypes(AccessTools.all)
				.FirstOrDefault(t => t.FullName.Contains("BestOrderedGotoDestNear"));
			return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.ReturnType == typeof(bool));*/

			/* need to redirect an invokation of 
				private static void Verse.Messages.Message(Messages.LiveMessage msg, MessageSound sound)
			from within
				public void Verse.Pawn_HealthTracker.Kill(DamageInfo? dinfo, Hediff hediff)
			to a method of my own
			*/

			var predicateClass = typeof(Pawn_HealthTracker).GetNestedTypes(AccessTools.all)
				.FirstOrDefault(t => t.FullName.Contains("Kill"));
			return predicateClass.GetMethods(AccessTools.all).FirstOrDefault( /* what to put here? */ );
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return instructions

				/* SameSpot code
				 * .MethodReplacer(
					AccessTools.Method(typeof(PawnDestinationManager), "DestinationIsReserved", new Type[] { typeof(IntVec3), typeof(Pawn) }),
					AccessTools.Method(typeof(Patcher), "DestinationIsReserved")
				);*/

				.MethodReplacer( // CodeInstruction does not contain a definition for MethodReplacer
					AccessTools.Method(typeof(Verse.Messages), "Message", new Type[] { AccessTools.TypeByName("LiveMessage"), typeof(MessageSound) }),
					AccessTools.Method(typeof(BeepBoop.Messages), "Message", new Type[] { typeof(string), typeof(SoundDef) })
				);
		}
	}
}