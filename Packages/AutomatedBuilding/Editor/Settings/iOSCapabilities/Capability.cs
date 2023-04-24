using System;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public abstract class Capability<T> : ICapability where T : CapabilitySettings
    {

        [SerializeField] protected bool enabled;

        [SerializeField] public T settings = default;

        public void AddCapabilityIfEnabled(ProjectCapabilityManager capabilityManager)
        {
            if (enabled)
                AddCapability(capabilityManager);
        }

        public abstract void AddCapability(ProjectCapabilityManager capabilityManager);

    }

}