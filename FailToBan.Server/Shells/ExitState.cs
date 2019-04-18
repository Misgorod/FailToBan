using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class ExitState : ShellState
    {
        public ExitState(Shell shell, IServiceContainer serviceContainer, IServiceFactory serviceFactory, ISettingFactory settingFactory, IServiceSaver jailSaver, IServiceSaver filterSaver) : base(shell, serviceContainer, serviceFactory, settingFactory, jailSaver, filterSaver)
        {
        }

        public ExitState(ShellState state) : base(state)
        {
        }

        public override string Handle(string[] values)
        {
            return Constants.ShellTexts[Constants.ShellSteps.Exit];
        }
    }
}