using System;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class LegendCli_NetClient2Main_SessionDisposeHandler: MessageHandler<Scene, NetClient2Main_SessionDispose>
    {
        protected override ETTask Run(Scene root, NetClient2Main_SessionDispose message)
        {
            LegendCliClientComponent client = root.GetComponent<LegendCliClientComponent>();
            if (client == null) return ETTask.CompletedTask;
            client.Online = false;
            if (client.Print) Console.WriteLine($"[client {client.Index}] disconnected: {message.Error}");
            return ETTask.CompletedTask;
        }
    }
}
