namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LegendEnterMapFinish_CreateMapView: AEvent<Scene, LegendEnterMapFinish>
    {
        protected override async ETTask Run(Scene root, LegendEnterMapFinish args)
        {
            root.CurrentScene().AddComponent<LegendClickMoveComponent>();
            await ETTask.CompletedTask;
        }
    }
}
