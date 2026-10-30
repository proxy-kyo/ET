using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class LegendSceneMessagesComponent: Entity, IAwake, IDestroy
    {
        public bool Ready;
        public Queue<(long FromFiber, MessageObject Message)> Messages = new();
    }
}
