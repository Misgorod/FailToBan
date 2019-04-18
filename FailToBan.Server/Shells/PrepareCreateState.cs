using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class PrepareCreateState : ShellState
    {
        public PrepareCreateState(
            Shell shell,
            IServiceContainer serviceContainer,
            IServiceFactory serviceFactory,
            ISettingFactory settingFactory,
            IServiceSaver jailSaver,
            IServiceSaver filterSaver
        ) : base(shell, serviceContainer, serviceFactory, settingFactory, jailSaver, filterSaver)
        { }

        public PrepareCreateState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.Prepare];
        }

        public override string Handle(string[] values)
        {
            shell.SetState(new CreateNameState(this));
            return "Оболочка создана";
        }
    }
}