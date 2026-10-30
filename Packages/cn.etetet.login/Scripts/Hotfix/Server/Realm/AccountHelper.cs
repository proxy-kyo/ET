using System.Collections.Generic;
using System.Security.Cryptography;
using MongoDB.Driver;

namespace ET.Server
{
    public static class AccountHelper
    {
        public static async ETTask CreateIndexes(DBComponent db)
        {
            IMongoCollection<Account> accounts = db.database.GetCollection<Account>(nameof(Account));
            await accounts.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<Account>(Builders<Account>.IndexKeys.Ascending(a => a.Name),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<Account>(Builders<Account>.IndexKeys.Ascending(a => a.CharacterIds),
                    new CreateIndexOptions { Unique = true })
            });
        }

        public static async ETTask<(int Error, long PlayerId)> LoginOrRegister(DBComponent db, string name, string password)
        {
            EntityRef<DBComponent> dbRef = db;
            using (await db.Root().CoroutineLockComponent.Wait(CoroutineLockType.LoginAccount, name.GetLongHashCode()))
            {
                db = dbRef;
                List<Account> existing = await db.Query<Account>(a => a.Name == name);
                if (existing.Count != 0)
                {
                    return await Login(existing[0], password);
                }

                byte[] salt = new byte[16];
                using (RandomNumberGenerator random = RandomNumberGenerator.Create())
                {
                    random.GetBytes(salt);
                }
                byte[] hash = await LoginPasswordHelper.Hash(password, salt);
                for (int attempt = 0; attempt < 3; ++attempt)
                {
                    db = dbRef;
                    Account account = db.AddChildWithId<Account>(CharacterIdHelper.New());
                    using EntityRef<Account> accountRef = account;
                    long playerId = CharacterIdHelper.New();
                    account.Name = name;
                    account.Salt = salt;
                    account.PasswordHash = hash;
                    account.CharacterIds.Add(playerId);
                    account.CreateTime = db.GetSingleton<TimeInfo>().ServerNow();
                    try
                    {
                        await db.database.GetCollection<Account>(nameof(Account)).InsertOneAsync(account);
                        return (ErrorCode.ERR_Success, playerId);
                    }
                    catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    {
                        db = dbRef;
                        existing = await db.Query<Account>(a => a.Name == name);
                        if (existing.Count != 0)
                        {
                            return await Login(existing[0], password);
                        }
                        if (attempt == 2)
                        {
                            throw;
                        }
                    }
                }
            }
            throw new System.InvalidOperationException("Account insert retry exhausted.");
        }

        private static async ETTask<(int Error, long PlayerId)> Login(Account account, string password)
        {
            long playerId = account.CharacterIds[0];
            byte[] salt = account.Salt;
            byte[] hash = account.PasswordHash;
            bool valid = await LoginPasswordHelper.Verify(password, salt, hash);
            return valid ? (ErrorCode.ERR_Success, playerId) : (ErrorCode.ERR_LoginAccountPasswordError, 0);
        }
    }
}
