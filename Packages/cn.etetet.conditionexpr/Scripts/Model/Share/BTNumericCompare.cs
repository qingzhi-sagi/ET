namespace ET
{
    public class BTNumericCompare : BTCondition
    {
        public string OwnerKey;
        public NumericType NumericType;
        public ConditionCompareOp Op;
        public long Value;
        public int ErrorCode;
    }
}
