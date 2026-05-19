using System;

namespace ET
{
    public interface IMHandler
    {
        ETTask Handle(Entity entity, long fromFiber, MessageObject actorMessage);
        Type GetRequestType();
        Type GetResponseType();
    }
}
