using System;

namespace ET.Server
{
    [MessageHandler(SceneType.Location)]
    public class ObjectRemoveRequestHandler: MessageHandler<Scene, ObjectRemoveRequest, ObjectRemoveResponse>
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
}
