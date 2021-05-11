using System;
using UnityEngine;

namespace Pathfinding.Configs
{
    public class EditorConfigManager : ConfigManagerBase
    {
        protected override ConfigBase Create(Type type)
        {
            var instance = (ConfigBase) ScriptableObject.CreateInstance(type);
            return instance;
        }
    }
}