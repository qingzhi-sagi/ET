using System;

namespace ET.Test
{
    public class Core_FiberInstanceIdHeader_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            int headerLength = Packet.FiberInstanceIdLength;
            if (headerLength != 16)
            {
                Log.Console($"fiber instance id header length mismatch, expected: 16, actual: {headerLength}");
                return 1;
            }

            long fiberId = FiberIdHelper.Encode(100000, 1500000);
            long instanceId = (long)int.MaxValue + 12345;
            FiberInstanceId fiberInstanceId = new(fiberId, instanceId);
            byte[] bytes = new byte[Packet.FiberInstanceIdLength];
            bytes.WriteTo(0, fiberInstanceId);

            long decodedFiberId = BitConverter.ToInt64(bytes, 0);
            if (decodedFiberId != fiberId)
            {
                Log.Console($"decoded fiber id mismatch, expected: {fiberId}, actual: {decodedFiberId}");
                return 2;
            }

            long decodedInstanceId = BitConverter.ToInt64(bytes, 8);
            if (decodedInstanceId != instanceId)
            {
                Log.Console($"decoded instance id mismatch, expected: {instanceId}, actual: {decodedInstanceId}");
                return 3;
            }

            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }
    }
}
