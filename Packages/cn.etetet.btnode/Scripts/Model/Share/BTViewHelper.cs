using System.Collections.Generic;

namespace ET
{
    public static class BTViewHelper
    {
#if UNITY_EDITOR
        /// <summary>
        /// 获取用于Odin Inspector ValueDropdown的选项列表
        /// </summary>
        public static IEnumerable<Sirenix.OdinInspector.ValueDropdownItem<int>> GetEnumDropdownOptions<TEnum>(this TEnum tEnum) where TEnum : EnumSingleton<TEnum>
        {
            var options = new List<Sirenix.OdinInspector.ValueDropdownItem<int>>();
            var enumValues = tEnum.GetAllValues().GetAll();
            
            foreach ((int value, string name) in enumValues)
            {
                options.Add(new Sirenix.OdinInspector.ValueDropdownItem<int>($"{name} ({value})", value));
            }
            
            options.Sort((a, b) => a.Value.CompareTo(b.Value));
            return options;
        }
        
        /// <summary>
        /// 泛型方法，直接在 ValueDropdown 中使用：
        /// [ValueDropdown("@ET.BTViewHelper.GetOptions&lt;NumericTypeEnum&gt;()")]
        /// </summary>
        public static IEnumerable<Sirenix.OdinInspector.ValueDropdownItem<int>> GetOptions<T>() where T : EnumSingleton<T>
            => EnumSingleton<T>.Instance.GetEnumDropdownOptions();
#endif
    }
}
