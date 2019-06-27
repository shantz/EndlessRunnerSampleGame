using System;
using UnityEngine;

namespace PlayerHappiness
{
    interface ISensor
    {
        string name { get; }
        bool useFrames { get; }
        int[] projectedValues { get; }
        
        void SetContext(ICollectorContext context);
        void Start();
        CustomYieldInstruction Stop();
    }
}
