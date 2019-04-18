using System;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class CreateNameState : ShellState
    {
        public CreateNameState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.CreateRuleName];
        }

        public override string Handle(string[] values)
        {
            if (values[0] == "--List")
            {
                return PrintDefaultRules();
            }

            if (values.Length == 0)
            {
                return "Неверное количество аргументов";
            }

            var serviceName = values[0];
            currentJail = serviceContainer.GetJail(serviceName);

            if (currentJail != null)
            {
                return "Сервис с таким именем уже существует\n" +
                       "Для изменения существующего сервиса используйте команду edit";
            }

            var localSetting = settingFactory.Build();
            currentJail = serviceFactory.BuildJail(serviceName, localSetting, serviceContainer.GetDefault());

            //if (currentJail.GetRule(currentJail.Name, RuleType.Port) == null)
            if (currentJail.LocalSetting?.GetSection(currentJail.Name)?.GetRule(RuleType.Port) == null)
            {
                shell.SetState(new SetPortState(this));
            }
            else if (currentJail.GetRule(currentJail.Name, RuleType.Logpath) == null)
            {
                shell.SetState(new SetLogPathState(this));
            }
            else if (currentJail.GetRule(currentJail.Name, RuleType.Filter) == null)
            {
                shell.SetState(new SetFilterState(this));
            }
            else
            {
                shell.SetState(new SetRuleState(this));
            }

            return $"Создание сервиса";
        }
    }
}