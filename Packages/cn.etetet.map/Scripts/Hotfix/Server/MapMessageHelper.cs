using System.Collections.Generic;

namespace ET.Server
{
    public static partial class MapMessageHelper
    {
        private static void Broadcast(Unit unit, IMessage message, bool withSelf = true)
        {
            (message as MessageObject).IsFromPool = false;
            Dictionary<long, EntityRef<AOIEntity>> dict = unit.GetBeSeePlayers();
            MessageSender messageSender = unit.Root().GetComponent<MessageSender>();
            
            foreach (AOIEntity u in dict.Values)
            {
                if (!withSelf && u.Id == unit.Id)
                {
                    continue;
                }
                messageSender.Send(u.Unit.GetComponent<UnitGateInfoComponent>().ActorId, message);
            }
        }
        
        private static void SendToClient(Unit unit, IMessage message)
        {
            if (!unit.UnitType.IsSame(UnitType.Player))
            {
                return;
            }
            MessageSender messageSender = unit.Root().GetComponent<MessageSender>();
            messageSender.Send(unit.GetComponent<UnitGateInfoComponent>().ActorId, message);
        }
        
        public static void NoticeClient(Unit unit, IMessage message, NoticeType noticeType)
        {
            switch (noticeType)
            {
                case NoticeType.Broadcast:
                    Broadcast(unit, message);
                    break;
                case NoticeType.Self:
                    SendToClient(unit, message);
                    break;
                case NoticeType.NoNotice:
                    break;
                case NoticeType.BroadcastWithoutSelf:
                    Broadcast(unit, message, false);
                    break;
            }
        }
    }
}
