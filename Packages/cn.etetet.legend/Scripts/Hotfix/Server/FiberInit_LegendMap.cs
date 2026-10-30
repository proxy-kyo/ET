namespace ET.Server
{
    [Invoke(SceneType.LegendMap)]
    public class FiberInit_LegendMap: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();
            root.AddComponent<UnitComponent>();
            root.AddComponent<AOIManagerComponent>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();

            ServiceDiscoveryProxy serviceDiscoveryProxy = root.AddComponent<ServiceDiscoveryProxy>();
            EntityRef<ServiceDiscoveryProxy> serviceDiscoveryProxyRef = serviceDiscoveryProxy;
            await serviceDiscoveryProxy.RegisterToServiceDiscovery();
            serviceDiscoveryProxy = serviceDiscoveryProxyRef;
            await serviceDiscoveryProxy.SubscribeServiceChange("Location",
                new StringKV
                {
                    { ServiceMetaKey.SceneType, SceneTypeSingleton.Instance.GetSceneName(SceneType.Location) },
                });
        }
    }
}
