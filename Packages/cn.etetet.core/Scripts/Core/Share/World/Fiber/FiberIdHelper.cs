using System;

namespace ET
{
    public static class FiberIdHelper
    {
        public const int ZoneBits = 32;
        public const int LocalSlotBits = 32;
        public const int MaxZone = int.MaxValue;
        public const int MaxLocalSlot = int.MaxValue;
        public const int ConfigLocalSlotMax = 99999;
        public const int DynamicLocalSlotStart = 100000;
        public const int ReservedLocalSlotStart = MaxLocalSlot - 15;

        public static long Encode(int zone, int localSlot)
        {
            ValidateZone(zone);
            ValidateLocalSlot(localSlot);
            ulong encoded = ((ulong)(uint)zone << LocalSlotBits) | (uint)localSlot;
            return unchecked((long)encoded);
        }

        public static int DecodeZone(long fiberId)
        {
            return (int)((ulong)fiberId >> LocalSlotBits);
        }

        public static int DecodeLocalSlot(long fiberId)
        {
            return (int)((ulong)fiberId & uint.MaxValue);
        }

        public static bool IsReservedLocalSlot(int localSlot)
        {
            return localSlot >= ReservedLocalSlotStart && localSlot <= MaxLocalSlot;
        }

        public static void ValidateZone(int zone)
        {
            if ((uint)zone > MaxZone)
            {
                throw new ArgumentOutOfRangeException(nameof(zone), zone, $"fiber zone must be in [0, {MaxZone}]");
            }
        }

        public static void ValidateLocalSlot(int localSlot)
        {
            if ((uint)localSlot > MaxLocalSlot || localSlot == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(localSlot), localSlot,
                    $"fiber local slot must be in [1, {MaxLocalSlot}]");
            }
        }

        public static void ValidateFixedLocalSlot(int localSlot)
        {
            ValidateLocalSlot(localSlot);
            if (localSlot >= DynamicLocalSlotStart && localSlot < ReservedLocalSlotStart)
            {
                throw new ArgumentOutOfRangeException(nameof(localSlot), localSlot,
                    $"fixed fiber local slot cannot be in dynamic range [{DynamicLocalSlotStart}, {ReservedLocalSlotStart - 1}]");
            }
        }
    }
}
