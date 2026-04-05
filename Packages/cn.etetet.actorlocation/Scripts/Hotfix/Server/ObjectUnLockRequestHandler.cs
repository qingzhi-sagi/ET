using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectUnLockRequestHandler: MessageHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectUnLockRequest request, ObjectUnLockResponse response)
        {
            LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
            if (!locationManagerComponent.EnsurePrimary(response))
            {
                return;
            }

            await locationManagerComponent.Get(request.Type)
                    .UnLock(request.Key, request.OldActorId, request.NewActorId, request.LockToken);
        }
    }
}
