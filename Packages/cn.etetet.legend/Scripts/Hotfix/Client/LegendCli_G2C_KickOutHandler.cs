using System;

namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class LegendCli_G2C_KickOutHandler: MessageHandler<Scene, G2C_KickOut>
    {
        protected override ETTask Run(Scene root, G2C_KickOut message)
        {
            LegendCliClientComponent client = root.GetComponent<LegendCliClientComponent>();
            if (client == null) return ETTask.CompletedTask;
            client.Online = false;
            if (client.Print) Console.WriteLine($"[client {client.Index}] kicked: {message.Message}");
            return ETTask.CompletedTask;
        }
    }
}
