using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.C2G_LegendEnterMap)]
    [ResponseType(nameof(G2C_LegendEnterMap))]
    public partial class C2G_LegendEnterMap : MessageObject, ISessionRequest
    {
        public static C2G_LegendEnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2G_LegendEnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.G2C_LegendEnterMap)]
    public partial class G2C_LegendEnterMap : MessageObject, ISessionResponse
    {
        public static G2C_LegendEnterMap Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<G2C_LegendEnterMap>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        [MemoryPackOrder(3)]
        public long MapSceneId { get; set; }
        [MemoryPackOrder(4)]
        public string MapName { get; set; }
        [MemoryPackOrder(5)]
        public long UnitId { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2G_LegendEnterMap = 11501;
        public const ushort G2C_LegendEnterMap = 11502;
    }
}