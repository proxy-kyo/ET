using MemoryPack;
using System.Collections.Generic;

namespace ET
{
    [MemoryPackable]
    [Message(Opcode.C2M_GridMoveTo)]
    public partial class C2M_GridMoveTo : MessageObject, ILocationMessage
    {
        public static C2M_GridMoveTo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<C2M_GridMoveTo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int X { get; set; }
        [MemoryPackOrder(2)]
        public int Y { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_GridStep)]
    public partial class M2C_GridStep : MessageObject, IMessage
    {
        public static M2C_GridStep Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_GridStep>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }
        [MemoryPackOrder(1)]
        public int X { get; set; }
        [MemoryPackOrder(2)]
        public int Y { get; set; }
        [MemoryPackOrder(3)]
        public int DurationMs { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_GridStop)]
    public partial class M2C_GridStop : MessageObject, IMessage
    {
        public static M2C_GridStop Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_GridStop>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }
        [MemoryPackOrder(1)]
        public int X { get; set; }
        [MemoryPackOrder(2)]
        public int Y { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2M_GridMoveTo = 11601;
        public const ushort M2C_GridStep = 11602;
        public const ushort M2C_GridStop = 11603;
    }
}