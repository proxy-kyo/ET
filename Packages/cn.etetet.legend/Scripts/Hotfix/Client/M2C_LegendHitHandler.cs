namespace ET.Client
{
    [MessageHandler(SceneType.Current)]
    public class M2C_LegendHitHandler: MessageHandler<Scene, M2C_LegendHit>
    {
        [EnableGetComponent(typeof(NumericComponent))]
        protected override async ETTask Run(Scene scene, M2C_LegendHit message)
        {
            Unit unit = scene.GetComponent<UnitComponent>().Get(message.TargetId);
            if (unit == null)
            {
                return;
            }
            unit.GetComponent<NumericComponent>().SetNoEvent(NumericType.HP, message.Hp);
            EventSystem.Instance.Publish(scene, new LegendUnitHit { Unit = unit, Damage = message.Damage });
            await ETTask.CompletedTask;
        }
    }
}
