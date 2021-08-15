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
    public class Logger
    {
        static readonly HashSet<string> openMethods = new HashSet<string>();
        static readonly Harmony         harmony     = new Harmony("CodeOptimist.Logger");


        static readonly List<Assembly> loopedAssemblies = new List<Assembly>();

        static readonly HashSet<MethodInfo> patchedMethods = new HashSet<MethodInfo>();

        static bool logging;

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
                harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(Logger), nameof(LogCall))));
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

        // ReSharper disable UnusedType.Local
        // ReSharper disable UnusedMember.Local

        [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
        static class Pawn_JobTracker_StartJob_Patch
        {
            [HarmonyPrefix]
            static void LogModsOfJobLoop(Pawn ___pawn) {
                if (___pawn.jobs.jobsGivenThisTick == 9) {
                    if (!patchedMethods.Any()) {
                        foreach (var method in Harmony.GetAllPatchedMethods()) {
                            try {
                                harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(Pawn_JobTracker_StartJob_Patch), nameof(LogMod))));
                            } catch {
                                // ignored
                            }
                        }
                    }

                    loopedAssemblies.Clear();
                    logging = true;
                } else if (___pawn.jobs.jobsGivenThisTick == 10) {
                    logging = false;

                    var loopedMods = loopedAssemblies.Select(
                        assembly => LoadedModManager.RunningModsListForReading.First(modContent => modContent.assemblies.loadedAssemblies.Contains(assembly)));
                    var loopedModNames = loopedMods.Select(x => x.Name).ToList();
                    loopedModNames.Sort();

                    Debug.WriteLine("JOB LOOPED MOD LIST:");
                    foreach (var modName in loopedModNames)
                        Debug.WriteLine($"\t{modName}");
                }
            }

            static void LogMod(MethodInfo __originalMethod) {
                if (!logging) return;
                if (patchedMethods.Contains(__originalMethod)) return;

                var patch = Harmony.GetPatchInfo(__originalMethod);
                foreach (var prefix in patch.Prefixes)
                    loopedAssemblies.AddDistinct(prefix.PatchMethod.DeclaringType?.Assembly);
                foreach (var postfix in patch.Postfixes)
                    loopedAssemblies.AddDistinct(postfix.PatchMethod.DeclaringType?.Assembly);
                foreach (var transpiler in patch.Transpilers)
                    loopedAssemblies.AddDistinct(transpiler.PatchMethod.DeclaringType?.Assembly);
                foreach (var finalizer in patch.Finalizers)
                    loopedAssemblies.AddDistinct(finalizer.PatchMethod.DeclaringType?.Assembly);

                patchedMethods.Add(__originalMethod);
            }
        }
    }
}
