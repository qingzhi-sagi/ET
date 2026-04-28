using System;

namespace ET
{
    public static class ConditionCompareHelper
    {
        public static bool Compare(long left, ConditionCompareOp op, long right)
        {
            return op switch
            {
                ConditionCompareOp.Greater => left > right,
                ConditionCompareOp.GreaterEqual => left >= right,
                ConditionCompareOp.Less => left < right,
                ConditionCompareOp.LessEqual => left <= right,
                ConditionCompareOp.Equal => left == right,
                ConditionCompareOp.NotEqual => left != right,
                _ => throw new Exception($"unknown condition compare op: {op}")
            };
        }
    }
}
