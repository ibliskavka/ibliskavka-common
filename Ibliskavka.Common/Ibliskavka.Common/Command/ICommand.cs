namespace Ibliskavka.Common.Command
{
    /// <summary>
    /// Interface for command objects.
    /// </summary>
    public interface ICommand
    {
        bool Execute();
        bool Undo();
        string GetExecuteMessage();
        string GetUndoMessage();
    }
}