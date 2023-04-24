using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class InterAppAudioCapability : Capability<EmptyCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddInterAppAudio();
        }
    }

}