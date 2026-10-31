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

    [MemoryPackable]
    [Message(Opcode.LegendUnitInfo)]
    public partial class LegendUnitInfo : MessageObject
    {
        public static LegendUnitInfo Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<LegendUnitInfo>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public long UnitId { get; set; }
        [MemoryPackOrder(1)]
        public int ConfigId { get; set; }
        [MemoryPackOrder(2)]
        public int X { get; set; }
        [MemoryPackOrder(3)]
        public int Y { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_LegendUnitsAppear)]
    public partial class M2C_LegendUnitsAppear : MessageObject, IMessage
    {
        public static M2C_LegendUnitsAppear Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_LegendUnitsAppear>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public List<LegendUnitInfo> Units { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.M2C_LegendUnitsDisappear)]
    public partial class M2C_LegendUnitsDisappear : MessageObject, IMessage
    {
        public static M2C_LegendUnitsDisappear Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<M2C_LegendUnitsDisappear>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public List<long> UnitIds { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.Cli2LegendClient_Command)]
    [ResponseType(nameof(LegendClient2Cli_Command))]
    public partial class Cli2LegendClient_Command : MessageObject, IRequest
    {
        public static Cli2LegendClient_Command Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<Cli2LegendClient_Command>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public string Command { get; set; }
        [MemoryPackOrder(2)]
        public List<string> Args { get; set; } = new();

        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    [MemoryPackable]
    [Message(Opcode.LegendClient2Cli_Command)]
    public partial class LegendClient2Cli_Command : MessageObject, IResponse
    {
        public static LegendClient2Cli_Command Create(bool isFromPool = false)
        {
            return ObjectPool.Fetch<LegendClient2Cli_Command>(isFromPool);
        }

        [MemoryPackOrder(0)]
        public int RpcId { get; set; }
        [MemoryPackOrder(1)]
        public int Error { get; set; }
        [MemoryPackOrder(2)]
        public string Message { get; set; }
        public override void Dispose()
        {
            ObjectPool.Recycle(this);
        }
    }

    public static partial class Opcode
    {
        public const ushort C2G_LegendEnterMap = 11501;
        public const ushort G2C_LegendEnterMap = 11502;
        public const ushort LegendUnitInfo = 11503;
        public const ushort M2C_LegendUnitsAppear = 11504;
        public const ushort M2C_LegendUnitsDisappear = 11505;
        public const ushort Cli2LegendClient_Command = 11506;
        public const ushort LegendClient2Cli_Command = 11507;
    }
}