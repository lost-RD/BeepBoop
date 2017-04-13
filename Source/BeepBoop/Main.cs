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
			Log.Message("[RD_BeepBoop] Initialising...");
			var harmony = HarmonyInstance.Create("org.rd.beepboop");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			Log.Message("[RD_BeepBoop] Done!");
		}
	}

	public class Log_
	{
		private static bool DEBUG = false;

		public static void Message(string text)
		{
			if (DEBUG)
				Log.Message(text);
		}
		public static void Warning(string text)
		{
			if (DEBUG)
				Log.Warning(text);
		}
		public static void Error(string text)
		{
			if (DEBUG)
				Log.Error(text);
		}

	}

	[HarmonyPatch]
	public class MessagesMessage_Patch
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
			Log_.Warning($"Message prefix for {sound.ToString()}");
			__state = sound;
			sound = MessageSound.Silent;
			return true;
		}

		static void Postfix(object msg, MessageSound __state)
		{
			if (__state.Equals(MessageSound.Negative))
			{
				Log_.Warning($"Message postfix for {__state.ToString()}");
				var txt = Traverse.Create(msg).Field("text").GetValue<string>();
				Log_.Error($"# Message {__state} txt='{txt}'");
				var stackTrace = new StackTrace();
				bool hasFired = false;
				for (int i = 0; i < stackTrace.FrameCount; i++)
				{
					var method = stackTrace.GetFrame(i).GetMethod();
					Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");

					if (method.DeclaringType.Equals(PawnDeathType) && method.Equals(PawnDeathMethod))
					{
						SoundDef soundDef = SoundDef.Named("PawnDeath");
						Log_.Warning("# Should play PawnDeath here");
						soundDef.PlayOneShotOnCamera();
						i = stackTrace.FrameCount;
						hasFired = true;
						Log_.Warning("# Should have played PawnDeath there");
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
					Log_.Warning("# Should play an alert here");
					soundDef.PlayOneShotOnCamera();
					Log_.Warning("# Should have played an alert there");
				}
			}
		}
	}
}