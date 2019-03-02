namespace FailToBan.Server
{
    public class PrepareEditState : ShellState
    {
        public PrepareEditState(Shell shell) : base(shell)
        { }

        public override string Handle(string[] messages)
        {
            shell.SetState(new EditNameState(shell));
            return "Оболочка создана";
        }
    }
}