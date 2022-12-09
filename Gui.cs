using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Verse;

namespace CodeOptimist
{
    public class DrawContext : IDisposable
    {
        public static float guiLabelPct = 0.5f;

        readonly Color      guiColor;
        readonly TextAnchor textAnchor;
        readonly GameFont   textFont;
        readonly float      labelPct;

        public DrawContext() {
            guiColor   = GUI.color;
            textFont   = Text.Font;
            textAnchor = Text.Anchor;
            labelPct   = guiLabelPct;
        }

        public void Dispose() {
            GUI.color   = guiColor;
            Text.Font   = textFont;
            Text.Anchor = textAnchor;
            guiLabelPct = labelPct;
        }

        public Color GuiColor {
            set => GUI.color = value;
        }

        public GameFont TextFont {
            set => Text.Font = value;
        }

        public TextAnchor TextAnchor {
            set => Text.Anchor = value;
        }

        public float LabelPct {
            set => guiLabelPct = value;
        }
    }

    public static class Gui
    {
        public static string modId;

        public static string Title(this string name) => $"{modId}_SettingTitle_{name}".Translate().Resolve();
        public static string Desc(this string name)  => $"{modId}_SettingDesc_{name}".Translate().Resolve();

        public static void DrawBool(this Listing_Standard list, ref bool value, string name) {
            list.CheckboxLabeled(name.Title(), ref value, name.Desc());
        }

        static void NumberLabel(this Listing_Standard list, Rect rect, float value, string format, string name, out string buffer) {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width - 8, rect.height), name.Title());
            buffer = value.ToString(format);
            list.Gap(list.verticalSpacing);

            var tooltip = name.Desc();
            if (!tooltip.NullOrEmpty()) {
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static void DrawFloat(this Listing_Standard list, ref float value, string name) {
            var rect = list.GetRect(Text.LineHeight);
            list.NumberLabel(rect.LeftPart(DrawContext.guiLabelPct), value, "f1", name, out var buffer);
            Widgets.TextFieldNumeric(rect.RightPart(1 - DrawContext.guiLabelPct), ref value, ref buffer, 0f, 999f);
        }

        public static void DrawInt(this Listing_Standard list, ref int value, string name) {
            var rect = list.GetRect(Text.LineHeight);
            list.NumberLabel(rect.LeftPart(DrawContext.guiLabelPct), value, "n0", name, out var buffer);
            Widgets.IntEntry(rect.RightPart(1 - DrawContext.guiLabelPct), ref value, ref buffer);
        }

        public static void DrawEnum<T>(this Listing_Standard list, T value, string name, Action<T> setValue, float height = 30f) {
            var rect    = list.GetRect(height);
            var tooltip = name.Desc();
            if (!tooltip.NullOrEmpty()) {
                if (Mouse.IsOver(rect.LeftPart(DrawContext.guiLabelPct)))
                    Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect.LeftPart(DrawContext.guiLabelPct), tooltip);
            }

            Widgets.Label(rect.LeftPart(DrawContext.guiLabelPct), name.Title());
            var valueName = Enum.GetName(typeof(T), value);
            if (Widgets.ButtonText(rect.RightPart(1 - DrawContext.guiLabelPct), $"{name}_{valueName}".Title())) {
                var menuOptions = new List<FloatMenuOption>();
                foreach (var enumValue in Enum.GetValues(typeof(T)).Cast<T>()) {
                    var enumValueName = Enum.GetName(typeof(T), enumValue);
                    menuOptions.Add(new FloatMenuOption($"{name}_{enumValueName}".Title(), () => { setValue(enumValue); }));
                }

                Find.WindowStack.Add(new FloatMenu(menuOptions.ToList()));
            }

            list.Gap(list.verticalSpacing);
        }
    }
}
