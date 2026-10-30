using ET.Server;

namespace ET.Test
{
    public class Login_Validator_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            if (LoginValidator.Validate("ab", "abc123", out _) != ErrorCode.ERR_LoginAccountInvalid) return 1;
            if (LoginValidator.Validate(new string('a', 17), "abc123", out _) != ErrorCode.ERR_LoginAccountInvalid) return 2;
            if (LoginValidator.Validate("ab-c", "abc123", out _) != ErrorCode.ERR_LoginAccountInvalid) return 3;
            if (LoginValidator.Validate(" Ab_C12 ", "abc123", out string name) != 0 || name != "ab_c12") return 4;
            if (LoginValidator.Validate("abc", "12345", out _) != ErrorCode.ERR_LoginPasswordInvalid) return 5;
            if (LoginValidator.Validate("abc", new string('a', 33), out _) != ErrorCode.ERR_LoginPasswordInvalid) return 6;
            if (LoginValidator.Validate(new string('a', 16), new string('b', 32), out _) != 0) return 7;
            if (LoginValidator.Validate("abc", " 1234 ", out _) != 0) return 8;
            if (LoginValidator.Validate(null, "abc123", out _) != ErrorCode.ERR_LoginAccountInvalid) return 9;
            if (LoginValidator.Validate("abc", null, out _) != ErrorCode.ERR_LoginPasswordInvalid) return 10;
            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }
    }
}
