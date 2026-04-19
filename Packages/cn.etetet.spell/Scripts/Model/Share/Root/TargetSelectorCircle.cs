using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    [System.Serializable]
    public class TargetSelectorCircle : TargetSelector
    {
        [Sirenix.OdinInspector.BoxGroup("输出参数")]
        [BTOutput(typeof(Unit))]
        public string Unit = "Unit";
        
        [Sirenix.OdinInspector.BoxGroup("输出参数")]
        [BTOutput(typeof(List<long>))]
        public string Units = "Units";
        
        public int Radius;
        public UnitType UnitType;

#if UNITY
        [LabelText("技能指示器资源名")]
#endif
        public string SpellIndicatorName;
    }
}
