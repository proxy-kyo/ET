$ErrorActionPreference = 'Stop'
$package = Split-Path $PSScriptRoot -Parent
$source = Get-Content -Raw (Join-Path $package 'Scripts/Hotfix/Server/Realm/C2R_LoginHandler.cs')
# Execute the actual leading guard; infrastructure after authentication is outside this isolated check.
$match = [regex]::Match($source, '(?s)protected override async ETTask Run\([^\r\n]+\)\s*\{(?<guard>.*?)EntityRef<Session>')
if (-not $match.Success) { throw 'Cannot locate Realm authentication guard.' }
$guard = $match.Groups['guard'].Value
Add-Type -TypeDefinition @"
public static class RealmCredentialCheck
{
    public sealed class Request { public string Account; public string Password; }
    public sealed class Response { public int Error; public string Message; public bool Passed; }
    private static class ErrorCode { public const int ERR_LoginAccountPasswordError = 1; }
    private static void Run(Request request, Response response)
    {
        $guard
        response.Passed = true;
    }
    public static void Check(string account, string password, bool expected)
    {
        var response = new Response();
        var request = new Request { Account = account, Password = password };
        Run(request, response);
        if (expected && (request.Account != "kyo" || request.Password != "111111"))
            throw new System.Exception("Credentials were not normalized for downstream login.");
        if (response.Passed != expected || (!expected && (response.Error != 1 || string.IsNullOrEmpty(response.Message))))
            throw new System.Exception("Unexpected Realm authentication result.");
    }
}
"@
[RealmCredentialCheck]::Check('kyo', '111111', $true)
[RealmCredentialCheck]::Check('other', '111111', $false)
[RealmCredentialCheck]::Check('kyo', 'wrong', $false)
[RealmCredentialCheck]::Check('Kyo', '111111', $false)
[RealmCredentialCheck]::Check(' kyo ', ' 111111 ', $true)
[RealmCredentialCheck]::Check("`tkyo`r`n", "`t111111`r`n", $true)
[RealmCredentialCheck]::Check($null, '111111', $false)
[RealmCredentialCheck]::Check('kyo', $null, $false)
[RealmCredentialCheck]::Check('   ', "`t", $false)
[RealmCredentialCheck]::Check('', '', $false)
[RealmCredentialCheck]::Check($null, $null, $false)
Write-Output 'PASS: 11 Realm credential cases (isolated source guard, not an integration test).'
