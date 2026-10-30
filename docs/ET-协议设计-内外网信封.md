# ET 协议设计：内外网信封

## 一、先总结

ET 的协议由三层组成：传输层、ET 信封层、业务消息体。

```text
传输层：TCP 或 UDP/KCP
ET 信封层：长度（TCP）/ KCP 控制头 + FiberInstanceId（内网）+ opcode
业务消息体：MemoryPack 按具体消息类型序列化
```

### 1. 外网业务消息

外网消息不带 `FiberInstanceId`，业务信封是：

```text
TCP：
[2 字节 bodyLength][2 字节 opcode][MemoryPack body]

KCP：
[1 字节 ET 控制类型][4 字节 localConn][KCP payload]
KCP payload = [2 字节 opcode][MemoryPack body]
```

`bodyLength` 只计算后面的 `opcode + MemoryPack body`，不包括长度字段本身。

### 2. 内网业务消息

内网消息在业务 opcode 前增加 16 字节 `FiberInstanceId`，用于把消息投递到目标纤程实例：

```text
TCP：
[4 字节 bodyLength]
[8 字节 Fiber]
[8 字节 InstanceId]
[2 字节 opcode]
[MemoryPack body]

KCP：
[1 字节 ET 控制类型][4 字节 localConn][KCP payload]
KCP payload =
    [8 字节 Fiber][8 字节 InstanceId]
    [2 字节 opcode][MemoryPack body]
```

### 3. opcode 的职责

`opcode` 是 2 字节 `ushort`，作用是把字节流映射到具体 C# 消息类型：

```text
opcode → OpcodeType.GetType(opcode) → C# 消息类型 → MemoryPack 反序列化
```

它不是版本号，也不是额外的消息方向字段。请求、响应和推送消息都各自拥有独立 opcode。

### 4. 当前编号约定

| 范围 | 用途 |
| --- | --- |
| `10001 ~ 20000` | 外网客户端消息，`OuterMinOpcode = 10001`、`OuterMaxOpcode = 20000` |
| `20001 ~ 40000` | 内网消息，`InnerMinOpcode = 20001`、`InnerMaxOpcode = 40000` |
| `60000` 以下 | `OpcodeRangeDefine.MaxOpcode` 允许的总上限 |
| `65535` | 内置 `MessageResponse` 的特殊 opcode |

外网接收代码实际判断为 `opcode > 20000 || opcode < 10000`，因此有效值是 `10001 ~ 20000`；项目通常把 `20000` 作为下一个内网区间的边界，新增外网消息应按已有区间规划，避免使用边界值造成歧义。

## 二、再实现

### 1. 发送路径

业务代码调用：

```csharp
session.Send(message);
```

调用链：

```text
Session.Send
  → MessageSerializeHelper.ToMemoryBuffer
  → MessageToStream
  → OpcodeType.GetOpcode(message.GetType())
  → MemoryPackHelper.Serialize(message)
  → AService.Send
  → TChannel.Send（TCP）或 KChannel.Send（KCP）
```

外网 `MessageToStream` 从偏移 `0` 写入：

```text
[opcode][body]
```

内网从偏移 `16` 写入：

```text
[FiberInstanceId][opcode][body]
```

对应实现：

- [SessionSystem.cs](../Packages/cn.etetet.core/Scripts/Hotfix/Share/Message/SessionSystem.cs)
- [MessageSerializeHelper.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/MessageSerializeHelper.cs)

### 2. TCP 封装

TCP 是字节流，`TChannel.Send` 在业务数据前补长度：

```text
外网：2 字节 ushort
内网：4 字节 int
```

接收端由 `PacketParser` 先读长度，再等待完整消息体：

```text
PacketSize → PacketBody → ReadCallback
```

这样一次 `Socket.Receive` 拿到半包、整包或多个包都不会影响上层消息边界。

对应实现：

- [TChannel.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/TChannel.cs)
- [PacketParser.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/PacketParser.cs)

### 3. KCP/UDP 封装

ET 的 UDP 数据报先经过 `KService.Recv` 按第一个字节识别 ET 控制类型：

```text
1 = SYN
2 = ACK
3 = FIN
4 = MSG
5 = RouterReconnectSYN
6 = RouterReconnectACK
7 = RouterSYN
8 = RouterACK
```

普通业务包的 UDP 外层为：

```text
[1 字节 MSG][4 字节 localConn][KCP segment]
```

`KCP segment` 解出完整 payload 后，才交给 `KChannel` 处理业务消息。KCP 本身负责可靠传输、重传、排序和分片，不能把 KCP segment 头误当作 ET 的业务头。

大于 `10000` 字节的 KCP payload 会由 ET 额外分片：

```text
第一段：[4 字节 0][4 字节完整长度]
后续：按最多 10000 字节发送业务数据
```

对应实现：

- [KService.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/KService.cs)
- [KChannel.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/KChannel.cs)
- [Kcp.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/Kcp/Kcp.cs)

### 4. 接收路径

外网服务端接收流程：

```text
TCP/KCP 收到完整 ET 业务帧
  → MessageSerializeHelper.ToMessage
  → 读取 2 字节 opcode
  → OpcodeType.GetType(opcode)
  → MemoryPackHelper.Deserialize
  → NetComponent.OnRead
  → 按消息接口、场景和 Handler 分发
```

内网接收流程多一步：

```text
读取前 16 字节 FiberInstanceId
  → 读取 opcode
  → 反序列化消息
  → 按 FiberInstanceId 投递到目标纤程
```

外网服务端还会检查 opcode 是否位于外网范围，防止客户端直接发送内网消息。

## 三、后细节

### 1. opcode 的生成方式

Proto 文件名使用：

```text
<名称>_<C 或 S>_<起始值>.proto
```

例如：

```text
Login_C_10000.proto
```

生成器读取第三段 `10000`，然后按照 `message` 的出现顺序递增：

```text
第一个 message → 10001
第二个 message → 10002
第三个 message → 10003
```

例如当前登录协议：

```text
10001 Main2NetClient_Login
10002 NetClient2Main_Login
10003 C2G_Ping
10004 G2C_Ping
10005 C2R_Login
10006 R2C_Login
```

生成结果使用：

```csharp
[Message(Opcode.C2G_Ping)]
public partial class C2G_Ping : MessageObject, ISessionRequest
```

生成器实现：

- [Proto2CS.cs](../Packages/cn.etetet.proto/DotNet~/Proto2CS.cs)
- [Login_C_10000.proto](../Packages/cn.etetet.login/Proto/Login_C_10000.proto)

### 2. opcode、RpcId 和消息接口的区别

| 项目 | 作用 | 是否在所有消息中存在 |
| --- | --- | --- |
| `opcode` | 确定 C# 消息类型 | 是，位于业务 body 前 |
| `RpcId` | 匹配一次请求和响应 | 仅请求/响应约定使用 |
| `IRequest` | 标记需要响应的请求 | 仅请求 |
| `IResponse` | 标记响应并携带错误信息 | 仅响应 |
| `IMessage` | 普通消息/推送标记 | 推送消息通常使用 |
| `ResponseType` | 声明请求对应的响应类型 | 仅请求 |

例如：

```text
C2G_Ping  opcode=10003，RpcId=请求编号
G2C_Ping  opcode=10004，RpcId=同一个请求编号
```

`opcode` 不负责匹配请求响应，`RpcId` 才负责匹配。

### 3. MemoryPack body 的结构

MemoryPack body 没有通用的字段类型表，解析端先由 opcode 获得具体 C# 类型，再按该类型的字段顺序读取。

常见形式：

```text
对象：按 MemoryPackOrder 顺序写字段
字符串：[4 字节长度][UTF-8 或 UTF-16 内容]
数组/List：[4 字节元素数量][元素...]
Dictionary：[4 字节数量][key/value...]
嵌套对象：对象头 + 内部字段
```

请求通常以 `RpcId` 开头：

```text
[RpcId][业务字段...]
```

响应通常以以下字段开头：

```text
[RpcId][Error][Message][业务返回字段...]
```

### 4. 字节序

代码使用 `BitConverter` 和 `WriteTo` 读写整数，没有额外的网络字节序转换。因此当前 Windows/.NET 环境通常表现为小端序：

```text
ushort 10003 = 0x2713 → 13 27
```

长度字段、opcode、Fiber、InstanceId 都应按当前实现的小端方式解码。跨平台协议文档或独立抓包工具不能仅凭 TCP 常见习惯假定大端序。

### 5. 内外网信封对比

| 层 | 外网 | 内网 |
| --- | --- | --- |
| TCP 长度 | 2 字节 `ushort` | 4 字节 `int` |
| FiberInstanceId | 无 | 16 字节 |
| opcode | 2 字节 | 2 字节 |
| body | MemoryPack | MemoryPack |
| 主要用途 | 客户端到 Realm/Gate 等接入服务 | 服务间 Actor/Fiber 消息 |
| 外网 opcode 校验 | `10001~20000` | 不走外网校验 |

### 6. 一个外网 TCP 示例

假设：

```text
opcode = C2G_Ping = 10003
MemoryPack body 中 RpcId = 7
```

业务层至少可确定：

```text
[13 27][MemoryPack(C2G_Ping{RpcId=7})]
```

TCP 实际还要在前面增加 2 字节业务长度：

```text
[2 字节长度][13 27][MemoryPack body]
```

长度字段不包含自身，只包含：

```text
2 字节 opcode + MemoryPack body
```

### 7. 协议变更规则

opcode 是由 message 顺序生成的，因此已有协议发布后：

- 不要在已有 message 中间插入新 message；
- 不要改变文件名中的起始值；
- 不要复用已占用的 opcode；
- 新消息优先追加在文件末尾或使用新编号区间；
- 修改字段顺序或字段类型时，要同时评估 MemoryPack 兼容性；
- 客户端和服务端必须使用同一套生成后的 opcode 与消息模型。

运行时 `OpcodeType` 使用双向映射，重复类型或重复 opcode 会在初始化时抛错；但生成器按单个 proto 文件递增，跨文件区间冲突需要靠区间规划或运行时初始化暴露。

### 8. 代码索引

- [OpcodeRangeDefine.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/OpcodeRangeDefine.cs)：opcode 区间常量
- [OpcodeType.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/OpcodeType.cs)：类型与 opcode 双向映射
- [MessageSerializeHelper.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/MessageSerializeHelper.cs)：内外网业务信封构造和解析
- [AChannel.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/AChannel.cs)：opcode、FiberInstanceId 长度常量
- [PacketParser.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/PacketParser.cs)：TCP 长度帧解析
- [TChannel.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/TChannel.cs)：TCP 发送封装
- [KService.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/KService.cs)：KCP/UDP 控制头
- [KChannel.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/KChannel.cs)：KCP payload、分片和业务回调
- [IMessage.cs](../Packages/cn.etetet.core/Scripts/Core/Share/Network/IMessage.cs)：消息接口和 RPC 字段约定
- [Proto2CS.cs](../Packages/cn.etetet.proto/DotNet~/Proto2CS.cs)：proto 到 C# 与 opcode 的生成规则

### 9. 适用边界

本文描述当前 ET 源码中的协议实现，不描述 TLS、WebSocket 或第三方网关的额外封装。如果抓到的是原始 UDP 包，还需要先剥离 ET 的 `MSG + localConn` 外层和 KCP segment；如果抓到的是 TCP 字节流，还需要先按外网 2 字节或内网 4 字节长度切帧。
