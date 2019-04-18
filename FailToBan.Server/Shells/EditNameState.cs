using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class EditNameState : ShellState
    {
        public EditNameState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.EditRuleName];
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
            currentFilter = serviceContainer.GetFilter(serviceName);

            if (currentJail == null)
            {
                return "Сервиса с таким именем не существует\n" +
                       "Для создания сервиса используйте команду create";
            }

            if (currentFilter == null)
            {
                return $"Фильтра для сервиса {serviceName} не существует";
            }

            if (currentJail.GetRule(currentJail.Name, RuleType.Port) == null)
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

            return "Сервис найден";

        }

        
    }
}