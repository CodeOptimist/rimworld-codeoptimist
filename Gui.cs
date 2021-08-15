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
        readonly Color guiColor;

        readonly TextAnchor textAnchor;

        readonly GameFont textFont;

        public DrawContext() {
            guiColor = GUI.color;
            textFont = Text.Font;
            textAnchor = Text.Anchor;
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

        public void Dispose() {
            GUI.color = guiColor;
            Text.Font = textFont;
            Text.Anchor = textAnchor;
        }
    }

    public static class Gui
    {
        public static string modId;

        public static void DrawBool(this Listing_Standard list, ref bool value, string name) {
            list.CheckboxLabeled($"{modId}_SettingTitle_{name}".Translate(), ref value, $"{modId}_SettingDesc_{name}".Translate());
        }

        static void NumberLabel(this Listing_Standard list, Rect rect, float value, string name, out string buffer) {
            Widgets.Label(new Rect(rect.x, rect.y, rect.width - 8, rect.height), $"{modId}_SettingTitle_{name}".Translate());
            buffer = value.ToString(CultureInfo.InvariantCulture);
            list.Gap(list.verticalSpacing);

            var tooltip = $"{modId}_SettingDesc_{name}".Translate();
            if (!tooltip.NullOrEmpty()) {
                if (Mouse.IsOver(rect))
                    Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }

        public static void DrawFloat(this Listing_Standard list, ref float value, string name) {
            var rect = list.GetRect(Text.LineHeight);
            list.NumberLabel(rect.LeftHalf(), value, name, out var buffer);
            Widgets.TextFieldNumeric(rect.RightHalf(), ref value, ref buffer, 0f, 999f);
        }

        public static void DrawInt(this Listing_Standard list, ref int value, string name) {
            var rect = list.GetRect(Text.LineHeight);
            list.NumberLabel(rect.LeftHalf(), value, name, out var buffer);
            Widgets.IntEntry(rect.RightHalf(), ref value, ref buffer);
        }

        public static void DrawEnum<T>(this Listing_Standard list, T value, string name, Action<T> setValue) {
            var rect = list.GetRect(30f);
            var tooltip = $"{modId}_SettingDesc_{name}".Translate();
            if (!tooltip.NullOrEmpty()) {
                if (Mouse.IsOver(rect.LeftHalf()))
                    Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect.LeftHalf(), tooltip);
            }

            Widgets.Label(rect.LeftHalf(), $"{modId}_SettingTitle_{name}".Translate());
            var valueName = Enum.GetName(typeof(T), value);
            if (Widgets.ButtonText(rect.RightHalf(), $"{modId}_SettingTitle_{name}_{valueName}".Translate())) {
                var menuOptions = new List<FloatMenuOption>();
                foreach (var enumValue in Enum.GetValues(typeof(T)).Cast<T>()) {
                    var enumValueName = Enum.GetName(typeof(T), enumValue);
                    menuOptions.Add(new FloatMenuOption($"{modId}_SettingTitle_{name}_{enumValueName}".Translate(), () => { setValue(enumValue); }));
                }

                Find.WindowStack.Add(new FloatMenu(menuOptions.ToList()));
            }

            list.Gap(list.verticalSpacing);
        }
    }
}
