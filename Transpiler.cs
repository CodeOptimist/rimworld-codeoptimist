using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse; // ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

namespace CodeOptimist
{
    public class CodeInstructionComparer : IEqualityComparer<CodeInstruction>
    {
        public bool Equals(CodeInstruction x, CodeInstruction y) {
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(null, y)) return false;
            if (ReferenceEquals(x,    y)) return true;
            if (!Equals(x.opcode, y.opcode)) return false;
            if (Equals(x.operand, y.operand)) return true;

            // consider null operands to be wildcards
            if (x.operand == null || y.operand == null)
                return true;
            return false;
        }

        public int GetHashCode(CodeInstruction obj) {
            return obj.GetHashCode();
        }
    }

    public static class TranspilerHelper
    {
        public static IEnumerable<CodeInstruction> ReplaceTypes(IEnumerable<CodeInstruction> codes, Dictionary<Type, Type> subs) {
            Debug.Assert(codes != null, "Is the patch being applied? Did you forget [HarmonyPatch] on the class?");
            var list = codes.ToList();
            foreach (var instruction in list) {
                if (instruction.operand is MethodInfo methodInfo && subs.TryGetValue(methodInfo.DeclaringType, out var to)) {
                    var paramTypes = methodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
                    var genericTypes = methodInfo.GetGenericArguments();
                    var replacement = methodInfo.IsGenericMethod
                        ? AccessTools.DeclaredMethod(to, methodInfo.Name, paramTypes, genericTypes)
                        : AccessTools.DeclaredMethod(to, methodInfo.Name, paramTypes);
                    if (replacement != null)
                        instruction.operand = replacement;
                }
            }
            return list.AsEnumerable();
        }

        public static string NameWithType(this MethodBase method, bool withNamespace = true) {
            // ReSharper disable PossibleNullReferenceException
            var typeName = withNamespace ? method.DeclaringType.FullName : method.DeclaringType.FullName.Substring(method.DeclaringType.Namespace.Length + 1);
            // ReSharper restore PossibleNullReferenceException
            return $"{typeName}.{method.Name}";
        }
    }

    public class Transpiler
    {
        public static readonly CodeInstructionComparer                      comparer       = new CodeInstructionComparer();
        readonly               Dictionary<int, List<List<CodeInstruction>>> indexesInserts = new Dictionary<int, List<List<CodeInstruction>>>();
        readonly               List<Patch>                                  neighbors      = new List<Patch>();
        readonly               MethodBase                                   originalMethod, patchMethod;
        public                 List<CodeInstruction>                        codes;
#if DEBUG
        public List<CodeInstruction> initialCodes;
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase originalMethod) {
            this.originalMethod = originalMethod;
            patchMethod = new StackFrame(1).GetMethod();

            var patches = Harmony.GetPatchInfo(originalMethod);
            if (patches != null)
                neighbors.AddRange(patches.Transpilers);

            Debug.WriteLine($"patchMethod: '{patchMethod.NameWithType(false)}'");
            Debug.WriteLine($"\toriginalMethod: '{originalMethod.NameWithType()}'");
            Debug.WriteLine($"\tneighbors: '{string.Join(", ", neighbors.Select(x => x.PatchMethod.NameWithType()))}'");
            codes = instructions.ToList();

#if DEBUG
            initialCodes = new List<CodeInstruction>();
            foreach (var code in codes)
                initialCodes.Add(new CodeInstruction(code));
#endif
        }

        public int MatchIdx  { get; private set; }
        public int InsertIdx { get; private set; }

        public int TryFindCodeIndex(Predicate<CodeInstruction> match) {
            return TryFindCodeIndex(0, match);
        }

        public int TryFindCodeIndex(int startIndex, Predicate<CodeInstruction> match) {
            return TryFind(match, () => codes.FindIndex(startIndex, match));
        }

        public int TryFindCodeLastIndex(Predicate<CodeInstruction> match) {
            return TryFindCodeLastIndex(codes.Count - 1, match);
        }

        public int TryFindCodeLastIndex(int startIndex, Predicate<CodeInstruction> match) {
            return TryFind(match, () => codes.FindLastIndex(startIndex, match));
        }

        int TryFind(Predicate<CodeInstruction> match, Func<int> resultFunc) {
            int result;
            try {
                result = resultFunc();
            } catch (Exception) {
                throw new CodeNotFoundException(match.Method, patchMethod, neighbors);
            }

            if (result == -1)
                throw new CodeNotFoundException(match.Method, patchMethod, neighbors);
            return result;
        }

        public bool TrySequenceEqual(int startIndex, List<CodeInstruction> sequence) {
            try {
                return codes.GetRange(startIndex, sequence.Count).SequenceEqual(sequence, comparer);
            } catch (Exception) {
                throw new CodeNotFoundException(sequence, patchMethod, neighbors);
            }
        }

        public int TryFindCodeSequence(List<CodeInstruction> sequence) {
            return TryFindCodeSequence(0, sequence);
        }

        // https://stackoverflow.com/a/12302013/879
        public int TryFindCodeSequence(int startIndex, List<CodeInstruction> sequence) {
            if (sequence.Count > codes.Count) return -1;
            try {
                return Enumerable.Range(startIndex, codes.Count - sequence.Count + 1).First(i => codes.Skip(i).Take(sequence.Count).SequenceEqual(sequence, comparer));
            } catch (InvalidOperationException) {
                throw new CodeNotFoundException(sequence, patchMethod, neighbors);
            }
        }

        public void TryInsertCodes(int offset, Func<int, List<CodeInstruction>, bool> match, Func<int, List<CodeInstruction>, List<CodeInstruction>> newCodes,
            bool bringLabels = false) {
            for (var i = MatchIdx; i < codes.Count; i++) {
                if (match(i, codes)) {
                    if (!indexesInserts.TryGetValue(i + offset, out var idxInserts)) {
                        idxInserts = new List<List<CodeInstruction>>();
                        indexesInserts.Add(i + offset, idxInserts);
                    }

                    var inserts = newCodes(i, codes);
                    if (bringLabels) {
                        inserts[0].labels.AddRange(codes[i + offset].labels);
                        codes[i + offset].labels.Clear();
                    }

                    idxInserts.Add(inserts);
                    MatchIdx = i;
                    InsertIdx = i + offset;
                    return;
                }
            }

            throw new CodeNotFoundException(match.Method, patchMethod, neighbors);
        }

        public IEnumerable<CodeInstruction> GetFinalCodes(bool debug = false) {
            var outCodes = new List<CodeInstruction>();
            for (var i = 0; i < codes.Count; i++) {
                if (indexesInserts.TryGetValue(i, out var idxInserts)) {
                    foreach (var inserts in idxInserts)
                        outCodes.AddRange(inserts);
                }

                outCodes.Add(codes[i]);
            }

#if DEBUG
            var bc4Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Beyond Compare 4\BComp.exe");
            if (debug && File.Exists(bc4Path)) {
                var leftTitle = $"{originalMethod.DeclaringType}::{originalMethod}";
                var rightTitle = $"{patchMethod.NameWithType(false)}";
                var leftTemp = Path.GetTempFileName();
                File.WriteAllLines(leftTemp, initialCodes.Select(x => x.ToString()));
                var rightTemp = Path.GetTempFileName();
                File.WriteAllLines(rightTemp, outCodes.Select(x => x.ToString()));
                var cmpProc = Process.Start(bc4Path, $"/nobackups /lefttitle=\"{leftTitle}\" /righttitle=\"{rightTitle}\" \"{leftTemp}\" \"{rightTemp}\"");
                if (cmpProc != null) {
                    cmpProc.Exited += (sender, args) => {
                        try {
                            File.Delete(leftTemp);
                            File.Delete(rightTemp);
                        } catch (Exception) {
                            // ignored
                        }
                    };
                    cmpProc.EnableRaisingEvents = true;
                }
            }
#endif

            return outCodes.AsEnumerable();
        }

        /*
        // dumb test
        [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
        static class FloatMenuMakerMap_AddHumanlikeOrders_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> CauseError(IEnumerable<CodeInstruction> _codes, MethodBase __originalMethod) {
                var t = new Common.Transpiler(_codes, __originalMethod);
                t.TryFindCodeIndex(1000000000, code => code.opcode == OpCodes.Brfalse);
                return t.GetFinalCodes();
            }
        }
        */

        // dynamic call example
        // var enabledFunc = new Func<bool>(() => enabled.Value);
        //                            new CodeInstruction(OpCodes.Ldarg_0),
        //                            new CodeInstruction(OpCodes.Ldftn, enabledFunc.Method),
        //                            new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(enabledFunc.GetType(), new[] {typeof(object), typeof(IntPtr)})),
        //                            new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(enabledFunc.GetType(), "Invoke")),

        class CodeNotFoundException : Exception
        {
            public CodeNotFoundException(List<CodeInstruction> sequence, MethodBase patchMethod, List<Patch> neighbors) : this(
                $"Unmatched sequence: {string.Join(", ", sequence.Select(x => x.ToString()))}",
                patchMethod, neighbors) {
            }

            public CodeNotFoundException(MethodInfo matchMethod, MethodBase patchMethod, List<Patch> neighbors) : this(
                $"Unmatched predicate: {BitConverter.ToString(matchMethod.GetMethodBody()?.GetILAsByteArray() ?? Array.Empty<byte>()).Replace("-", "")}",
                patchMethod, neighbors) {
            }

            CodeNotFoundException(string message, MethodBase patchMethod, List<Patch> neighbors) :
                base(message) {
                var modContent = LoadedModManager.RunningModsListForReading.First(x => x.assemblies.loadedAssemblies.Contains(patchMethod.DeclaringType?.Assembly));
                var neighborMods = neighbors.Select(
                        n => LoadedModManager.RunningModsListForReading.First(m => m.assemblies.loadedAssemblies.Contains(n.PatchMethod.DeclaringType?.Assembly)).Name)
                    .Distinct().ToList();

                Log.Warning($"[{modContent.Name}] You're welcome to 'Share logs' to my Discord: https://discord.gg/pnZGQAN \n");
                if (neighborMods.Any())
                    Log.Error($"[{modContent.Name}] Likely conflict with one of: {string.Join(", ", neighborMods)}");
            }
        }
    }
}
