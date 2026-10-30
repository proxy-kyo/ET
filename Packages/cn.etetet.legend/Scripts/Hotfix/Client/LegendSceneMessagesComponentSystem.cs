namespace ET.Client
{
    [EntitySystemOf(typeof(LegendSceneMessagesComponent))]
    public static partial class LegendSceneMessagesComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LegendSceneMessagesComponent self)
        {
            self.Ready = false;
        }

        [EntitySystem]
        private static void Destroy(this LegendSceneMessagesComponent self)
        {
            self.Ready = false;
            self.Messages.Clear();
        }
    }
}
