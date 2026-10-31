using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ET.Client
{
    [EntitySystemOf(typeof(LegendCliComponent))]
    public static partial class LegendCliComponentSystem
    {
        private const string LoadPassword = "load_password";

        [EntitySystem]
        private static void Awake(this LegendCliComponent self)
        {
            self.CancellationTokenSource = new CancellationTokenSource();
        }

        [EntitySystem]
        private static void Destroy(this LegendCliComponent self)
        {
            self.CancellationTokenSource.Cancel();
            self.CancellationTokenSource.Dispose();
            self.CancellationTokenSource = null;
        }

        public static void StartReading(this LegendCliComponent self)
        {
            self.ReadLines().Coroutine();
        }

        public static async ETTask<LegendCliClient> CreateClient(this LegendCliComponent self, bool spawned, bool print)
        {
            EntityRef<LegendCliComponent> selfRef = self;
            long fiberId = await self.Fiber().CreateFiber(SchedulerType.Parent,
                IdGenerater.Instance.GenerateId(), SceneType.Client, "LegendClient");
            self = selfRef;
            LegendCliClient client = new() { Index = ++self.NextIndex, FiberId = fiberId, Spawned = spawned };
            using LegendClient2Cli_Command response = await self.Call(client, "init", 60000,
                client.Index.ToString(CultureInfo.InvariantCulture), print ? "1" : "0");
            self = selfRef;
            if (response.Error != ErrorCode.ERR_Success)
            {
                throw new Exception(response.Message);
            }
            self.Clients.Add(client);
            return client;
        }

        private static async ETTask<LegendClient2Cli_Command> Call(this LegendCliComponent self,
            LegendCliClient client, string command, long timeout, params string[] args)
        {
            Cli2LegendClient_Command request = Cli2LegendClient_Command.Create();
            request.Command = command;
            foreach (string arg in args)
            {
                request.Args.Add(arg);
            }
            return (LegendClient2Cli_Command)await self.Root().GetComponent<ProcessInnerSender>()
                .Call(new FiberInstanceId(client.FiberId), request, timeout, needException: false);
        }

        private static async ETTask ReadLines(this LegendCliComponent self)
        {
            bool redirected = Console.IsInputRedirected;
            EntityRef<LegendCliComponent> selfRef = self;
            while (true)
            {
                try
                {
                    self = selfRef;
                    if (!redirected)
                    {
                        Console.Write("> ");
                    }
                    string line = await Task.Factory.StartNew(Console.In.ReadLine, self.CancellationTokenSource.Token);
                    self = selfRef;
                    if (self == null)
                    {
                        return;
                    }
                    if (line == null)
                    {
                        Environment.Exit(0);
                        return;
                    }
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }
                    bool success = await self.Execute(line);
                    if (!success && redirected)
                    {
                        Environment.Exit(1);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"error: {e.Message}");
                    if (redirected)
                    {
                        Environment.Exit(1);
                    }
                }
            }
        }

        private static async ETTask<bool> Execute(this LegendCliComponent self, string line)
        {
            string[] words = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            string command = words[0].ToLowerInvariant();
            command = command switch
            {
                "l" => "login",
                "a" => "attack",
                "hu" => "hunt",
                "m" => "move",
                "p" => "pos",
                "u" => "units",
                "w" => "wait",
                "n" => "new",
                "s" => "use",
                "sp" => "spawn",
                "st" => "stat",
                "h" or "?" => "help",
                _ => command,
            };
            if (command == "help")
            {
                Console.WriteLine("login (l) [账号 密码] - 登录并进图；不带参数时随机建号");
                Console.WriteLine("move (m) x y - 移动到格子");
                Console.WriteLine("attack (a) 单位Id - 攻击单位，不相邻时先走过去");
                Console.WriteLine("hunt (hu) [超时毫秒] - 寻找并击杀附近的野猪，默认 30000 毫秒");
                Console.WriteLine("pos (p) - 打印自己所在的格子");
                Console.WriteLine("units (u) - 列出视野内的单位，自己带 *");
                Console.WriteLine("wait (w) at x y [超时毫秒] - 等自己到达格子，默认 10000 毫秒");
                Console.WriteLine("new (n) - 新建客户端并切换过去");
                Console.WriteLine("use (s) 序号 - 切换当前客户端");
                Console.WriteLine("ls - 列出所有客户端");
                Console.WriteLine("spawn (sp) 数量 wander - 启动压测客户端");
                Console.WriteLine("stat (st) - 显示在线数、登录失败数和移动延迟");
                Console.WriteLine("help (h/?) - 显示本帮助");
                return true;
            }

            EntityRef<LegendCliComponent> selfRef = self;
            if (command == "new")
            {
                if (words.Length != 1)
                {
                    Console.WriteLine("error: usage: new");
                    return false;
                }
                using (LegendClient2Cli_Command old = await self.Call(self.Clients[self.CurrentIndex - 1], "print", 60000, "0"))
                {
                    if (!Check(old)) return false;
                }
                self = selfRef;
                LegendCliClient client = await self.CreateClient(false, true);
                self = selfRef;
                self.CurrentIndex = client.Index;
                Console.WriteLine($"client {client.Index}");
                return true;
            }
            if (command == "use")
            {
                if (words.Length != 2 || !int.TryParse(words[1], out int index))
                {
                    Console.WriteLine("error: usage: use index");
                    return false;
                }
                LegendCliClient client = self.Clients.Find(c => c.Index == index);
                if (client == null)
                {
                    Console.WriteLine($"error: client {index} not found");
                    return false;
                }
                if (index != self.CurrentIndex)
                {
                    using (LegendClient2Cli_Command old = await self.Call(self.Clients[self.CurrentIndex - 1], "print", 60000, "0"))
                    {
                        if (!Check(old)) return false;
                    }
                    self = selfRef;
                    using (LegendClient2Cli_Command current = await self.Call(client, "print", 60000, "1"))
                    {
                        if (!Check(current)) return false;
                    }
                    self = selfRef;
                    self.CurrentIndex = index;
                }
                Console.WriteLine($"client {index}");
                return true;
            }
            if (command == "ls")
            {
                if (words.Length != 1)
                {
                    Console.WriteLine("error: usage: ls");
                    return false;
                }
                for (int i = 0; i < self.Clients.Count; ++i)
                {
                    LegendCliClient client = self.Clients[i];
                    using LegendClient2Cli_Command info = await self.Call(client, "info", 60000);
                    self = selfRef;
                    if (!Check(info)) return false;
                    Console.WriteLine($"{client.Index} {info.Message}{(client.Spawned ? " spawned" : "")}{(client.Index == self.CurrentIndex ? " *" : "")}");
                }
                return true;
            }
            if (command == "spawn")
            {
                if (words.Length != 3 || !int.TryParse(words[1], out int count) || count <= 0 || words[2] != "wander")
                {
                    Console.WriteLine("error: usage: spawn count wander");
                    return false;
                }
                for (int i = 0; i < count; ++i)
                {
                    LegendCliClient client = await self.CreateClient(true, false);
                    self = selfRef;
                    string account = $"load_{++self.SpawnIndex}";
                    using LegendClient2Cli_Command response = await self.Call(client, "wander", 60000, account, LoadPassword);
                    self = selfRef;
                    if (!Check(response)) return false;
                }
                Console.WriteLine($"spawned {count}");
                return true;
            }
            if (command == "stat")
            {
                if (words.Length != 1)
                {
                    Console.WriteLine("error: usage: stat");
                    return false;
                }
                int online = 0;
                int loginFailed = 0;
                List<long> latencies = new();
                for (int i = 0; i < self.Clients.Count; ++i)
                {
                    using LegendClient2Cli_Command response = await self.Call(self.Clients[i], "stat", 60000);
                    self = selfRef;
                    if (!Check(response)) return false;
                    string[] values = response.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    online += int.Parse(values[0], CultureInfo.InvariantCulture);
                    loginFailed += int.Parse(values[1], CultureInfo.InvariantCulture);
                    for (int j = 2; j < values.Length; ++j)
                    {
                        latencies.Add(long.Parse(values[j], CultureInfo.InvariantCulture));
                    }
                }
                latencies.Sort();
                string average = "-";
                string p99 = "-";
                if (latencies.Count > 0)
                {
                    long total = 0;
                    foreach (long latency in latencies) total += latency;
                    average = ((double)total / latencies.Count).ToString("F1", CultureInfo.InvariantCulture);
                    p99 = latencies[(int)Math.Ceiling(0.99 * latencies.Count) - 1].ToString(CultureInfo.InvariantCulture);
                }
                Console.WriteLine($"online={online} login_failed={loginFailed} move_latency_avg={average} p99={p99} samples={latencies.Count}");
                return true;
            }
            if (command is not ("login" or "move" or "pos" or "units" or "wait" or "attack" or "hunt"))
            {
                Console.WriteLine($"error: unknown command: {command}");
                return false;
            }
            long timeout = 60000;
            if (command == "attack")
            {
                // ponytail: 走过去的时间随距离变，主纤程只给一个宽松上限。
                timeout = 120000;
            }
            if (command == "wait")
            {
                int waitTimeout = 10000;
                if (words.Length == 5 && (!int.TryParse(words[4], out waitTimeout) || waitTimeout < 0))
                {
                    Console.WriteLine("error: invalid wait timeout");
                    return false;
                }
                timeout = (long)waitTimeout + 5000;
            }
            if (command == "hunt")
            {
                int huntTimeout = 30000;
                if (words.Length > 2 || (words.Length == 2 && (!int.TryParse(words[1], out huntTimeout) || huntTimeout < 0)))
                {
                    Console.WriteLine("error: invalid hunt timeout");
                    return false;
                }
                timeout = (long)huntTimeout + 5000;
            }
            using LegendClient2Cli_Command result = await self.Call(self.Clients[self.CurrentIndex - 1], command, timeout, words[1..]);
            if (!Check(result)) return false;
            if (!string.IsNullOrEmpty(result.Message))
            {
                Console.WriteLine(result.Message);
            }
            return true;
        }

        private static bool Check(LegendClient2Cli_Command response)
        {
            if (response.Error == ErrorCode.ERR_Success) return true;
            Console.WriteLine($"error: {response.Message}");
            return false;
        }
    }
}
