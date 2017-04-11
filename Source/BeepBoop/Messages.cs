using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;
using System.Reflection;
using Harmony;

namespace BeepBoop
{
	class Messages
	{
		// Verse.Messages
		public static void Message(string text, SoundDef sound)
		{
			MethodInfo dynMethod = typeof(Verse.Messages).GetMethod("AcceptsMessages", (BindingFlags)60);
			//dynMethod.Invoke(null, new object[] { text, TargetInfo.Invalid });

			if (dynMethod.Invoke(null, new object[] { text, TargetInfo.Invalid }).Equals(false))
			{
				return;
			}

			//Messages.LiveMessage msg = new Messages.LiveMessage(text);
			var msg = AccessTools.TypeByName("LiveMessage").GetConstructor(new Type[] { typeof(string) }).Invoke(new object[] { text });
			//Messages.liveMessages.Add(msg);
			List<object> liveMessages = Traverse.Create(typeof(Verse.Messages)).Field("liveMessages").GetValue<List<object>>();
			liveMessages.Add(msg);
			while (liveMessages.Count > 12)
			{
				liveMessages.RemoveAt(0);
			}
			Traverse.Create(typeof(Verse.Messages)).Field("liveMessages").SetValue(liveMessages);
			sound.PlayOneShotOnCamera();
		}
	}
}
