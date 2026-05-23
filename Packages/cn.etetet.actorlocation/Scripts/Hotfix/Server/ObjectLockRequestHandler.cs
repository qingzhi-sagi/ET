using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectLockRequestHandler: MessageHandler<Scene, ObjectLockRequest, ObjectLockResponse>
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
}
