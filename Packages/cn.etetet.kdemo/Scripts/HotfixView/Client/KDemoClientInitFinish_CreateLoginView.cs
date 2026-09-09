using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ET.Client
{
    [Event(SceneType.KDemoClient)]
    public class KDemoClientInitFinish_CreateLoginView: AEvent<Scene, KDemoClientInitFinish>
    {
        protected override async ETTask Run(Scene root, KDemoClientInitFinish args)
        {
            GameObject global = GameObject.Find("/Global");
            if (global == null)
            {
                Log.Error("KDemo 场景缺少 /Global。");
                return;
            }

            if (global.transform.Find("Unit") == null)
            {
                new GameObject("Unit").transform.SetParent(global.transform, false);
            }

            World.Instance.AddSingleton<YIUIEventComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<GlobalComponent>();

            EntityRef<Scene> rootRef = root;
            bool initialized = await root.AddComponent<YIUIMgrComponent>().Initialize();
            if (!initialized)
            {
                Log.Error("初始化 YIUI 失败");
                return;
            }

            root = rootRef;
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Log.Error("KDemo 场景缺少 Main Camera。");
                return;
            }

            Camera uiCamera = root.YIUIMgr().UICamera;
            UniversalAdditionalCameraData mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            if (!mainCameraData.cameraStack.Contains(uiCamera))
            {
                mainCameraData.cameraStack.Add(uiCamera);
            }

            await root.YIUIRoot().OpenPanelAsync<KLoginPanelComponent>();
        }
    }
}
