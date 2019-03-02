namespace FailToBan.Server
{
    public abstract class ShellState
    {
        protected Shell shell;

        protected ShellState(Shell shell)
        {
            this.shell = shell;
        }

        public abstract string Handle(string[] messages);
    }
}