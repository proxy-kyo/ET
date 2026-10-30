using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class LegendUnitViewComponent: Entity, IAwake, IUpdate, IDestroy
    {
        public GameObject GameObject;
        public Transform Transform;
    }
}
