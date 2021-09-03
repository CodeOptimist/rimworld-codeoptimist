using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI; // ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

namespace CodeOptimist
{
    public class Debugger
    {
        static readonly HashSet<string> openMethods = new HashSet<string>();
        static readonly Harmony         harmony     = new Harmony("CodeOptimist.Logger");


        static readonly List<Assembly> loggedAssemblies = new List<Assembly>();

        static readonly HashSet<MethodInfo> loggedMethods = new HashSet<MethodInfo>();

        static bool logging;
        static bool patched;

        public static void LogAssembly(Assembly assembly) {
            foreach (var type in assembly.GetTypes())
                LogType(type);
        }

        public static void LogType(Type type, bool recursive = true) {
            foreach (var method in AccessTools.GetDeclaredMethods(type))
                LogMethod(method);

            if (!recursive) return;
            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                LogType(nestedType);
        }

        public static void LogMethod(MethodBase method) {
            Debug.Assert(method.DeclaringType != null, "method.DeclaringType != null");
            Debug.WriteLine($"Patching: {method.DeclaringType.Name} -> {method.Name}");
            try {
                harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(Debugger), nameof(LogCall))));
            } catch {
                // ignored
            }
        }

        static void LogCall(MethodInfo __originalMethod) {
            var callStack = Environment.StackTrace.Split('\n');
            openMethods.IntersectWith(callStack);
            openMethods.Add(callStack[2]);

            var tabs = new string('\t', (openMethods.Count - 1) * 2);
            Debug.WriteLine($"{RealTime.frameCount} • {tabs}{__originalMethod.DeclaringType} -> {__originalMethod.Name}");
        }

        public static void LogMods() {
            if (!patched) {
                foreach (var method in Harmony.GetAllPatchedMethods()) {
                    try {
                        harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(Debugger), nameof(LogMod))));
                    } catch {
                        // ignored
                    }
                }
                patched = true;
            }

            loggedMethods.Clear();
            loggedAssemblies.Clear();
            logging = true;
        }

        static void LogMod(MethodInfo __originalMethod) {
            if (!logging) return;
            LogCall(__originalMethod);
            if (loggedMethods.Contains(__originalMethod)) return;

            var patch = Harmony.GetPatchInfo(__originalMethod);
            foreach (var prefix in patch.Prefixes)
                loggedAssemblies.AddDistinct(prefix.PatchMethod.DeclaringType?.Assembly);
            foreach (var postfix in patch.Postfixes)
                loggedAssemblies.AddDistinct(postfix.PatchMethod.DeclaringType?.Assembly);
            foreach (var transpiler in patch.Transpilers)
                loggedAssemblies.AddDistinct(transpiler.PatchMethod.DeclaringType?.Assembly);
            foreach (var finalizer in patch.Finalizers)
                loggedAssemblies.AddDistinct(finalizer.PatchMethod.DeclaringType?.Assembly);

            loggedMethods.Add(__originalMethod);
        }

        public static void ReportMods() {
            logging = false;

            var loggedMods = loggedAssemblies.Select(
                assembly => LoadedModManager.RunningModsListForReading.First(modContent => modContent.assemblies.loadedAssemblies.Contains(assembly)));
            var loggedModNames = loggedMods.Select(x => x.Name).ToList();
            loggedModNames.Sort();

            Debug.WriteLine("LOGGED MOD LIST:");
            foreach (var modName in loggedModNames)
                Debug.WriteLine($"\t{modName}");
        } // ReSharper disable UnusedType.Local
        // ReSharper disable UnusedMember.Local

        [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
        static class Pawn_JobTracker_StartJob_Patch
        {
            [HarmonyPrefix]
            static void LogModsOfJobLoop(Pawn ___pawn) {
                if (___pawn.jobs.jobsGivenThisTick == 9)
                    LogMods();
                else if (___pawn.jobs.jobsGivenThisTick == 10)
                    ReportMods();
            }
        }
    }
}
