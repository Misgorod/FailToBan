namespace FailToBan.Server.Shells
{
    public class PrepareEditState : ShellState
    {
        public PrepareEditState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.Prepare];
        }

        public override string Handle(string[] messages)
        {
            shell.SetState(new EditNameState(this));
            return "Оболочка создана";
        }
    }
}