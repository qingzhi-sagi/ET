#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 在行为树编辑器中，用 GenericSelector 替代默认枚举下拉，规避 Unity EnumPopup 卡死问题。
    /// </summary>
    public sealed class BTEnumDropdownDrawer : OdinValueDrawer<Enum>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Type enumType = this.ValueEntry?.TypeOfValue;
            if (!BTNodeDrawer.IsDrawingInBehaviorTreeEditor || enumType == null || !enumType.IsEnum)
            {
                this.CallNextDrawer(label);
                return;
            }

            List<EnumOption> options = BuildOptions(enumType);
            bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);
            Enum currentValue = this.ValueEntry.SmartValue;

            Rect rect = EditorGUILayout.GetControlRect();
            Rect contentRect = EditorGUI.PrefixLabel(rect, label);
            GUIContent content = new(GetDisplayText(enumType, currentValue, options, isFlags));

            if (!EditorGUI.DropdownButton(contentRect, content, FocusType.Keyboard))
            {
                return;
            }

            if (isFlags)
            {
                ShowFlagsSelector(enumType, currentValue, options, contentRect);
                return;
            }

            ShowSingleSelector(enumType, currentValue, options);
        }

        private void ShowSingleSelector(Type enumType, Enum currentValue, List<EnumOption> options)
        {
            List<string> names = options.Select(option => option.Name).ToList();
            GenericSelector<string> selector = new("Enum", false, name => GetOptionLabel(options, name), names);

            if (currentValue != null)
            {
                string currentName = Enum.GetName(enumType, currentValue);
                if (!string.IsNullOrEmpty(currentName))
                {
                    selector.SetSelection(currentName);
                }
            }

            selector.SelectionTree.Config.DrawSearchToolbar = true;
            selector.SelectionTree.Config.SearchToolbarHeight = 22;
            selector.SelectionConfirmed += selection =>
            {
                string selectedName = selection.FirstOrDefault();
                if (string.IsNullOrEmpty(selectedName))
                {
                    return;
                }

                this.ValueEntry.WeakSmartValue = Enum.Parse(enumType, selectedName);
            };

            selector.ShowInPopup();
        }

        private void ShowFlagsSelector(Type enumType, Enum currentValue, List<EnumOption> options, Rect anchorRect)
        {
            ulong currentBits = ToUInt64(currentValue, enumType);
            Rect popupRect = new(anchorRect.x, anchorRect.yMax, Mathf.Max(anchorRect.width, 1f), 1f);
            PopupWindow.Show(popupRect, new FlagsMultiSelectPopup(options, currentBits, selectedBits =>
            {
                this.ValueEntry.WeakSmartValue = CreateEnumValue(enumType, selectedBits);
            }));
        }

        private static List<EnumOption> BuildOptions(Type enumType)
        {
            List<EnumOption> options = new();
            Array values = Enum.GetValues(enumType);
            foreach (object raw in values)
            {
                Enum enumValue = (Enum)raw;
                string name = Enum.GetName(enumType, enumValue);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                string label = GetMemberLabel(enumType, name);
                options.Add(new EnumOption(name, label, ToUInt64(enumValue, enumType)));
            }

            return options;
        }

        private static string GetDisplayText(Type enumType, Enum currentValue, List<EnumOption> options, bool isFlags)
        {
            if (currentValue == null)
            {
                return "请选择";
            }

            if (!isFlags)
            {
                string name = Enum.GetName(enumType, currentValue);
                if (string.IsNullOrEmpty(name))
                {
                    return currentValue.ToString();
                }

                return GetOptionLabel(options, name);
            }

            ulong bits = ToUInt64(currentValue, enumType);
            List<string> labels = new();
            foreach (string name in GetSelectedFlags(bits, options))
            {
                labels.Add(GetOptionLabel(options, name));
            }

            return labels.Count == 0 ? currentValue.ToString() : string.Join(" | ", labels);
        }

        private static List<string> GetSelectedFlags(ulong bits, List<EnumOption> options)
        {
            List<string> selected = new();
            if (bits == 0UL)
            {
                EnumOption zeroOption = options.FirstOrDefault(option => option.RawValue == 0UL);
                if (zeroOption != null)
                {
                    selected.Add(zeroOption.Name);
                }

                return selected;
            }

            foreach (EnumOption option in options)
            {
                if (option.RawValue == 0UL)
                {
                    continue;
                }

                if ((bits & option.RawValue) == option.RawValue)
                {
                    selected.Add(option.Name);
                }
            }

            return selected;
        }

        private static string GetOptionLabel(List<EnumOption> options, string name)
        {
            EnumOption option = options.FirstOrDefault(item => item.Name == name);
            return option?.Label ?? name;
        }

        private static string GetMemberLabel(Type enumType, string memberName)
        {
            MemberInfo member = enumType.GetMember(memberName).FirstOrDefault();
            LabelTextAttribute labelAttribute = member?.GetCustomAttribute<LabelTextAttribute>(false);
            string label = labelAttribute?.Text;
            if (string.IsNullOrEmpty(label))
            {
                return memberName;
            }

            return $"{label} ({memberName})";
        }

        private static ulong ToUInt64(Enum value, Type enumType)
        {
            if (value == null || enumType == null)
            {
                return 0UL;
            }

            Type underlyingType = Enum.GetUnderlyingType(enumType);
            if (IsSigned(underlyingType))
            {
                long signed = Convert.ToInt64(value);
                return unchecked((ulong)signed);
            }

            return Convert.ToUInt64(value);
        }

        private static Enum CreateEnumValue(Type enumType, ulong rawValue)
        {
            Type underlyingType = Enum.GetUnderlyingType(enumType);
            object typedValue;
            if (underlyingType == typeof(byte))
            {
                typedValue = unchecked((byte)rawValue);
            }
            else if (underlyingType == typeof(sbyte))
            {
                typedValue = unchecked((sbyte)rawValue);
            }
            else if (underlyingType == typeof(short))
            {
                typedValue = unchecked((short)rawValue);
            }
            else if (underlyingType == typeof(ushort))
            {
                typedValue = unchecked((ushort)rawValue);
            }
            else if (underlyingType == typeof(int))
            {
                typedValue = unchecked((int)rawValue);
            }
            else if (underlyingType == typeof(uint))
            {
                typedValue = unchecked((uint)rawValue);
            }
            else if (underlyingType == typeof(long))
            {
                typedValue = unchecked((long)rawValue);
            }
            else
            {
                typedValue = rawValue;
            }

            return (Enum)Enum.ToObject(enumType, typedValue);
        }

        private static bool IsSigned(Type type)
        {
            return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long);
        }

        private sealed class EnumOption
        {
            public EnumOption(string name, string label, ulong rawValue)
            {
                this.Name = name;
                this.Label = label;
                this.RawValue = rawValue;
            }

            public string Name { get; }
            public string Label { get; }
            public ulong RawValue { get; }
        }

        private sealed class FlagsMultiSelectPopup : PopupWindowContent
        {
            private readonly List<EnumOption> options;
            private readonly Action<ulong> onConfirm;
            private string searchText = string.Empty;
            private Vector2 scrollPosition;
            private ulong selectedBits;

            public FlagsMultiSelectPopup(List<EnumOption> options, ulong currentBits, Action<ulong> onConfirm)
            {
                this.options = options ?? new List<EnumOption>();
                this.selectedBits = currentBits;
                this.onConfirm = onConfirm;
            }

            public override Vector2 GetWindowSize()
            {
                int visibleRows = Mathf.Clamp(this.options.Count, 4, 12);
                float height = 84f + visibleRows * 20f;
                return new Vector2(340f, height);
            }

            public override void OnGUI(Rect rect)
            {
                DrawToolbar();
                DrawOptions();
                DrawFooter();
            }

            private void DrawToolbar()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("搜索", GUILayout.Width(32f));
                this.searchText = EditorGUILayout.TextField(this.searchText);
                EditorGUILayout.EndHorizontal();
            }

            private void DrawOptions()
            {
                this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

                foreach (EnumOption option in this.options)
                {
                    if (!IsVisible(option, this.searchText))
                    {
                        continue;
                    }

                    bool selected = IsSelected(option);
                    bool changed = DrawToggle(option, selected, out bool newSelected);
                    if (!changed)
                    {
                        continue;
                    }

                    ApplySelection(option, newSelected);
                }

                EditorGUILayout.EndScrollView();
            }

            private void DrawFooter()
            {
                GUILayout.Space(4f);
                if (GUILayout.Button("确定", GUILayout.Height(24f)))
                {
                    this.onConfirm?.Invoke(this.selectedBits);
                    this.editorWindow.Close();
                }
            }

            private static bool IsVisible(EnumOption option, string searchText)
            {
                if (option == null)
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return true;
                }

                return option.Label.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                    || option.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            private bool IsSelected(EnumOption option)
            {
                if (option.RawValue == 0UL)
                {
                    return this.selectedBits == 0UL;
                }

                return (this.selectedBits & option.RawValue) == option.RawValue;
            }

            private static bool DrawToggle(EnumOption option, bool selected, out bool newSelected)
            {
                newSelected = EditorGUILayout.ToggleLeft(option.Label, selected);
                return newSelected != selected;
            }

            private void ApplySelection(EnumOption option, bool selected)
            {
                if (option.RawValue == 0UL)
                {
                    if (selected)
                    {
                        this.selectedBits = 0UL;
                    }

                    return;
                }

                if (selected)
                {
                    this.selectedBits |= option.RawValue;
                    return;
                }

                this.selectedBits &= ~option.RawValue;
            }
        }
    }
}
#endif
