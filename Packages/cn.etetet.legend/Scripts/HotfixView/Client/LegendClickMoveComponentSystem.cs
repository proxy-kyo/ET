using Unity.Mathematics;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LegendClickMoveComponent))]
    public static partial class LegendClickMoveComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LegendClickMoveComponent self)
        {
            GridMapComponent map = self.Scene().GetComponent<GridMapComponent>();
            self.Ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            self.Ground.name = "LegendGround";
            self.Ground.transform.position = new Vector3(map.Width * map.CellSize / 2, -0.01f, map.Height * map.CellSize / 2);
            self.Ground.transform.localScale = new Vector3(map.Width * map.CellSize / 10, 1, map.Height * map.CellSize / 10);
            self.GroundCollider = self.Ground.GetComponent<Collider>();
            Material material = new(Shader.Find("Universal Render Pipeline/Unlit"));
            // ponytail: 棋盘格只为看出格子和移动，Tiled 地图接入后删掉。
            Texture2D checker = new(map.Width, map.Height) { filterMode = FilterMode.Point };
            for (int y = 0; y < map.Height; ++y)
            {
                for (int x = 0; x < map.Width; ++x)
                {
                    checker.SetPixel(x, y, (x + y) % 2 == 0 ? new Color(0.45f, 0.45f, 0.45f) : new Color(0.6f, 0.6f, 0.6f));
                }
            }
            checker.Apply();
            material.mainTexture = checker;
            self.Ground.GetComponent<Renderer>().sharedMaterial = material;
            self.Camera = Camera.main;
            self.Camera.transform.rotation = Quaternion.Euler(45, 45, 0);
            self.Camera.orthographic = true;
            self.Camera.orthographicSize = 12;
        }

        [EntitySystem]
        private static void Update(this LegendClickMoveComponent self)
        {
            Unit unit = self.Scene().GetComponent<UnitComponent>().Get(self.Root().GetComponent<PlayerComponent>().MyId);
            // 地图表现先初始化，自己的单位随后由 Appear 创建。
            if (unit == null)
            {
                return;
            }
            self.Camera.transform.position = unit.GetComponent<LegendUnitViewComponent>().Transform.position
                    - self.Camera.transform.forward * 30;
            if (!Input.GetMouseButtonDown(0) ||
                !self.GroundCollider.Raycast(self.Camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000))
            {
                return;
            }
            GridMapComponent map = self.Scene().GetComponent<GridMapComponent>();
            int2 cell = GridHelper.WorldToCell(map, hit.point);
            if (!GridHelper.InBounds(map, cell))
            {
                return;
            }
            // ponytail: 不做本机预测，收到服务端步进后再插值。
            C2M_GridMoveTo message = C2M_GridMoveTo.Create();
            message.X = cell.x;
            message.Y = cell.y;
            self.Root().GetComponent<ClientSenderComponent>().Send(message);
        }

        [EntitySystem]
        private static void Destroy(this LegendClickMoveComponent self)
        {
            Material material = self.Ground.GetComponent<Renderer>().sharedMaterial;
            UnityEngine.Object.Destroy(material.mainTexture);
            UnityEngine.Object.Destroy(material);
            UnityEngine.Object.Destroy(self.Ground);
            self.Ground = null;
            self.GroundCollider = null;
            self.Camera = null;
        }
    }
}
