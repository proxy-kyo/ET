using System;
using Unity.Mathematics;

namespace ET.Server
{
    public static class LegendCombatHelper
    {
        [EnableGetComponent(typeof(NumericComponent))]
        public static int Attack(Unit attacker, long targetId)
        {
            if (attacker.Id == targetId)
            {
                return ErrorCode.ERR_LegendAttackSelf;
            }
            Unit target = attacker.Scene().GetComponent<UnitComponent>().Get(targetId);
            NumericComponent numeric = target?.GetComponent<NumericComponent>();
            if (numeric == null || numeric.GetAsLong(NumericType.HP) <= 0)
            {
                return ErrorCode.ERR_LegendAttackTargetNotFound;
            }
            int2 delta = attacker.GetComponent<GridMoveComponent>().Cell - target.GetComponent<GridMoveComponent>().Cell;
            if (math.max(math.abs(delta.x), math.abs(delta.y)) != 1)
            {
                return ErrorCode.ERR_LegendAttackNotAdjacent;
            }
            LegendCombatComponent combat = attacker.GetComponent<LegendCombatComponent>();
            long now = attacker.GetSingleton<TimeInfo>().ServerNow();
            if (now < combat.NextAttackTime)
            {
                return ErrorCode.ERR_LegendAttackCooldown;
            }
            combat.NextAttackTime = now + LegendCombatConst.CooldownMs;
            long hp = Math.Max(0, numeric.GetAsLong(NumericType.HP) - LegendCombatConst.Damage);
            numeric.SetNoEvent(NumericType.HP, hp);
            M2C_LegendHit hit = M2C_LegendHit.Create();
            hit.AttackerId = attacker.Id;
            hit.TargetId = targetId;
            hit.Damage = LegendCombatConst.Damage;
            hit.Hp = hp;
            MapMessageHelper.NoticeClient(target, hit, NoticeType.Broadcast);
            // ponytail: 玩家死亡（复活点、掉落）还没做，玩家 0 血时留在原地。
            if (hp == 0 && target.GetComponent<LegendMonsterComponent>() != null)
            {
                LegendMonsterHelper.Die(target);
            }
            return ErrorCode.ERR_Success;
        }
    }
}
