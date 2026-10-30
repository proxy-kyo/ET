using Unity.Mathematics;

namespace ET.Server
{
    [Event(SceneType.LegendMap)]
    public class ChangePosition_LegendNotifyAOI: AEvent<Scene, ChangePosition>
    {
        protected override async ETTask Run(Scene scene, ChangePosition args)
        {
            Unit unit = args.Unit;
            float3 oldPos = args.OldPos;
            int oldCellX = (int)(oldPos.x * 1000) / AOIManagerComponent.CellSize;
            int oldCellY = (int)(oldPos.z * 1000) / AOIManagerComponent.CellSize;
            int newCellX = (int)(unit.Position.x * 1000) / AOIManagerComponent.CellSize;
            int newCellY = (int)(unit.Position.z * 1000) / AOIManagerComponent.CellSize;
            if (oldCellX == newCellX && oldCellY == newCellY)
            {
                return;
            }

            // Enter 设置出生位置时尚未添加 AOIEntity。
            AOIEntity aoi = unit.GetComponent<AOIEntity>();
            if (aoi != null)
            {
                scene.GetComponent<AOIManagerComponent>().Move(aoi, newCellX, newCellY);
            }
            await ETTask.CompletedTask;
        }
    }
}
