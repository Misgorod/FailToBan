using System;

namespace FailToBan.Server
{
    public class BanInfo
    {
        public BanInfo(BanType type, DateTime banTime, string service)
        {
            Type = type;
            BanTime = banTime;
            Service = service;
        }

        public BanType Type { get; }
        public DateTime BanTime { get; }
        public string Service { get; }

        public enum BanType
        {
            Ban,
            Unban,
        }

    }
}