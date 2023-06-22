using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
// ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

// for Harmony patches
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace CodeOptimist
{
    [HarmonyPatch]
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public class Listing_SettingsTreeThingFilter : Listing_Tree
    {
        static readonly Dictionary<Type, Type> _subs = new() {
            { typeof(Listing_TreeThingFilter), typeof(Listing_SettingsTreeThingFilter) },
            { typeof(ThingFilter), typeof(SettingsThingFilter) },
        };

        static readonly Color NoMatchColor = Color.grey;

        static readonly LRUCache<ValueTuple<TreeNode_ThingCategory, SettingsThingFilter>, List<SpecialThingFilterDef>> cachedHiddenSpecialFilters = new(500);

        public Listing_SettingsTreeThingFilter(SettingsThingFilter filter, SettingsThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs,
            IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags, QuickSearchFilter searchFilter) {
            _construct_Listing_TreeThingFilter(this, filter, parentFilter, forceHiddenDefs, forceHiddenFilters, suppressSmallVolumeTags, searchFilter);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(
            typeof(Listing_TreeThingFilter),            MethodType.Constructor, typeof(ThingFilter), typeof(ThingFilter), typeof(IEnumerable<ThingDef>),
            typeof(IEnumerable<SpecialThingFilterDef>), typeof(List<ThingDef>), typeof(QuickSearchFilter))]
        static void _construct_Listing_TreeThingFilter(object instance, SettingsThingFilter filter, SettingsThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs,
            IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags, QuickSearchFilter searchFilter) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoThingDef))]
        static void OriginalDoThingDef(object instance, ThingDef tDef, int nestLevel, Map map) {
        }

        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoThingDef))]
        static class Listing_TreeThingFilter_DoThingDef_Patch
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.VeryHigh)]
            [HarmonyAfter("com.github.automatic1111.recipeicons")]
            static bool RecipeIconsPatchOnly(Listing_Tree __instance, ThingDef tDef, int nestLevel, Map map) {
                if (!(__instance is Listing_SettingsTreeThingFilter)) return Patch.Continue();
                OriginalDoThingDef(__instance, tDef, nestLevel, map);
                return Patch.Halt();
            }
        }

    #region vanilla but replaced type
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.ListCategoryChildren))]
        public void ListCategoryChildren(TreeNode_ThingCategory node, int openMask, Map map, Rect visibleRect) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoCategoryChildren))]
        void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool subtreeMatchedSearch) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoSpecialFilter))]
        void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoCategory))]
        void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool subtreeMatchedSearch) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.AllowanceStateOf))]
        public MultiCheckboxState AllowanceStateOf(TreeNode_ThingCategory cat) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.Visible), typeof(ThingDef))]
        bool Visible(ThingDef td) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.IsOpen))]
        public override bool IsOpen(TreeNode node, int openMask) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.ThisOrDescendantsVisibleAndMatchesSearch))]
        bool ThisOrDescendantsVisibleAndMatchesSearch(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CategoryMatches))]
        bool CategoryMatches(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.Visible), typeof(TreeNode_ThingCategory))]
        bool Visible(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.Visible), typeof(SpecialThingFilterDef), typeof(TreeNode_ThingCategory))]
        bool Visible(SpecialThingFilterDef filter, TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CurrentRowVisibleOnScreen))]
        bool CurrentRowVisibleOnScreen() {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CalculateHiddenSpecialFilters), typeof(TreeNode_ThingCategory))]
        void CalculateHiddenSpecialFilters(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.GetCachedHiddenSpecialFilters))]
        static List<SpecialThingFilterDef> GetCachedHiddenSpecialFilters(TreeNode_ThingCategory node, SettingsThingFilter parentFilter) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CalculateHiddenSpecialFilters), typeof(TreeNode_ThingCategory), typeof(ThingFilter))]
        static List<SpecialThingFilterDef> CalculateHiddenSpecialFilters(TreeNode_ThingCategory node, SettingsThingFilter parentFilter) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.ResetStaticData))]
        public static void ResetStaticData() {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceTypes(codes, _subs);
            var _ = Transpiler(null);
        }
    #endregion

#pragma warning disable 169
        SettingsThingFilter         filter;
        SettingsThingFilter         parentFilter;
        List<SpecialThingFilterDef> hiddenSpecialFilters;
        List<ThingDef>              forceHiddenDefs;
        List<SpecialThingFilterDef> tempForceHiddenSpecialFilters;
        List<ThingDef>              suppressSmallVolumeTags;
        protected QuickSearchFilter searchFilter;
        public    int               matchCount;
        Rect                        visibleRect;
#pragma warning restore 169
    }
}
