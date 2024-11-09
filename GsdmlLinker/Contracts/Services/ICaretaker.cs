using GsdmlLinker.Core.Contracts;

namespace GsdmlLinker.Contracts.Services;

public interface ICaretaker
{
    public void Backup(Contracts.Builders.IModuleBuilder memento);
}
