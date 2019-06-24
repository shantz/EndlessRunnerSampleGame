using System;

namespace PlayerHappiness
{
    public interface ISensor
    {
        string name { get; }
        
        void SetContext(ICollectorContext context);
        void Start();
        void Stop();
    }
}
