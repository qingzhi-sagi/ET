using System;

namespace ET.Test
{
    [Invoke(SceneType.TestEmpty)]
    public class FiberInit_TestEmpty: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            if (root == null)
            {
                throw new Exception("test empty root scene is null");
            }

            root.AddComponent<TestFiberDatabaseCleanupComponent>();

            await ETTask.CompletedTask;
        }
    }
}
