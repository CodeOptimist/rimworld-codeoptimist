using System.Runtime.CompilerServices;
using Verse;

namespace CodeOptimist
{
    public static class Extensions
    {
        // `Resolve()` supports colored text e.g. `<Threat>text</Threat>`
        public static string ModTranslate(this string key, params NamedArgument[] args) => $"{Gui.modId}_{key}".Translate(args).Resolve();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Squared(this float val) => val * val;
    }
}
