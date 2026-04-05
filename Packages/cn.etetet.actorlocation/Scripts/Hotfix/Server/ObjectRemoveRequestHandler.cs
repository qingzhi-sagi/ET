using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectRemoveRequestHandler: MessageHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectRemoveRequest request, ObjectRemoveResponse response)
        {
            LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
            if (!locationManagerComponent.EnsurePrimary(response))
            {
                return;
            }

            await locationManagerComponent.Get(request.Type).Remove(request.Key, request.ExpectedActorId);
        }
    }
}
