using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Harmony;

namespace RD_BeepBoop
{
	class CustomAlertDef : Def
	{
		public MessageSound sourceMessageSound;
		public SoundDef replacementSoundDef;
		public string sourceClass;
		public string sourceMethod;

		public Type type
		{
			get
			{
				return AccessTools.TypeByName(sourceClass);
			}
		}

		public MethodBase method
		{
			get
			{
				return type.GetMethod(sourceMethod);
			}
		}

		public static CustomAlertDef Named(string defName)
		{
			return DefDatabase<CustomAlertDef>.GetNamed(defName, true);
		}
		
		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string e in base.ConfigErrors())
			{
				yield return e;
			}
			if (this.sourceMethod.NullOrEmpty())
			{
				yield return "no source method";
			}
			if (this.sourceClass.NullOrEmpty())
			{
				yield return "no source method";
			}
			if (this.replacementSoundDef == null)
			{
				yield return "no replacement sound";
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
		}
	}
}
