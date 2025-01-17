using System;
using UnityEngine;

namespace PlayerHappiness
{
    public interface IFrame : IDisposable
    {
        void Write(string name, string value);
        void Write(string name, float value);
        void Write(string name, int value);
        void Write(string name, Vector2 value);
        void Write(string name, Vector3 value);
        void Write(string name, Quaternion value);
    }
}


