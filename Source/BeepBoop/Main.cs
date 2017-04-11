using Verse;
using Verse.Sound;
using Harmony;
using RimWorld;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

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
	public class MessagesMessagePatch
	{

		public static Type PawnDeathType = typeof(Pawn_HealthTracker);
		public static MethodBase PawnDeathMethod = PawnDeathType.GetMethod("Kill");

		static MethodBase TargetMethod()
		{
			var parameters = new Type[] { AccessTools.Inner(typeof(Messages), "LiveMessage"), typeof(MessageSound) };
			return AccessTools.Method(typeof(Messages), "Message", parameters);
		}

		static bool Prefix(ref MessageSound sound, out MessageSound __state)
		{
			__state = sound;
			sound = MessageSound.Silent;
			return true;
		}

		static void Postfix(object msg, MessageSound __state)
		{
			var txt = Traverse.Create(msg).Field("text").GetValue<string>();
			Log.Error("# Message " + __state + " txt=\"" + txt + "\"");
			var stackTrace = new StackTrace();
			bool hasFired = false;
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				var method = stackTrace.GetFrame(i).GetMethod();
				Log.Warning("# " + i + " - " + method.DeclaringType.FullName + " " + method.Name);
				if (method.DeclaringType.Equals(PawnDeathType) && method.Equals(PawnDeathMethod))
				{
					SoundDef soundDef = null;
					soundDef = SoundDefOf.PawnDeath;
					Log.Warning("# Should play a custom sound here");
					soundDef.PlayOneShotOnCamera();
					i = stackTrace.FrameCount;
					hasFired = true;
					Log.Warning("# Should have played a custom sound there");
					break;
				}
			}
			if (__state != MessageSound.Silent && !hasFired)
			{
				SoundDef soundDef = null;
				switch (__state)
				{
					case MessageSound.Standard:
						soundDef = RimWorld.SoundDefOf.MessageAlert;
						break;
					case MessageSound.RejectInput:
						soundDef = RimWorld.SoundDefOf.ClickReject;
						break;
					case MessageSound.Benefit:
						soundDef = RimWorld.SoundDefOf.MessageBenefit;
						break;
					case MessageSound.Negative:
						soundDef = RimWorld.SoundDefOf.MessageAlertNegative;
						break;
					case MessageSound.SeriousAlert:
						soundDef = RimWorld.SoundDefOf.MessageSeriousAlert;
						break;
				}
				Log.Warning("# Should play an alert here");
				soundDef.PlayOneShotOnCamera();
				Log.Warning("# Should have played an alert there");
			}
		}
	}
}