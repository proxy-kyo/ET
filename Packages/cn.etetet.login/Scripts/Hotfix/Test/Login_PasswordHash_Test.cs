using System.Linq;
using ET.Server;

namespace ET.Test
{
    public class Login_PasswordHash_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            byte[] salt = new byte[16];
            byte[] otherSalt = new byte[16];
            otherSalt[0] = 1;
            byte[] first = await LoginPasswordHelper.Hash("abc12345", salt);
            byte[] second = await LoginPasswordHelper.Hash("abc12345", salt);
            byte[] other = await LoginPasswordHelper.Hash("abc12345", otherSalt);
            if (first.Length != 32 || !first.SequenceEqual(second)) return 1;
            if (first.SequenceEqual(other)) return 2;
            if (!await LoginPasswordHelper.Verify("abc12345", salt, first)) return 3;
            if (await LoginPasswordHelper.Verify("wrong123", salt, first)) return 4;
            if (await LoginPasswordHelper.Verify(" abc12345 ", salt, first)) return 5;
            return ErrorCode.ERR_Success;
        }
    }
}
