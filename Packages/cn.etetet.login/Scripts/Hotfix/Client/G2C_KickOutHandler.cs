namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class G2C_KickOutHandler : MessageHandler<Scene, G2C_KickOut>
    {
        protected override async ETTask Run(Scene scene, G2C_KickOut message)
        {
            Log.Info(message.Message);
            await ETTask.CompletedTask;
        }
    }
}
