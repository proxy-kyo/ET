namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LoginFinish_EnterMap: AEvent<Scene, LoginFinish>
    {
        protected override async ETTask Run(Scene root, LoginFinish args)
        {
            await LegendEnterMapHelper.EnterMapAsync(root);
        }
    }
}
