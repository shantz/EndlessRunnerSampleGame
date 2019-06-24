using System;

namespace PlayerHappiness
{
    public interface ISensor
    {
        void SetContext(ICollectorContext context);
        void Start();
        void Stop();
    }
}
