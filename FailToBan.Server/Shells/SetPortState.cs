using System.Text;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class SetPortState : ShellState
    {
        public SetPortState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.Ports];
        }

        public override string Handle(string[] values)
        {
            if (values.Length == 0)
            {
                return "Неверное количество аргументов";
            }

            var port = new StringBuilder("");
            foreach (var value in values)
            {
                port.Append(value);
            }

            if (Constants.PortRegex.IsMatch(port.ToString()))
            {
                currentJail.SetRule(currentJail.Name, RuleType.Port, port.ToString());

                if (currentJail.GetRule(currentJail.Name, RuleType.Logpath) == null)
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

                return "Порт установлен";
            }
            else
            {
                return "Ошибка\n" + 
                       "Порты не соответствуют требованиям";
            }

        }
    }
}