using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.G2M_LegendEnterMap)]
    [ResponseType(nameof(M2G_LegendEnterMap))]
    public partial class G2M_LegendEnterMap : MessageObject, IRequest
    {
        public static G2M_LegendEnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2M_LegendEnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public long PlayerId { get; set; }
        [MemoryPackOrder(2)]
        public ActorId GateActorId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2G_LegendEnterMap)]
    public partial class M2G_LegendEnterMap : MessageObject, IResponse
    {
        public static M2G_LegendEnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2G_LegendEnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public long UnitId { get; set; }
        [MemoryPackOrder(4)]
        public long MapSceneId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort G2M_LegendEnterMap = 21401;
        public const ushort M2G_LegendEnterMap = 21402;
    }
}