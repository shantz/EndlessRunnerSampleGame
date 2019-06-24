using System;
using UnityEngine;

namespace PlayerHappiness
{
    public interface ISensor
    {
        string name { get; }
        
        void SetContext(ICollectorContext context);
        void Start();
        CustomYieldInstruction Stop();
    }
}
