using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerTrading.Patches
{
    class MessageHudPatches
    {
		[HarmonyPriority(9999)]
		[HarmonyPatch(typeof(MessageHud), nameof(MessageHud.UpdateBiomeFound))]
        public class FixBiomeMsgWrapping
        {
            public static bool Prefix(MessageHud __instance)
            {
				if (__instance.m_biomeMsgInstance != null)
				{
					AnimatorStateInfo currentAnimatorStateInfo = __instance.m_biomeMsgInstance.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0);
					if (((AnimatorStateInfo)(currentAnimatorStateInfo)).IsTag("done"))
					{
						GameObject.Destroy(__instance.m_biomeMsgInstance);
						__instance.m_biomeMsgInstance = null;
					}
				}
				if (__instance.m_biomeFoundQueue.Count > 0 && __instance.m_biomeMsgInstance == null && __instance.m_msgQeue.Count == 0 && __instance.m_msgQueueTimer > 2f)
				{
					MessageHud.BiomeMessage biomeMessage = __instance.m_biomeFoundQueue.Dequeue();
					__instance.m_biomeMsgInstance = GameObject.Instantiate(__instance.m_biomeFoundPrefab, __instance.transform);
					Text component = Utils.FindChild(__instance.m_biomeMsgInstance.transform, "Title").GetComponent<Text>();
					string text = Localization.m_instance.Localize(biomeMessage.m_text);
					component.text = text;
					component.verticalOverflow = VerticalWrapMode.Overflow;
					component.horizontalOverflow = HorizontalWrapMode.Overflow;
					if (biomeMessage.m_playStinger && (bool)__instance.m_biomeFoundStinger)
					{
						GameObject.Instantiate(__instance.m_biomeFoundStinger);
					}
				}

				return false;
			}
        }
    }
}
