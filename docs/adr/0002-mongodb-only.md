# 只用 MongoDB 做持久化

所有持久数据都存 MongoDB。PostgreSQL 和 Redis 本地虽然都有，但故意不用：一个人开发、单进程、300 人以内在线，维护三套存储的运维、备份和一致性都是纯成本。会话、在线状态、排行榜放在进程内存里。等真正出现多进程共享或强事务需求时，再引入 Redis 或 PostgreSQL（Fantasy 同时支持 MongoDB 和 PostgreSQL）。
