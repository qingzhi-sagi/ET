using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectGetRequestHandler: MessageHandler<Scene, ObjectGetRequest, ObjectGetResponse>
    {
        protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response)
        {
            LocationManagerComponent locationManagerComponent = scene.GetComponent<LocationManagerComponent>();
            if (!locationManagerComponent.EnsurePrimary(response))
            {
                return;
            }

            response.ActorId = await locationManagerComponent.Get(request.Type).Get(request.Key);
        }
    }
}
