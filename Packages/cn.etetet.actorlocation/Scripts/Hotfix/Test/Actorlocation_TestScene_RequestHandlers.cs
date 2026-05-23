using System;
using ET.Server;

namespace ET.Test
{
    [MessageHandler(SceneType.TestEmpty)]
    public class Actorlocation_TestScene_ObjectAddRequestHandler : MessageHandler<Scene, ObjectAddRequest, ObjectAddResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response)
        {
            LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
            if (!locationComponent.EnsurePrimary(response))
            {
                return;
            }

            await locationComponent.Add(request.Type, request.Key, request.ActorId);
        }
    }

    [MessageHandler(SceneType.TestEmpty)]
    public class Actorlocation_TestScene_ObjectGetRequestHandler : MessageHandler<Scene, ObjectGetRequest, ObjectGetResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response)
        {
            LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
            if (!locationComponent.EnsurePrimary(response))
            {
                return;
            }

            response.ActorId = await locationComponent.Get(request.Type, request.Key);
        }
    }

    [MessageHandler(SceneType.TestEmpty)]
    public class Actorlocation_TestScene_ObjectLockRequestHandler : MessageHandler<Scene, ObjectLockRequest, ObjectLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectLockRequest request, ObjectLockResponse response)
        {
            LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
            if (!locationComponent.EnsurePrimary(response))
            {
                return;
            }

            response.LockToken = await locationComponent.Lock(request.Type, request.Key, request.ActorId, request.Time);
        }
    }

    [MessageHandler(SceneType.TestEmpty)]
    public class Actorlocation_TestScene_ObjectUnLockRequestHandler : MessageHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
            if (!locationComponent.EnsurePrimary(response))
            {
                return;
            }

            await locationComponent.UnLock(request.Type, request.Key, request.OldActorId, request.NewActorId, request.LockToken);
        }
    }

    [MessageHandler(SceneType.TestEmpty)]
    public class Actorlocation_TestScene_ObjectRemoveRequestHandler : MessageHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response)
        {
            LocationComponent locationComponent = scene.GetComponent<LocationComponent>();
            if (!locationComponent.EnsurePrimary(response))
            {
                return;
            }

            await locationComponent.Remove(request.Type, request.Key, request.ExpectedActorId);
        }
    }

    [MessageHandler(SceneType.TestEmpty)]
    public class Actorlocation_TestScene_C2M_TestRequestHandler : MessageLocationHandler<Scene, C2M_TestRequest, M2C_TestResponse>
    {
        protected override async ETTask Run(Scene scene, C2M_TestRequest request, M2C_TestResponse response)
        {
            response.response = $"echo:{request.request}";
            await ETTask.CompletedTask;
        }
    }
}
