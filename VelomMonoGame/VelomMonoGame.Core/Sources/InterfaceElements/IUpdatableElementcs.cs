

namespace VelomMonoGame.Core.Sources.InterfaceElements;

internal interface IUpdatableElement
{
    void Update();
    bool IsUpdatable { get; set; }
}
