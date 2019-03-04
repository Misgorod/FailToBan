using System.Data;
using System.IO;
using System.Text;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class SetFilterState : ShellState
    {
        public SetFilterState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.Filter];
        }

        public override string Handle(string[] values)
        {
            if (values.Length == 0) return "Неверное количество аргументов";
            var builder = new StringBuilder("");
            foreach (var value in values)
                builder.Append(value);

            if (Constants.FilterRegex.IsMatch(builder.ToString()))
            {
                var regex = "";
                regex = builder.ToString();

                currentFilter = serviceFactory.BuildService(currentJail.Name);
                currentFilter.SetRule("INCLUDES", RuleType.Before, "common.conf");
                currentFilter.SetRule("Definition", RuleType.Failregex, regex);
                currentJail.SetRule(currentJail.Name, RuleType.Filter, currentFilter.Name);

                shell.SetState(new TestFilterState(this));
                return "Фильтр установлен";
            }
            else
            {
                return "Фильтр не соответствует требованиям";
            }

        }
    }
}