using Verse;
using Verse.Sound;
using Harmony;
using RimWorld;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace RD_BeepBoop
{
	[StaticConstructorOnStartup]
	class Main
	{
		static Main()
		{
			Log_.Print("Initialising...");
			var harmony = HarmonyInstance.Create("org.rd.beepboop");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			listCustomAlerts = (List<CustomAlertDef>)DefDatabase<CustomAlertDef>.AllDefs;
			Log_.Print("# Found the following custom alerts:");
			foreach (CustomAlertDef def in listCustomAlerts)
			{
				Log_.Print($"## {def.defName}: replace MessageSound.{def.sourceSound.ToString()} in {def.sourceClass}.{def.sourceMethod} with SoundDef {def.replacementSound.ToString()}");
			}
			Log_.Print("Initialised!");
		}

		public static List<CustomAlertDef> listCustomAlerts;
	}

	public class Log_
	{
		private static bool DEBUG = true;

		public static void Message(string text)
		{
			if (DEBUG)
				Log.Message("[RD_BeepBoop] "+text);
		}
		public static void Warning(string text)
		{
			if (DEBUG)
				Log.Warning("[RD_BeepBoop] "+text);
		}
		public static void Error(string text)
		{
			if (DEBUG)
				Log.Error("[RD_BeepBoop] "+text);
		}
		public static void Print(string text)
		{
			Log.Message("[RD_BeepBoop] " + text);
		}

	}

	[HarmonyPatch]
	public class MessagesMessage_Patch
	{

		public static Type PawnDeathType = typeof(Pawn_HealthTracker);
		public static MethodBase PawnDeathMethod = PawnDeathType.GetMethod("NotifyPlayerOfKilled");

		public static Type PlantDeathType = typeof(Plant);
		public static MethodBase PlantDeathMethod = PlantDeathType.GetMethod("MakeLeafless");

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
			bool hasFired = false;
			switch (__state)
			{
				case MessageSound.Negative:
				{
					Log_.Warning($"Message postfix for {__state.ToString()}");
					var txt = Traverse.Create(msg).Field("text").GetValue<string>();
					Log_.Error($"# Message {__state} txt='{txt}'");
					var stackTrace = new StackTrace();
					for (int i = 0; i < stackTrace.FrameCount; i++)
					{
						var method = stackTrace.GetFrame(i).GetMethod();
						Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");

						if (method.DeclaringType.Equals(PawnDeathType) && method.Equals(PawnDeathMethod))
						{
							string defName = "PawnDeath";
							SoundDef soundDef = SoundDef.Named(defName);
							Log_.Warning($"# Should play {defName} here");
							soundDef.PlayOneShotOnCamera();
							i = stackTrace.FrameCount;
							hasFired = true;
							Log_.Warning($"# Should have played {defName} there");
							break;
						}
						else if (method.DeclaringType.Equals(PlantDeathType) && method.Equals(PlantDeathMethod))
						{
							string defName = "PlantDeath";
							SoundDef soundDef = SoundDef.Named(defName);
							Log_.Warning($"# Should play {defName} here");
							soundDef.PlayOneShotOnCamera();
							i = stackTrace.FrameCount;
							hasFired = true;
							Log_.Warning($"# Should have played {defName} there");
							break;
						}
					}
					break;
				}
				default:
				{
					if (!hasFired)
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
				break;
				}
			}
		}
	}
}