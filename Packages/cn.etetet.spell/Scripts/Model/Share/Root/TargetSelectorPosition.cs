using MongoDB.Bson.Serialization.Attributes;
using Sirenix.OdinInspector;

namespace ET
{
    [System.Serializable]
    public class TargetSelectorPosition : TargetSelector
    {
        [Sirenix.OdinInspector.BoxGroup("输出参数")]
        [BTOutput(typeof(Unit))]
        public string Pos = "Pos";

        public int Radius;

#if UNITY
        [LabelText("技能指示器资源名")]
#endif
        public string SpellIndicatorName;
    }
}
