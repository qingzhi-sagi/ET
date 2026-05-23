using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectGetRequestHandler: MessageHandler<Scene, ObjectGetRequest, ObjectGetResponse>
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
}
