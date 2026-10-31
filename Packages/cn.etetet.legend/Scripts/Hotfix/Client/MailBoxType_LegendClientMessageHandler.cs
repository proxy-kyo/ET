namespace ET.Client
{
    [Invoke(MailBoxType.LegendClient)]
    public class MailBoxType_LegendClientMessageHandler: AInvokeHandler<MailBoxInvoker>
    {
        public override void Handle(MailBoxInvoker args)
        {
            MailBoxComponent mailbox = args.MailBoxComponent;
            Scene root = mailbox.GetParent<Scene>();
            MessageObject message = args.MessageObject;
            if (message is M2C_LegendUnitsAppear or M2C_LegendUnitsDisappear or M2C_LegendHit or M2C_GridStep or M2C_GridStop)
            {
                LegendSceneMessagesComponent pending = root.GetComponent<LegendSceneMessagesComponent>();
                // AOI 的首次通知可能先于进图 RPC 返回。
                if (!pending.Ready)
                {
                    pending.Messages.Enqueue((args.FromFiber, message));
                    return;
                }
                root.CurrentScene().GetComponent<MailBoxComponent>().Add(args.FromFiber, message);
                return;
            }
            MessageDispatcher.Instance.Handle(root, args.FromFiber, message);
        }
    }
}
