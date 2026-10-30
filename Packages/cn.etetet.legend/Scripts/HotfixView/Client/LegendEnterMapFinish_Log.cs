namespace ET.Client
{
    [Event(SceneType.LegendClient)]
    public class LegendEnterMapFinish_Log: AEvent<Scene, LegendEnterMapFinish>
    {
        protected override async ETTask Run(Scene root, LegendEnterMapFinish args)
        {
            Log.Info($"legend enter map succeeded. Map: {args.MapName} UnitId: {args.UnitId}");
            await ETTask.CompletedTask;
        }
    }
}
