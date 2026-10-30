using System.Collections.Generic;
using ET.Server;
using MongoDB.Driver;

namespace ET.Test
{
    public class Login_Account_Test : ATestHandler
    {
        public override async ETTask<int> Handle(TestContext context)
        {
            await using TestFiberScope scope = await TestFiberScope.Create(context.Fiber, SceneType.TestEmpty,
                nameof(Login_Account_Test));
            Scene root = scope.TestFiber.Root;
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.GetComponent<TestFiberDatabaseCleanupComponent>().RegisterLogicalDbName(nameof(Account));
            DBComponent db = root.AddComponent<DBManagerComponent>().GetZoneDB(root.Zone());
            EntityRef<DBComponent> dbRef = db;
            await AccountHelper.CreateIndexes(db);
            db = dbRef;
            (int error, long playerId) = await AccountHelper.LoginOrRegister(db, "acct_check", "abc12345");
            if (error != 0 || playerId <= 0) return 1;
            db = dbRef;
            (int secondError, long secondId) = await AccountHelper.LoginOrRegister(db, "acct_check", "abc12345");
            if (secondError != 0 || secondId != playerId) return 2;
            db = dbRef;
            (int wrongError, long wrongId) = await AccountHelper.LoginOrRegister(db, "acct_check", "wrong123");
            if (wrongError != ErrorCode.ERR_LoginAccountPasswordError || wrongId != 0) return 3;
            db = dbRef;
            List<Account> saved = await db.Query<Account>(a => a.Name == "acct_check");
            if (saved.Count != 1 || saved[0].Id <= 0 || saved[0].Salt.Length != 16 ||
                saved[0].PasswordHash.Length != 32 || saved[0].CharacterIds[0] != playerId || saved[0].CreateTime <= 0) return 4;
            long accountId = saved[0].Id;
            db = dbRef;
            IMongoCollection<Account> collection = db.database.GetCollection<Account>(nameof(Account));
            // Each rejected insert must leave the original account intact.
            for (int conflict = 0; conflict < 3; ++conflict)
            {
                db = dbRef;
                Account duplicate = db.AddChildWithId<Account>(conflict == 0 ? accountId : CharacterIdHelper.New());
                using EntityRef<Account> duplicateRef = duplicate;
                duplicate.Name = conflict == 1 ? "acct_check" : "another_account";
                duplicate.CharacterIds.Add(conflict == 2 ? playerId : CharacterIdHelper.New());
                try
                {
                    await collection.InsertOneAsync(duplicate);
                    return 5 + conflict;
                }
                catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                }
            }
            db = dbRef;
            if (await db.Count<Account>() != 1) return 8;
            db = dbRef;
            (int finalError, long finalId) = await AccountHelper.LoginOrRegister(db, "acct_check", "abc12345");
            return finalError == 0 && finalId == playerId ? ErrorCode.ERR_Success : 9;
        }
    }
}
