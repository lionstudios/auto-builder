using UnityEditor.iOS.Xcode;

namespace LionStudios.Editor.AutoBuilder
{

    public interface ICapability
    {

        void AddCapabilityIfEnabled(ProjectCapabilityManager capabilityManager);

    }

}
