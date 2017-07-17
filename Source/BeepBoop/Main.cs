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
			alertsStandard     = from def in listCustomAlerts where def.sourceSound == MessageSound.Standard     select def;
			alertsRejectInput  = from def in listCustomAlerts where def.sourceSound == MessageSound.RejectInput  select def;
			alertsBenefit      = from def in listCustomAlerts where def.sourceSound == MessageSound.Benefit      select def;
			alertsNegative     = from def in listCustomAlerts where def.sourceSound == MessageSound.Negative     select def;
			alertsSeriousAlert = from def in listCustomAlerts where def.sourceSound == MessageSound.SeriousAlert select def;
			Log_.Print("# Found the following custom alerts:");
			foreach (CustomAlertDef def in listCustomAlerts)
			{
				Log_.Print($"## {def.defName}: replace MessageSound.{def.sourceSound.ToString()} in {def.sourceClass}.{def.sourceMethod} with SoundDef {def.replacementSound.ToString()}");
			}
			Log_.Print("Initialised!");
		}

		public static List<CustomAlertDef> listCustomAlerts;
		public static IEnumerable<CustomAlertDef> alertsStandard;
		public static IEnumerable<CustomAlertDef> alertsRejectInput;
		public static IEnumerable<CustomAlertDef> alertsBenefit;
		public static IEnumerable<CustomAlertDef> alertsNegative;
		public static IEnumerable<CustomAlertDef> alertsSeriousAlert;
	}

	public class Log_
	{
		private static bool DEBUG = false;

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
			switch (__state)
			{
				case MessageSound.Standard:
				{
					Log_.Warning($"Message postfix for {__state.ToString()}");
					string txt = Traverse.Create(msg).Field("text").GetValue<string>();
					Log_.Error($"# Message {__state} txt='{txt}'");
					StackTrace stackTrace = new StackTrace();
					for (int i = 0; i < stackTrace.FrameCount; i++)
					{
						MethodBase method = stackTrace.GetFrame(i).GetMethod();
						Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");
						foreach (CustomAlertDef def in Main.alertsStandard)
						{
							if (method.DeclaringType.Equals(def.type) && method.Equals(def.method))
							{
								SoundDef soundDef = def.replacementSound;
								Log_.Warning($"# Should play {def.replacementSound.defName} here");
								soundDef.PlayOneShotOnCamera();
								i = stackTrace.FrameCount;
								Log_.Warning($"# Should have played {def.replacementSound.defName} there");
								break;
							}
						}
						//Log_.Message("Did not find a matching CustomAlertDef");
					}
					break;
				}

				case MessageSound.RejectInput:
				{
					Log_.Warning($"Message postfix for {__state.ToString()}");
					string txt = Traverse.Create(msg).Field("text").GetValue<string>();
					Log_.Error($"# Message {__state} txt='{txt}'");
					StackTrace stackTrace = new StackTrace();
					for (int i = 0; i < stackTrace.FrameCount; i++)
					{
						MethodBase method = stackTrace.GetFrame(i).GetMethod();
						Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");
						foreach (CustomAlertDef def in Main.alertsRejectInput)
						{
							if (method.DeclaringType.Equals(def.type) && method.Equals(def.method))
							{
								SoundDef soundDef = def.replacementSound;
								Log_.Warning($"# Should play {def.replacementSound.defName} here");
								soundDef.PlayOneShotOnCamera();
								i = stackTrace.FrameCount;
								Log_.Warning($"# Should have played {def.replacementSound.defName} there");
								break;
							}
						}
						//Log_.Message("Did not find a matching CustomAlertDef");
					}
					break;
				}

				case MessageSound.Benefit:
				{
					Log_.Warning($"Message postfix for {__state.ToString()}");
					string txt = Traverse.Create(msg).Field("text").GetValue<string>();
					Log_.Error($"# Message {__state} txt='{txt}'");
					StackTrace stackTrace = new StackTrace();
					for (int i = 0; i < stackTrace.FrameCount; i++)
					{
						MethodBase method = stackTrace.GetFrame(i).GetMethod();
						Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");
						foreach (CustomAlertDef def in Main.alertsBenefit)
						{
							if (method.DeclaringType.Equals(def.type) && method.Equals(def.method))
							{
								SoundDef soundDef = def.replacementSound;
								Log_.Warning($"# Should play {def.replacementSound.defName} here");
								soundDef.PlayOneShotOnCamera();
								i = stackTrace.FrameCount;
								Log_.Warning($"# Should have played {def.replacementSound.defName} there");
								break;
							}
						}
						//Log_.Message("Did not find a matching CustomAlertDef");
					}
					break;
				}

				case MessageSound.Negative:
				{
					Log_.Warning($"Message postfix for {__state.ToString()}");
					string txt = Traverse.Create(msg).Field("text").GetValue<string>();
					Log_.Error($"# Message {__state} txt='{txt}'");
					StackTrace stackTrace = new StackTrace();
					for (int i = 0; i < stackTrace.FrameCount; i++)
					{
						MethodBase method = stackTrace.GetFrame(i).GetMethod();
						Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");
						foreach (CustomAlertDef def in Main.alertsNegative)
						{
							if (method.DeclaringType.Equals(def.type) && method.Equals(def.method))
							{
								SoundDef soundDef = def.replacementSound;
								Log_.Warning($"# Should play {def.replacementSound.defName} here");
								soundDef.PlayOneShotOnCamera();
								i = stackTrace.FrameCount;
								Log_.Warning($"# Should have played {def.replacementSound.defName} there");
								break;
							}
						}
						//Log_.Message("Did not find a matching CustomAlertDef");
					}
					break;
				}

				case MessageSound.SeriousAlert:
				{
					Log_.Warning($"Message postfix for {__state.ToString()}");
					string txt = Traverse.Create(msg).Field("text").GetValue<string>();
					Log_.Error($"# Message {__state} txt='{txt}'");
					StackTrace stackTrace = new StackTrace();
					for (int i = 0; i < stackTrace.FrameCount; i++)
					{
						MethodBase method = stackTrace.GetFrame(i).GetMethod();
						Log_.Message($"# {i} - {method.DeclaringType.Name}.{method.Name}");
						foreach (CustomAlertDef def in Main.alertsSeriousAlert)
						{
							if (method.DeclaringType.Equals(def.type) && method.Equals(def.method))
							{
								SoundDef soundDef = def.replacementSound;
								Log_.Warning($"# Should play {def.replacementSound.defName} here");
								soundDef.PlayOneShotOnCamera();
								i = stackTrace.FrameCount;
								Log_.Warning($"# Should have played {def.replacementSound.defName} there");
								break;
							}
						}
						//Log_.Message("Did not find a matching CustomAlertDef");
					}
					break;
				}
			}
		}
	}
}