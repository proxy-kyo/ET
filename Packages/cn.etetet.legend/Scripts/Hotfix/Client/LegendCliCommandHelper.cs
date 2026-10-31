using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Mathematics;

namespace ET.Client
{
    public static class LegendCliCommandHelper
    {
        public static async ETTask Execute(Scene root, Cli2LegendClient_Command request, LegendClient2Cli_Command response)
        {
            List<string> args = request.Args;
            if (request.Command == "init")
            {
                LegendCliClientComponent state = root.AddComponent<LegendCliClientComponent>();
                state.Index = int.Parse(args[0]);
                state.Print = args[1] == "1";
                return;
            }
            LegendCliClientComponent client = root.GetComponent<LegendCliClientComponent>() ?? root.AddComponent<LegendCliClientComponent>();
            switch (request.Command)
            {
                case "print":
                    client.Print = args[0] == "1";
                    return;
                case "info":
                    response.Message = $"{client.Account ?? "-"} {(client.Online ? "online" : "offline")}";
                    return;
                case "stat":
                    response.Message = $"{(client.Online ? 1 : 0)} {client.LoginFailed} {string.Join(" ", client.MoveLatencies)}";
                    return;
                case "wander":
                    root.Wander(args[0], args[1]).Coroutine();
                    return;
            }
            if (request.Command == "login")
            {
                await Login(root, args, response);
                return;
            }
            if (request.Command == "wait")
            {
                if (args.Count == 0 || args[0] != "at")
                {
                    Fail(response, "unknown wait condition");
                    return;
                }
                int timeout = 10000;
                if ((args.Count != 3 && args.Count != 4) ||
                    !int.TryParse(args[1], out int x) || !int.TryParse(args[2], out int y) ||
                    (args.Count == 4 && (!int.TryParse(args[3], out timeout) || timeout < 0)))
                {
                    Fail(response, "usage: wait at x y [timeoutMs]");
                    return;
                }
                await WaitAt(root, new int2(x, y), timeout, response);
                return;
            }
            Scene scene = root.CurrentScene();
            GridMapComponent map = scene?.GetComponent<GridMapComponent>();
            if (map == null)
            {
                Fail(response, "not in map");
                return;
            }
            switch (request.Command)
            {
                case "move":
                    if (args.Count != 2 || !int.TryParse(args[0], out int x) || !int.TryParse(args[1], out int y))
                    {
                        Fail(response, "usage: move x y");
                        return;
                    }
                    int2 cell = new(x, y);
                    if (!GridHelper.InBounds(map, cell))
                    {
                        Fail(response, "cell out of bounds");
                        return;
                    }
                    LegendMoveHelper.MoveTo(root, cell);
                    response.Message = $"move -> {x} {y}";
                    return;
                case "pos":
                    if (args.Count != 0)
                    {
                        Fail(response, "usage: pos");
                        return;
                    }
                    Unit unit = scene.GetComponent<UnitComponent>().Get(root.GetComponent<PlayerComponent>().MyId);
                    if (unit == null)
                    {
                        Fail(response, "own unit not found");
                        return;
                    }
                    int2 position = GridHelper.WorldToCell(map, unit.Position);
                    response.Message = $"{position.x} {position.y}";
                    return;
                case "units":
                    if (args.Count != 0)
                    {
                        Fail(response, "usage: units");
                        return;
                    }
                    UnitComponent units = scene.GetComponent<UnitComponent>();
                    long myId = root.GetComponent<PlayerComponent>().MyId;
                    StringBuilder result = new();
                    result.Append($"count={units.Children.Count}");
                    foreach (Unit visible in units.Children.Values)
                    {
                        int2 unitCell = GridHelper.WorldToCell(map, visible.Position);
                        result.AppendLine();
                        result.Append($"{visible.Id} {unitCell.x} {unitCell.y}");
                        if (visible.Id == myId)
                        {
                            result.Append(" *");
                        }
                    }
                    response.Message = result.ToString();
                    return;
                default:
                    Fail(response, $"unknown command: {request.Command}");
                    return;
            }
        }

        private static async ETTask Login(Scene root, List<string> args, LegendClient2Cli_Command response)
        {
            if (args.Count != 0 && args.Count != 2)
            {
                Fail(response, "usage: login [account password]");
                return;
            }
            string account = args.Count == 2 ? args[0] : "r" + RandomText(8);
            string password = args.Count == 2 ? args[1] : RandomText(12);
            await Login(root, account, password, response);
        }

        private static async ETTask<bool> Login(Scene root, string account, string password, LegendClient2Cli_Command response)
        {
            LegendCliClientComponent client = root.GetComponent<LegendCliClientComponent>();
            string address = Options.Instance.Address;
            if (string.IsNullOrEmpty(address))
            {
                try
                {
                    foreach (string line in File.ReadLines("Packages/com.etetet.init/Resources/GlobalConfig.asset"))
                    {
                        string value = line.Trim();
                        if (value.StartsWith("Address:", StringComparison.Ordinal))
                        {
                            address = value.Substring("Address:".Length).Trim();
                            break;
                        }
                    }
                }
                catch (IOException e)
                {
                    ++client.LoginFailed;
                    Fail(response, e.Message);
                    return false;
                }
                catch (UnauthorizedAccessException e)
                {
                    ++client.LoginFailed;
                    Fail(response, e.Message);
                    return false;
                }
                if (string.IsNullOrEmpty(address))
                {
                    ++client.LoginFailed;
                    Fail(response, "GlobalConfig Address not found");
                    return false;
                }
            }
            EntityRef<Scene> rootRef = root;
            try
            {
                await LoginHelper.Login(root, address, account, password);
            }
            catch (RpcException rpc)
            {
                root = rootRef;
                if (root != null) ++root.GetComponent<LegendCliClientComponent>().LoginFailed;
                Fail(response, $"{rpc.Error}: {rpc.Message}");
                return false;
            }
            root = rootRef;
            if (root == null) return false;
            client = root.GetComponent<LegendCliClientComponent>();
            client.Account = account;
            client.Online = true;
            response.Message = $"login ok account={account} password={password} unit={root.GetComponent<PlayerComponent>().MyId}";
            return true;
        }

        [EnableGetComponent(typeof(TimerComponent))]
        private static async ETTask Wander(this Scene root, string account, string password)
        {
            EntityRef<Scene> rootRef = root;
            using LegendClient2Cli_Command login = LegendClient2Cli_Command.Create();
            if (!await Login(root, account, password, login)) return;
            while (true)
            {
                root = rootRef;
                if (root == null) return;
                Scene scene = root.CurrentScene();
                GridMapComponent map = scene?.GetComponent<GridMapComponent>();
                Unit unit = scene?.GetComponent<UnitComponent>().Get(root.GetComponent<PlayerComponent>().MyId);
                if (map == null || unit == null)
                {
                    await root.GetComponent<TimerComponent>().WaitAsync(50);
                    root = rootRef;
                    continue;
                }
                int2 position = GridHelper.WorldToCell(map, unit.Position);
                int2 target;
                do
                {
                    target = new int2(position.x + RandomGenerator.RandomNumber(-8, 9),
                        position.y + RandomGenerator.RandomNumber(-8, 9));
                } while (target.Equals(position) || !GridHelper.InBounds(map, target));
                LegendMoveHelper.MoveTo(root, target);
                using (LegendClient2Cli_Command wait = LegendClient2Cli_Command.Create())
                {
                    await WaitAt(root, target, 5000, wait);
                }
                root = rootRef;
                if (root == null) return;
                await root.GetComponent<TimerComponent>().WaitAsync(RandomGenerator.RandomNumber(500, 1501));
                root = rootRef;
            }
        }

        private static string RandomText(int length)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
            char[] text = new char[length];
            for (int i = 0; i < text.Length; ++i)
            {
                text[i] = alphabet[RandomGenerator.RandomNumber(0, alphabet.Length)];
            }
            return new string(text);
        }

        [EnableGetComponent(typeof(TimerComponent))]
        private static async ETTask WaitAt(Scene root, int2 target, int timeout, LegendClient2Cli_Command response)
        {
            EntityRef<Scene> rootRef = root;
            long deadline = root.GetSingleton<TimeInfo>().ClientNow() + timeout;
            while (true)
            {
                Scene scene = root.CurrentScene();
                GridMapComponent map = scene?.GetComponent<GridMapComponent>();
                Unit unit = scene?.GetComponent<UnitComponent>().Get(root.GetComponent<PlayerComponent>().MyId);
                // 自己的 Appear 可能晚于进图 RPC 到达，没看到自己时继续等。
                int2? cell = map == null || unit == null ? null : GridHelper.WorldToCell(map, unit.Position);
                if (cell.HasValue && cell.Value.Equals(target))
                {
                    response.Message = $"at {target.x} {target.y}";
                    return;
                }
                if (root.GetSingleton<TimeInfo>().ClientNow() >= deadline)
                {
                    string now = cell.HasValue ? $"{cell.Value.x} {cell.Value.y}" : "unknown";
                    Fail(response, $"wait timeout: at {target.x} {target.y}, now {now}");
                    return;
                }
                await root.GetComponent<TimerComponent>().WaitAsync(50);
                root = rootRef;
                if (root == null) return;
            }
        }

        private static void Fail(LegendClient2Cli_Command response, string message)
        {
            response.Error = ErrorCode.ERR_LegendCliCommand;
            response.Message = message;
        }
    }
}
