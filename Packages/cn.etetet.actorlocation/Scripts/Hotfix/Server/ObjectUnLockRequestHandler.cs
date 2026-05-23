using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectUnLockRequestHandler: MessageHandler<Scene, ObjectUnLockRequest, ObjectUnLockResponse>
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
}
