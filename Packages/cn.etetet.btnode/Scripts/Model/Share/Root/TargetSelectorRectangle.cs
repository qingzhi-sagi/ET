using Unity.Mathematics;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    [System.Serializable]
    public class TargetSelectorRectangle : TargetSelector
    {
#if UNITY
        [LabelText("技能指示器资源名")]
#endif
        public string SpellIndicatorName;

        public float Width;
        public float Length;
        public UnitType UnitType;
    }
}
