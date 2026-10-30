using UnityEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class LegendClickMoveComponent: Entity, IAwake, IUpdate, IDestroy
    {
        public GameObject Ground;
        public Collider GroundCollider;
        public Camera Camera;
    }
}
