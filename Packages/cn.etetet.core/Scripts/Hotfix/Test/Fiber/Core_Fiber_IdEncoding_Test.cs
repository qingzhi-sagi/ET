namespace ET.Test
{
    public class Core_Fiber_IdEncoding_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Core_Fiber_IdEncoding_Test));
            Fiber parent = scope.TestFiber;
            int zone = parent.Zone;

            Fiber first = await parent.CreateFiber(zone, IdGenerater.Instance.GenerateId(), SceneType.TestEmpty,
                "FiberIdEncodingFirst");
            long firstId = first.Id;
            int firstLocalSlot = first.LocalSlot;

            if (first.Zone != zone)
            {
                Log.Console($"fiber zone mismatch, expected: {zone}, actual: {first.Zone}");
                return 1;
            }

            if (FiberIdHelper.DecodeZone(first.Id) != zone)
            {
                Log.Console($"decoded zone mismatch, expected: {zone}, actual: {FiberIdHelper.DecodeZone(first.Id)}");
                return 2;
            }

            if (FiberIdHelper.DecodeLocalSlot(first.Id) != firstLocalSlot)
            {
                Log.Console(
                    $"decoded local slot mismatch, expected: {firstLocalSlot}, actual: {FiberIdHelper.DecodeLocalSlot(first.Id)}");
                return 3;
            }

            if (firstLocalSlot < FiberIdHelper.DynamicLocalSlotStart ||
                firstLocalSlot >= FiberIdHelper.ReservedLocalSlotStart)
            {
                Log.Console($"local slot not in dynamic range, actual: {firstLocalSlot}");
                return 4;
            }

            await parent.RemoveFiber(firstId);

            Fiber second = await parent.CreateFiber(zone, IdGenerater.Instance.GenerateId(), SceneType.TestEmpty,
                "FiberIdEncodingSecond");
            long secondId = second.Id;
            int secondLocalSlot = second.LocalSlot;

            try
            {
                if (secondId == firstId)
                {
                    Log.Console($"fiber id reused after destroy, id: {secondId}");
                    return 5;
                }

                if (secondLocalSlot <= firstLocalSlot)
                {
                    Log.Console($"local slot should increase, first: {firstLocalSlot}, second: {secondLocalSlot}");
                    return 6;
                }

                return ErrorCode.ERR_Success;
            }
            finally
            {
                await parent.RemoveFiber(secondId);
            }
        }
    }
}
