using System;

namespace ClashOfMinecraft
{
    internal class CrCooldownTimer
    {
        public TimeSpan TimeSpan { get; }
        public DateTime LastUsedTime { get; private set; }

        public CrCooldownTimer(long timeSpan) : this(new TimeSpan(timeSpan * TimeSpan.TicksPerMillisecond))
        {
        }

        public CrCooldownTimer(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
            LastUsedTime = DateTime.UtcNow.Subtract(TimeSpan);
        }

        public bool CanExecute()
        {
            return LastUsedTime.Add(TimeSpan) <= DateTime.UtcNow;
        }

        public bool Execute()
        {
            if (!CanExecute())
            {
                return false;
            }

            LastUsedTime = DateTime.UtcNow;

            return true;
        }
    }
}
