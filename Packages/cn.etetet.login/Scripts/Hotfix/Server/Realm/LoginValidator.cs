namespace ET.Server
{
    public static class LoginValidator
    {
        public static int Validate(string account, string password, out string name)
        {
            name = (account ?? string.Empty).Trim().ToLowerInvariant();
            if (name.Length < 3 || name.Length > 16)
            {
                return ErrorCode.ERR_LoginAccountInvalid;
            }
            foreach (char c in name)
            {
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '_')
                {
                    return ErrorCode.ERR_LoginAccountInvalid;
                }
            }
            return password == null || password.Length < 6 || password.Length > 32
                ? ErrorCode.ERR_LoginPasswordInvalid : ErrorCode.ERR_Success;
        }
    }
}
