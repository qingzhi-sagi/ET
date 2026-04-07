using Sirenix.OdinInspector;

namespace ET
{
    [System.Serializable]
    public class TargetSelectorSector : TargetSelector
    {
#if UNITY
        [LabelText("技能指示器资源名")]
#endif
        public string SpellIndicatorName;
        
        public int Radius;
        public int Angle;
        public UnitType UnitType;
    }
}
