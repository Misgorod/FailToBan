namespace FailToBan.Server
{
    public class CreateNameState : ShellState
    {
        public CreateNameState(Shell shell) : base(shell)
        { }

        public override string Handle(string[] messages)
        {
            return "Оболочка создана";
        }
    }
}