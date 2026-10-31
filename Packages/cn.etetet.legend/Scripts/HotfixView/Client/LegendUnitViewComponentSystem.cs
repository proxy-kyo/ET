using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LegendUnitViewComponent))]
    public static partial class LegendUnitViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LegendUnitViewComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            self.GameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            self.GameObject.name = $"LegendUnit_{unit.Id}";
            self.Transform = self.GameObject.transform;
            self.Transform.position = (Vector3)unit.Position + Vector3.up * 0.5f;
            Material material = new(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = BaseColor(unit);
            self.GameObject.GetComponent<Renderer>().sharedMaterial = material;
        }

        [EntitySystem]
        private static void Update(this LegendUnitViewComponent self)
        {
            LegendUnitMoveComponent move = self.GetParent<Unit>().GetComponent<LegendUnitMoveComponent>();
            self.Transform.position = (Vector3)LegendUnitMoveHelper.Position(move, self.GetSingleton<TimeInfo>().ClientNow())
                    + Vector3.up * 0.5f;
        }

        [EntitySystem]
        private static void Destroy(this LegendUnitViewComponent self)
        {
            UnityEngine.Object.Destroy(self.GameObject.GetComponent<Renderer>().sharedMaterial);
            UnityEngine.Object.Destroy(self.GameObject);
            self.GameObject = null;
            self.Transform = null;
        }

        public static Color BaseColor(Unit unit)
        {
            if (unit.ConfigId == LegendUnitConfigId.Boar)
            {
                return new Color(0.55f, 0.24f, 0.14f);
            }
            return unit.Id == unit.Root().GetComponent<PlayerComponent>().MyId ? Color.cyan : Color.yellow;
        }
    }
}
