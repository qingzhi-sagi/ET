using System;

namespace ET
{
    public static class FiberIdHelper
    {
        public const int ZoneBits = 12;
        public const int LocalSlotBits = 20;
        public const int MaxZone = (1 << ZoneBits) - 1;
        public const int MaxLocalSlot = (1 << LocalSlotBits) - 1;
        public const int ConfigLocalSlotMax = 99999;
        public const int DynamicLocalSlotStart = 100000;
        public const int ReservedLocalSlotStart = 0xFFFF0;

        public static int Encode(int zone, int localSlot)
        {
            ValidateZone(zone);
            ValidateLocalSlot(localSlot);
            uint encoded = ((uint)zone << LocalSlotBits) | (uint)localSlot;
            return unchecked((int)encoded);
        }

        public static int DecodeZone(int fiberId)
        {
            return (int)((uint)fiberId >> LocalSlotBits);
        }

        public static int DecodeLocalSlot(int fiberId)
        {
            return (int)((uint)fiberId & MaxLocalSlot);
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
