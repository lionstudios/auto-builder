#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LionStudios.Editor.AutoBuilder
{

    public interface ICapability
    {

#if UNITY_IOS
        void AddCapabilityIfEnabled(ProjectCapabilityManager capabilityManager);
#endif

    }

}
