using System.Collections.Generic;
using UnityEngine;

namespace PlayerHappiness
{
    public struct FrameData<T>
    {
        public string name;
        public T value;
    }
    
    public class FrameInfo
    {
        public float timestamp;
        public List<FrameData<float>> floats;
        public List<FrameData<int>> ints;
        public List<FrameData<Vector2>> vector2s;
        public List<FrameData<Vector3>> vector3s;
        public List<FrameData<Quaternion>> quaternions;
        public List<FrameData<string>> strings;

        public FrameInfo(float timestamp, int capacity)
        {
            this.timestamp = timestamp;
            floats = new List<FrameData<float>>(capacity);
            ints = new List<FrameData<int>>(capacity);
            strings = new List<FrameData<string>>(capacity);
            vector2s = new List<FrameData<Vector2>>(capacity);
            vector3s = new List<FrameData<Vector3>>(capacity);
            quaternions = new List<FrameData<Quaternion>>(capacity);
        }
    }
}
