namespace lib;

public interface ICommand
{
    string Name { get; }
    string Description { get; }

    int Execute();
}
