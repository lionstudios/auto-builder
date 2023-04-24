using System;
using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class SignInWithAppleCapability : Capability<EmptyCapabilitySettings>
    {
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddSignInWithApple();
        }
    }

}