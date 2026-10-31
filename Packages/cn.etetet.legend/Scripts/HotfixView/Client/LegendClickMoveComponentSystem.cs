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
            LegendMapComponent terrain = self.Scene().GetComponent<LegendMapComponent>();
            Texture2D texture = new(map.Width, map.Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Mesh mesh = self.Ground.GetComponent<MeshFilter>().sharedMesh;
            // 用实际 Plane 角点的 UV 方向对齐世界 X/Z 与地图列/行。
            bool flipX = (mesh.vertices[0].x < 0) != (mesh.uv[0].x < 0.5f);
            bool flipY = (mesh.vertices[0].z < 0) != (mesh.uv[0].y < 0.5f);
            for (int y = 0; y < map.Height; ++y)
            {
                for (int x = 0; x < map.Width; ++x)
                {
                    int argb = terrain.CellColors[y * map.Width + x];
                    texture.SetPixel(flipX ? map.Width - 1 - x : x, flipY ? map.Height - 1 - y : y,
                        new Color32((byte)(argb >> 16), (byte)(argb >> 8), (byte)argb, (byte)(argb >> 24)));
                }
            }
            texture.Apply();
            material.mainTexture = texture;
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
            LegendMoveHelper.MoveTo(self.Root(), cell);
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
