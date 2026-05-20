namespace ET
{
    [EntitySystemOf(typeof(NumericComponent))]
    public static partial class NumericComponentSystem
    {
        [EntitySystem]
        private static void Awake(this NumericComponent self)
        {
            self.GetParent<Unit>().NumericComponent = self;
        }
        
        [EntitySystem]
        private static void Deserialize(this NumericComponent self)
        {
            self.GetParent<Unit>().NumericComponent = self;
        }

        public const int Max = 10000;

        public static void CopyTo(this NumericComponent self, NumericComponent to)
        {
            foreach (var kv in self.NumericDic)
            {
                to.NumericDic[kv.Key] = kv.Value;
            }
        }

        public static float GetAsFloat(this NumericComponent self, int numericType)
        {
            return self.GetAsFloat((NumericType)numericType);
        }

        public static float GetAsFloat(this NumericComponent self, NumericType numericType)
        {
            return (float)self.GetByKey(numericType) / 1000;
        }

        public static int Get(this NumericComponent self, int numericType)
        {
            return self.Get((NumericType)numericType);
        }

        public static int Get(this NumericComponent self, NumericType numericType)
        {
            return (int)self.GetByKey(numericType);
        }

        public static int GetAsInt(this NumericComponent self, int numericType)
        {
            return self.GetAsInt((NumericType)numericType);
        }

        public static int GetAsInt(this NumericComponent self, NumericType numericType)
        {
            return (int)self.GetByKey(numericType);
        }

        public static long GetAsLong(this NumericComponent self, int numericType)
        {
            return self.GetAsLong((NumericType)numericType);
        }

        public static long GetAsLong(this NumericComponent self, NumericType numericType)
        {
            return self.GetByKey(numericType);
        }

        public static void Set(this NumericComponent self, int nt, float value)
        {
            self.Set((NumericType)nt, value);
        }

        public static void Set(this NumericComponent self, NumericType nt, float value)
        {
            self.Insert(nt, (long)(value * 1000));
        }

        public static void Set(this NumericComponent self, int nt, int value)
        {
            self.Set((NumericType)nt, value);
        }

        public static void Set(this NumericComponent self, NumericType nt, int value)
        {
            self.Insert(nt, value);
        }

        public static void Set(this NumericComponent self, int nt, long value)
        {
            self.Set((NumericType)nt, value);
        }

        public static void Set(this NumericComponent self, NumericType nt, long value)
        {
            self.Insert(nt, value);
        }

        public static void SetNoEvent(this NumericComponent self, int numericType, long value)
        {
            self.SetNoEvent((NumericType)numericType, value);
        }

        public static void SetNoEvent(this NumericComponent self, NumericType numericType, long value)
        {
            self.Insert(numericType, value, false);
        }

        public static void Insert(this NumericComponent self, int numericType, long value, bool isPublicEvent = true)
        {
            self.Insert((NumericType)numericType, value, isPublicEvent);
        }

        public static void Insert(this NumericComponent self, NumericType numericType, long value, bool isPublicEvent = true)
        {
            long oldValue = self.GetByKey(numericType);
            if (oldValue == value)
            {
                return;
            }

            self.NumericDic[numericType] = value;

            int numericTypeValue = (int)numericType;
            if (numericTypeValue >= Max)
            {
                self.Update(numericType, isPublicEvent);
                return;
            }

            // 如果有最大值，需要限制最大值
            NumericTypeConfig numericTypeConfig = self.Fiber().GetSingleton<NumericTypeConfigCategory>().Get(numericTypeValue);
            if (numericTypeConfig.MaxNumericType > 0)
            {
                long max = self.GetByKey(numericTypeConfig.MaxNumericType);
                if (value > max)
                {
                    value = max;
                }
            }

            // 影响其它数值
            if (numericTypeConfig.AffectNumeric.Count > 0)
            {
                foreach (var kv in numericTypeConfig.AffectNumeric)
                {
                    // 影响的值必须大于Max，这样才能恢复回去
                    if (kv.Key < Max)
                    {
                        Log.Error($"AffectNumericType is {kv.Key}, numericType is {numericTypeConfig.Id}");
                        continue;
                    }
                    NumericType affectNumericType = (NumericType)kv.Key;
                    long affectOldValue = self.GetByKey(affectNumericType);
                    self.Insert(affectNumericType, affectOldValue + kv.Value * (value - oldValue), isPublicEvent);
                }
            }

            if (isPublicEvent)
            {
                Unit unit = self.GetParent<Unit>();
                if (unit == null)
                {
                    return;
                }
                EventSystem.Instance.Publish(self.Scene(),
                    new NumbericChange() { Unit = unit, New = value, Old = oldValue, NumericType = numericType });
            }
        }

        public static long GetByKey(this NumericComponent self, int key)
        {
            return self.GetByKey((NumericType)key);
        }

        public static long GetByKey(this NumericComponent self, NumericType key)
        {
            long value = 0;
            self.NumericDic.TryGetValue(key, out value);
            return value;
        }

        public static void Update(this NumericComponent self, int numericType, bool isPublicEvent)
        {
            self.Update((NumericType)numericType, isPublicEvent);
        }

        public static void Update(this NumericComponent self, NumericType numericType, bool isPublicEvent)
        {
            int numericTypeValue = (int)numericType;
            int final = numericTypeValue / 10;
            int bas = final * 10 + 1;
            int add = final * 10 + 2;
            int pct = final * 10 + 3;
            int finalAdd = final * 10 + 4;
            int finalPct = final * 10 + 5;

            // 一个数值可能会多种情况影响，比如速度,加个buff可能增加速度绝对值100，也有些buff增加10%速度，所以一个值可以由5个值进行控制其最终结果
            // final = (((base + add) * (100 + pct) / 100) + finalAdd) * (100 + finalPct) / 100;
            int pctValue = self.GetAsInt(pct);
            if (pctValue < -100)
            {
                pctValue = -100;
            }
            int finalPctValue = self.GetAsInt(finalPct);
            if (finalPctValue < -100)
            {
                finalPctValue = -100;
            }
            long result = (long)(((self.GetByKey(bas) + self.GetByKey(add)) * (100 + pctValue) / 100f + self.GetByKey(finalAdd)) * (100 + finalPctValue) / 100f);
            self.Insert(final, result, isPublicEvent);
        }
    }
}
