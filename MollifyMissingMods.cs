using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse; // ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

#if DEBUG
namespace CodeOptimist
{
    class MollifyMissingMods
    {
        // todo doesn't seem to find these automatically
        [HarmonyPatch(typeof(ThingFilter), nameof(ThingFilter.Allows), typeof(Thing))]
        static class ThingFilter_Allows_Patch
        {
            [HarmonyPriority(Priority.VeryHigh)]
            [HarmonyPrefix]
            static void AvoidNullReferenceException(ThingFilter __instance, List<SpecialThingFilterDef> ___disallowedSpecialFilters, Thing t) {
                void ReportRemoved(string message, int numRemoved) {
                    if (numRemoved > 0)
                        Debug.WriteLine(message);
                }

                ReportRemoved("Removed null disallowedSpecialFilters", ___disallowedSpecialFilters.RemoveAll(x => x == null));
                ReportRemoved(
                    "Removed disallowedSpecialFilters throwing NullReferenceException on Worker.Matches()", ___disallowedSpecialFilters.RemoveAll(
                        x => {
                            try {
                                x.Worker.Matches(t);
                            } catch (NullReferenceException) {
                                return true;
                            }

                            return false;
                        }));
            }
        }
    }
}
#endif
