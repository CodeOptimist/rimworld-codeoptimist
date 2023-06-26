using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;
// ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

namespace CodeOptimist
{
    public class Diagnostics
    {
        static readonly Harmony             harmony              = new("CodeOptimist.Diagnostics");
        static readonly HashSet<string>     openMethods          = new();
        static readonly List<Assembly>      loggedAssemblies     = new();
        static readonly HashSet<MethodInfo> loggedPatchedMethods = new();

        static bool logging;
        static bool patched;

        public static void LogAssembly(Assembly assembly) {
            foreach (var type in assembly.GetTypes())
                LogType(type);
        }

        public static void LogNamespace(string @namespace) {
            var types = LoadedModManager.RunningModsListForReading.SelectMany(x => x.assemblies.loadedAssemblies).SelectMany(x => x.GetTypes())
                .Where(t => t.IsClass && t.Namespace == @namespace);
            foreach (var type in types)
                LogType(type);
        }

        public static void LogNamespace(string modName, string @namespace) {
            var mod = LoadedModManager.RunningModsListForReading.FirstOrDefault(x => x.Name == modName);
            Debug.Assert(mod != null, nameof(mod) + " != null");
            var types = mod!.assemblies.loadedAssemblies.SelectMany(x => x.GetTypes()).Where(t => t.IsClass)
                .Where(t => t.Namespace == @namespace || (t.Namespace?.StartsWith($"{@namespace}.") ?? false));
            foreach (var type in types)
                LogType(type);
        }

        public static void LogType(Type type, List<string> excludedMethods = null, bool recursive = true) {
            Debug.WriteLine($"Patching: {type.Name}");
            foreach (var method in AccessTools.GetDeclaredMethods(type).Where(x => !excludedMethods?.Contains(x.Name) ?? true))
                LogMethod(method);

            if (!recursive) return;
            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                LogType(nestedType, excludedMethods);
        }

        public static void LogMethod(MethodBase method) {
            Debug.Assert(method.DeclaringType != null, "method.DeclaringType != null");
            try {
                harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(Diagnostics), nameof(LogCall))));
            } catch {
                // ignored
            }
        }

        static int? realMethodCallStackIdx;

        static void LogCall(MethodInfo __originalMethod) {
            var callStack = Environment.StackTrace.Split('\n');
            openMethods.IntersectWith(callStack);
            realMethodCallStackIdx ??= callStack.Skip(1).FirstIndexOf(s => !s.StartsWith($"  at {MethodBase.GetCurrentMethod()!.DeclaringType}")) + 1;
            openMethods.Add(callStack[(int)realMethodCallStackIdx]);

            // if (openMethods.Count == 1)
            //     Debug.WriteLine($"{Environment.StackTrace}");
            var tabs = new string('\t', (openMethods.Count - 1) * 2);
            Debug.WriteLine($" • {tabs}{__originalMethod.DeclaringType} -> {__originalMethod}");
        }

        public static void LogMods() {
            if (!patched) {
                foreach (var method in Harmony.GetAllPatchedMethods()) {
                    try {
                        harmony.Patch(method, new HarmonyMethod(AccessTools.Method(typeof(Diagnostics), nameof(LogMod))));
                    } catch {
                        // ignored
                    }
                }
                patched = true;
            }

            loggedPatchedMethods.Clear();
            loggedAssemblies.Clear();
            logging = true;
        }

        static void LogMod(MethodInfo __originalMethod) {
            if (!logging) return;
            LogCall(__originalMethod);

            if (loggedPatchedMethods.Contains(__originalMethod)) return;
            var patch = Harmony.GetPatchInfo(__originalMethod);
            foreach (var prefix in patch.Prefixes)
                loggedAssemblies.AddDistinct(prefix.PatchMethod.DeclaringType?.Assembly);
            foreach (var postfix in patch.Postfixes)
                loggedAssemblies.AddDistinct(postfix.PatchMethod.DeclaringType?.Assembly);
            foreach (var transpiler in patch.Transpilers)
                loggedAssemblies.AddDistinct(transpiler.PatchMethod.DeclaringType?.Assembly);
            foreach (var finalizer in patch.Finalizers)
                loggedAssemblies.AddDistinct(finalizer.PatchMethod.DeclaringType?.Assembly);
            loggedPatchedMethods.Add(__originalMethod);
        }

        static readonly HashSet<string> ongoingLoggedMods = new();

        public static void ReportOngoingMods() {
            logging = false;

            var loggedContentPacks = loggedAssemblies.Select(
                assembly => LoadedModManager.RunningModsListForReading.First(modContent => modContent.assemblies.loadedAssemblies.Contains(assembly)));
            var loggedMods = loggedContentPacks.Select(x => x.Name).ToList();
            loggedMods.Sort();
            if (ongoingLoggedMods.Count == 0)
                ongoingLoggedMods.AddRange(loggedMods);
            else
                ongoingLoggedMods.IntersectWith(loggedMods);

            Debug.WriteLine("LOGGED MOD LIST:");
            foreach (var modName in ongoingLoggedMods)
                Debug.WriteLine($"\t{modName}");
        }
    }
}
