namespace ET
{
    public class BTNumericCompare : BTCondition
    {
        public string OwnerKey;
        public int NumericType;
        public ConditionCompareOp Op;
        public long Value;
        public int ErrorCode;
    }
}
