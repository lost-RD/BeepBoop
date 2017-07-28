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
			alertsStandard = from def in listCustomAlerts where def.sourceMessageSound == MessageSound.Standard select def;
			alertsRejectInput = from def in listCustomAlerts where def.sourceMessageSound == MessageSound.RejectInput select def;
			alertsBenefit = from def in listCustomAlerts where def.sourceMessageSound == MessageSound.Benefit select def;
			alertsNegative = from def in listCustomAlerts where def.sourceMessageSound == MessageSound.Negative select def;
			alertsSeriousAlert = from def in listCustomAlerts where def.sourceMessageSound == MessageSound.SeriousAlert select def;
			Log_.Print("# Found the following custom alerts:");
			foreach (CustomAlertDef def in listCustomAlerts)
			{
				Log_.Print($"## {def.defName}: replace MessageSound.{def.sourceMessageSound.ToString()} in {def.sourceClass}.{def.sourceMethod} with SoundDef {def.replacementSoundDef.ToString()}");
			}
			Log_.Print("Initialised!");
		}

		public static List<CustomAlertDef> listCustomAlerts;
		public static IEnumerable<CustomAlertDef> alertsStandard;
		public static IEnumerable<CustomAlertDef> alertsRejectInput;
		public static IEnumerable<CustomAlertDef> alertsBenefit;
		public static IEnumerable<CustomAlertDef> alertsNegative;
		public static IEnumerable<CustomAlertDef> alertsSeriousAlert;

		public static List<string> boringMethods = new List<string>();
	}

	public class Log_
	{
		private static bool DEBUG = false;

		public static void Message(string text)
		{
			if (DEBUG)
				Log.Message("[RD_BeepBoop] " + text);
		}
		public static void Warning(string text)
		{
			if (DEBUG)
				Log.Warning("[RD_BeepBoop] " + text);
		}
		public static void Error(string text)
		{
			if (DEBUG)
				Log.Error("[RD_BeepBoop] " + text);
		}
		public static void Print(string text)
		{
			Log.Message("[RD_BeepBoop] " + text);
		}

	}

	[HarmonyPatch]
	public class MessagesMessage_Patch
	{

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
			IEnumerable<CustomAlertDef> list;
			SoundDef soundDef = null;
			switch (__state)
			{
				case MessageSound.Standard:
					soundDef = SoundDefOf.MessageAlert;
					list = Main.alertsStandard;
					break;
				case MessageSound.RejectInput:
					soundDef = SoundDefOf.ClickReject;
					list = Main.alertsRejectInput;
					break;
				case MessageSound.Benefit:
					soundDef = SoundDefOf.MessageBenefit;
					list = Main.alertsBenefit;
					break;
				case MessageSound.Negative:
					soundDef = SoundDefOf.MessageAlertNegative;
					list = Main.alertsNegative;
					break;
				case MessageSound.SeriousAlert:
					soundDef = SoundDefOf.MessageSeriousAlert;
					list = Main.alertsSeriousAlert;
					break;
				default:
					soundDef = null;
					list = null;
					break;
			}
			string firstFrameType = null;
			string firstFrameMethod = null;
			Log_.Warning($"Message postfix for {__state.ToString()}");
			string txt = Traverse.Create(msg).Field("text").GetValue<string>();
			Log_.Error($"# Message {__state} txt='{txt}'");
			StackTrace stackTrace = new StackTrace();
			for (int i = 0; i < stackTrace.FrameCount; i++)
			{
				MethodBase method = stackTrace.GetFrame(i).GetMethod();
				Type type = method.DeclaringType;
				string typeString = type.Name;
				string methodString = method.Name;
				string frameString = typeString + "." + methodString;
				if (Main.boringMethods.Contains(frameString))
				{
					Log_.Message("Detected a method with no CustomAlertDef.");
					break;
				}
				if (typeString == "MessagesMessage_Patch" || methodString == "Message_Patch2" || methodString == "Message_Patch1" || (typeString == "Patch" && methodString == "Prefix"))
				{
					continue;
				}
				Log_.Message($"# {i} - {frameString}");
				if (firstFrameMethod == null && firstFrameType == null)
				{
					firstFrameType = typeString;
					firstFrameMethod = methodString;
				}
				foreach (CustomAlertDef def in list)
				{
					Log_.Error($"Def: {def.sourceClass}.{def.sourceMethod}");
					if (typeString == def.sourceClass && methodString == def.sourceMethod)
					{
						soundDef = def.replacementSoundDef;
						Log_.Warning($"# Should play {def.replacementSoundDef.defName} here");
						soundDef.PlayOneShotOnCamera();
						i = stackTrace.FrameCount;
						Log_.Warning($"# Should have played {def.replacementSoundDef.defName} there");
						break;
					}
				}
				//Log_.Message("Did not find a matching CustomAlertDef");
			}
			if (firstFrameType != null && firstFrameMethod != null)
			{
				string firstFrameString = firstFrameType + "." + firstFrameMethod;
				Log_.Message($"First frame: {firstFrameString}; Adding to list of methods to be ignored");
				Main.boringMethods.Add(firstFrameString);
			}
			soundDef.PlayOneShotOnCamera();
		}
	}
}