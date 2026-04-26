namespace ET.Client
{
    [Invoke(MailBoxType.ClientMessage)]
    public class MailBoxType_ClientMessageHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            MailBoxComponent mailBoxComponent = args.MailBoxComponent;
            MessageObject messageObject = args.MessageObject;

            if (messageObject is ICurrentMessage)
            {
                Scene clientScene = mailBoxComponent.Parent as Scene;
                Scene currentScene = clientScene?.GetComponent<CurrentScenesComponent>().Scene;
                if (currentScene == null)
                {
                    Log.Warning($"current scene mailbox not found: {messageObject}");
                    return;
                }

                MailBoxComponent currentMailBoxComponent = currentScene.GetComponent<MailBoxComponent>();
                currentMailBoxComponent.Add(args.FromFiber, messageObject);
                return;
            }

            MessageDispatcher.Instance.Handle(mailBoxComponent.Parent, args.FromFiber, messageObject);
        }
    }
}
