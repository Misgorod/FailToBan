using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class PrepareEditState : ShellState
    {
        public PrepareEditState
        (
            Shell shell, 
            IServiceContainer serviceContainer, 
            IServiceFactory serviceFactory, 
            ISettingFactory settingFactory,
            IServiceSaver jailSaver, 
            IServiceSaver filterSaver
        ) : base(shell, serviceContainer, serviceFactory, settingFactory, jailSaver, filterSaver)
        { }

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