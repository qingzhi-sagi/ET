using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: MessageHandler<Scene, ObjectAddRequest, ObjectAddResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response)
        {
            LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
            if (!locationManagerComponent.EnsurePrimary(response))
            {
                return;
            }

            await locationManagerComponent.Get(request.Type).Add(request.Key, request.ActorId);
        }
    }
}
