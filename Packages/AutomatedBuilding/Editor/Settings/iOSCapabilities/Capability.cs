using System;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public abstract class Capability<T> : ICapability where T : CapabilitySettings
    {

        [SerializeField] protected bool enabled;

        [SerializeField] public T settings = default;

#if UNITY_IOS
        public void AddCapabilityIfEnabled(ProjectCapabilityManager capabilityManager)
        {
            if (enabled)
                AddCapability(capabilityManager);
        }

        public abstract void AddCapability(ProjectCapabilityManager capabilityManager);
#endif

    }

}