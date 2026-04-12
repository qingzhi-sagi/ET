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
                LocationOneType location = Actorlocation_TestHelper.GetLocationOneType(scene, LocationType);
                EntityRef<LocationOneType> locationRef = location;

                TimerComponent timerComponent = scene.TimerComponent;
                EntityRef<TimerComponent> timerRef = timerComponent;

                long key = IdGenerater.Instance.GenerateId();
                ActorId actorA = Actorlocation_TestHelper.CreateActorId(scope.TestFiber, 301007, 1);

                await location.Add(key, actorA);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/add");

                long lockToken = await location.Lock(key, actorA, 180);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/lock");
                Actorlocation_TestHelper.AssertTrue(lockToken != 0, "persist-timeout/lock-token");

                LocationInfo lockInfo = location.GetChild<LocationInfo>(key);
                Actorlocation_TestHelper.AssertTrue(lockInfo != null, "persist-timeout/lock-info-exists");
                Actorlocation_TestHelper.AssertTrue(lockInfo.LockToken == lockToken, "persist-timeout/lock-token-match");
                Actorlocation_TestHelper.AssertTrue(lockInfo.LockExpireTime > lockInfo.GetSingleton<TimeInfo>().ServerNow(),
                    "persist-timeout/lock-expire-time");

                // 清理内存缓存模拟主备切换，仅靠 DB 恢复状态。
                location.RemoveChild(key);

                try
                {
                    LocationOneType current = Actorlocation_TestHelper.EnsureLocation(locationRef,
                        "persist-timeout/get-retry-before-expire");
                    await current.Get(key);
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

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/clear-cache-before-expire");
                location.RemoveChild(key);

                timerComponent = timerRef;
                if (timerComponent == null)
                {
                    throw new Exception("persist-timeout: timer component disposed before wait");
                }
                await timerComponent.WaitAsync(260);

                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/get-after-expire-before-call");
                ActorId actorAfterExpire = await location.Get(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/get-after-expire");
                Actorlocation_TestHelper.AssertActorEqual(actorA, actorAfterExpire, "persist-timeout/get-after-expire");

                LocationInfo unlockedInfo = location.GetChild<LocationInfo>(key);
                Actorlocation_TestHelper.AssertTrue(unlockedInfo != null, "persist-timeout/unlocked-info-exists");
                Actorlocation_TestHelper.AssertTrue(unlockedInfo.LockToken == 0, "persist-timeout/unlocked-token");
                Actorlocation_TestHelper.AssertTrue(unlockedInfo.LockExpireTime == 0, "persist-timeout/unlocked-expire-time");

                // 再次清缓存，验证解锁状态已持久化，重载后仍可直接 Get。
                location.RemoveChild(key);
                ActorId actorAfterSecondReload = await location.Get(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/get-after-second-reload");
                Actorlocation_TestHelper.AssertActorEqual(actorA, actorAfterSecondReload,
                    "persist-timeout/get-after-second-reload");

                await location.Remove(key);
                location = Actorlocation_TestHelper.EnsureLocation(locationRef, "persist-timeout/remove");
                Actorlocation_TestHelper.AssertTrue(location.GetChild<LocationInfo>(key) == null, "persist-timeout/remove-cache");

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
