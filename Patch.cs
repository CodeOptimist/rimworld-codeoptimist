using HarmonyLib;
using Verse;
using Verse.AI;
// ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

// for Harmony patches
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace CodeOptimist
{
    public static class Patch
    {
        public static readonly Harmony harmony = new("CodeOptimist");

        // Harmony patch syntactic sugar to visually distinguish these special cases from actual return true/false
        public static bool Continue(object _ = null) => true;
        public static bool Halt(object _ = null)     => false;
    }

#if DEBUG
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    static class Pawn_JobTracker_StartJob_Patch
    {
        [HarmonyPrefix]
        static void LogModsOfJobLoop(Pawn ___pawn) {
            if (___pawn.jobs.jobsGivenThisTick == 9)
                Debugger.LogMods();
            else if (___pawn.jobs.jobsGivenThisTick == 10)
                Debugger.ReportMods();
        }
    }
#endif
}
