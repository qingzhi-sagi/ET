namespace ET
{
    public class BTNumericChange : BTAction
    {
        [BTInput(typeof(Unit))]
        [Sirenix.OdinInspector.BoxGroup("输入参数")]
        public string Unit;
        
        [BTInput(typeof(Buff))]
        [Sirenix.OdinInspector.BoxGroup("输入参数")]
        public string Buff;
        
        [Sirenix.OdinInspector.LabelText("数值类型")]
        [BTIntDropdown(typeof(ET.NumericTypeEnum))]
        public int NumericType;
        
        [Sirenix.OdinInspector.LabelText("数值")]
        public int Value;

    }
}
