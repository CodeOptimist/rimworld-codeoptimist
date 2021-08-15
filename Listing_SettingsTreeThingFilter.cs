﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse; // ReSharper disable once RedundantUsingDirective
using Debug = System.Diagnostics.Debug;

namespace CodeOptimist
{
    [HarmonyPatch]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public class Listing_SettingsTreeThingFilter : Listing_Tree
    {
        static readonly Type from = typeof(Listing_TreeThingFilter);
        static readonly Type to   = typeof(Listing_SettingsTreeThingFilter);

        static readonly Color NoMatchColor = Color.grey;

        static readonly LRUCache<ValueTuple<TreeNode_ThingCategory, ThingFilter>, List<SpecialThingFilterDef>> cachedHiddenSpecialFilters =
            new LRUCache<ValueTuple<TreeNode_ThingCategory, ThingFilter>, List<SpecialThingFilterDef>>(500);

        public Listing_SettingsTreeThingFilter(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs,
            IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags, QuickSearchFilter searchFilter) {
            OriginalListing_TreeThingFilter(this, filter, parentFilter, forceHiddenDefs, forceHiddenFilters, suppressSmallVolumeTags, searchFilter);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(
            typeof(Listing_TreeThingFilter),            MethodType.Constructor, typeof(ThingFilter), typeof(ThingFilter), typeof(IEnumerable<ThingDef>),
            typeof(IEnumerable<SpecialThingFilterDef>), typeof(List<ThingDef>), typeof(QuickSearchFilter))]
        static void OriginalListing_TreeThingFilter(object instance, ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs,
            IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags, QuickSearchFilter searchFilter) {
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoThingDef))]
        static void OriginalDoThingDef(object instance, ThingDef tDef, int nestLevel, Map map) {
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.ListCategoryChildren))]
        public void ListCategoryChildren(TreeNode_ThingCategory node, int openMask, Map map, Rect visibleRect) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoCategoryChildren))]
        void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool subtreeMatchedSearch) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoSpecialFilter))]
        void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoCategory))]
        void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool subtreeMatchedSearch) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.AllowanceStateOf))]
        public MultiCheckboxState AllowanceStateOf(TreeNode_ThingCategory cat) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.Visible), typeof(ThingDef))]
        bool Visible(ThingDef td) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.IsOpen))]
        public override bool IsOpen(TreeNode node, int openMask) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.ThisOrDescendantsVisibleAndMatchesSearch))]
        bool ThisOrDescendantsVisibleAndMatchesSearch(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CategoryMatches))]
        bool CategoryMatches(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.Visible), typeof(TreeNode_ThingCategory))]
        bool Visible(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.Visible), typeof(SpecialThingFilterDef), typeof(TreeNode_ThingCategory))]
        bool Visible(SpecialThingFilterDef filter, TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CurrentRowVisibleOnScreen))]
        bool CurrentRowVisibleOnScreen() {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CalculateHiddenSpecialFilters), typeof(TreeNode_ThingCategory))]
        void CalculateHiddenSpecialFilters(TreeNode_ThingCategory node) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.GetCachedHiddenSpecialFilters))]
        static List<SpecialThingFilterDef> GetCachedHiddenSpecialFilters(TreeNode_ThingCategory node, ThingFilter parentFilter) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.CalculateHiddenSpecialFilters), typeof(TreeNode_ThingCategory), typeof(ThingFilter))]
        static List<SpecialThingFilterDef> CalculateHiddenSpecialFilters(TreeNode_ThingCategory node, ThingFilter parentFilter) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
            return default;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.ResetStaticData))]
        public static void ResetStaticData() {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes) => TranspilerHelper.ReplaceType(codes, from, to);
            var _ = Transpiler(null);
        }

        [HarmonyPatch(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoThingDef))]
        static class Listing_TreeThingFilter_DoThingDef_Patch
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.VeryHigh)]
            [HarmonyAfter("com.github.automatic1111.recipeicons")]
            static bool RecipeIconsPatchOnly(Listing_Tree __instance, ThingDef tDef, int nestLevel, Map map) {
                if (!(__instance is Listing_SettingsTreeThingFilter)) return true;
                OriginalDoThingDef(__instance, tDef, nestLevel, map);
                return false;
            }
        }

#pragma warning disable 169
        ThingFilter                 filter;
        ThingFilter                 parentFilter;
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
