using GsdmlLinker.Contracts.Services;
using GsdmlLinker.Core.Contracts;

namespace GsdmlLinker.Services;

public class Caretaker : ICaretaker
{
    private List<Contracts.Builders.IModuleBuilder> state = [];
    private int index = -1;

    public void Backup(Contracts.Builders.IModuleBuilder memento)
    {
        state.Add(memento);
        index++;
    }


}
