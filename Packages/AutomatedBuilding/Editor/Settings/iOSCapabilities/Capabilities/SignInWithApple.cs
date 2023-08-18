using System;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    [Serializable]
    public class SignInWithAppleCapability : Capability<EmptyCapabilitySettings>
    {
        public SignInWithAppleCapability(bool enabled) : base(enabled) {}
        
#if UNITY_IOS
        public override void AddCapability(ProjectCapabilityManager capabilityManager)
        {
            capabilityManager.AddSignInWithApple();
        }
#endif
    }

}