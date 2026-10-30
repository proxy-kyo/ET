namespace ET.Server
{
    public static class LoginRateLimitComponentSystem
    {
        public static bool TryAcquire(this LoginRateLimitComponent self, string ip, long now)
        {
            if (!self.Buckets.TryGetValue(ip, out var bucket))
            {
                bucket = (LoginConst.MaxLoginPerSecondPerIP, now);
            }

            double tokens = System.Math.Min(LoginConst.MaxLoginPerSecondPerIP,
                bucket.Tokens + (now - bucket.Time) * LoginConst.MaxLoginPerSecondPerIP / 1000.0);
            bool allowed = tokens >= 1;
            self.Buckets[ip] = (allowed ? tokens - 1 : tokens, now);
            return allowed;
        }
    }
}
