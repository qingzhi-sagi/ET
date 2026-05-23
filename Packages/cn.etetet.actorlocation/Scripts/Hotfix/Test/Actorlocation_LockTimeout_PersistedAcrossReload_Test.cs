using System;
using ET.Server;

namespace ET.Test
{
    public class Actorlocation_LockTimeout_PersistedAcrossReload_Test : ATestHandler
    {
        private const int LocationType = 910009;

        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Actorlocation_LockTimeout_PersistedAcrossReload_Test));

            try
            {
                Scene scene = Actorlocation_TestHelper.PrepareLocationScene(scope.TestFiber);
                LocationComponent location = Actorlocation_TestHelper.GetLocationComponent(scene);
                EntityRef<LocationComponent> locationRef = location;

                TimerComponent timerComponent = scene.TimerComponent;
                EntityRef<TimerComponent> timerRef = timerComponent;

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301007, 1);

                await location.Add(LocationType, key, actorA);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/add");

                long lockToken = await location.Lock(LocationType, key, actorA, 180);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/lock");
                Actorlocation_TestHelper.AssertTrue(lockToken != 0, "persist-timeout/lock-token");

                LocationTypeState lockState = Actorlocation_TestHelper.GetLocationTypeState(location, LocationType, key,
                    "persist-timeout/lock-info");
                Actorlocation_TestHelper.AssertTrue(lockState.LockToken == lockToken, "persist-timeout/lock-token-match");
                Actorlocation_TestHelper.AssertTrue(lockState.LockExpireTime > location.GetSingleton<TimeInfo>().ServerNow(),
                    "persist-timeout/lock-expire-time");

                // 清理内存缓存模拟主备切换，仅靠 DB 恢复状态。
                location.RemoveChild(key);

                try
                {
                    LocationComponent current = Actorlocation_TestHelper.EnsureLocationComponent(locationRef,
                        "persist-timeout/get-retry-before-expire");
                    await current.Get(LocationType, key);
                    throw new Exception(
                        $"persist-timeout/get-retry-before-expire: expected RpcException({ErrorCode.ERR_LocationGetRetry}), but no exception");
                }
                catch (RpcException e)
                {
                    Actorlocation_TestHelper.AssertRpcError(
                        e,
                        ErrorCode.ERR_LocationGetRetry,
                        "persist-timeout/get-retry-before-expire");
                }

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/clear-cache-before-expire");
                location.RemoveChild(key);

                timerComponent = timerRef;
                if (timerComponent == null)
                {
                    throw new Exception("persist-timeout: timer component disposed before wait");
                }
                await timerComponent.WaitAsync(260);

                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/get-after-expire-before-call");
                ActorId actorAfterExpire = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/get-after-expire");
                Actorlocation_TestHelper.AssertActorEqual(actorA, actorAfterExpire, "persist-timeout/get-after-expire");

                LocationTypeState unlockedState = Actorlocation_TestHelper.GetLocationTypeState(location, LocationType, key,
                    "persist-timeout/unlocked-info");
                Actorlocation_TestHelper.AssertTrue(unlockedState.LockToken == 0, "persist-timeout/unlocked-token");
                Actorlocation_TestHelper.AssertTrue(unlockedState.LockExpireTime == 0, "persist-timeout/unlocked-expire-time");

                // 再次清缓存，验证解锁状态已持久化，重载后仍可直接 Get。
                location.RemoveChild(key);
                ActorId actorAfterSecondReload = await location.Get(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/get-after-second-reload");
                Actorlocation_TestHelper.AssertActorEqual(actorA, actorAfterSecondReload,
                    "persist-timeout/get-after-second-reload");

                await location.Remove(LocationType, key);
                location = Actorlocation_TestHelper.EnsureLocationComponent(locationRef, "persist-timeout/remove");
                Actorlocation_TestHelper.AssertTrue(Actorlocation_TestHelper.GetLocationInfo(location, key) == null, "persist-timeout/remove-cache");

                return ErrorCode.ERR_Success;
            }
            catch (Exception e)
            {
                Log.Error($"Actorlocation_LockTimeout_PersistedAcrossReload_Test failed: {e}");
                return 1;
            }
        }
    }
}
