namespace ET
{
    public class BTCostNumeric: BTAction
    {
        [BTInput(typeof(Unit))]
        public string Caster = "Caster";
        
        [BTInput(typeof(bool))]
        public string Check;
        
        [Sirenix.OdinInspector.LabelText("数值类型")]
        public NumericType NumericType;
        
        public long Value;
        
    }
}
