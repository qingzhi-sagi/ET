using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectAddRequestHandler: MessageHandler<Scene, ObjectAddRequest, ObjectAddResponse>
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
}
