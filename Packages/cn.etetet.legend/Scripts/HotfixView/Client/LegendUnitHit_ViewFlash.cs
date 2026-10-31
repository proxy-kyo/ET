using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Current)]
    public class LegendUnitHit_ViewFlash: AEvent<Scene, LegendUnitHit>
    {
        [EnableGetComponent(typeof(NumericComponent))]
        protected override async ETTask Run(Scene scene, LegendUnitHit args)
        {
            Unit unit = args.Unit;
            if (unit == null)
            {
                return;
            }
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            float ratio = numeric.GetAsLong(NumericType.MaxHP) == 0 ? 0 :
                Mathf.Clamp01((float)numeric.GetAsLong(NumericType.HP) / numeric.GetAsLong(NumericType.MaxHP));
            Material material = unit.GetComponent<LegendUnitViewComponent>().GameObject.GetComponent<Renderer>().sharedMaterial;
            material.color = Color.Lerp(new Color(0.35f, 0.05f, 0.05f), LegendUnitViewComponentSystem.BaseColor(unit), ratio);
            await ETTask.CompletedTask;
        }
    }
}
