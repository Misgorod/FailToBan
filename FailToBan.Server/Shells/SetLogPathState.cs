using System.Collections.Generic;
using System.Data;
using System.IO;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class SetLogPathState : ShellState
    {
        public SetLogPathState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.LogPath];
        }

        public override string Handle(string[] values)
        {
            if (values.Length == 0)
            {
                return "Неверное количество аргументов";
            }

            var path = values[0];
            if ((path[0] == '/') || (path[0] == '\\'))
            {
                path = path.Substring(1);
            }

            var logPath = "/_Data/Logs/" + path;
            if (File.Exists(logPath))
            {
                currentJail.SetRule(currentJail.Name, RuleType.Logpath, logPath);

                if (currentJail.GetRule(currentJail.Name, RuleType.Filter) == null)
                {
                    shell.SetState(new SetFilterState(this));
                }
                else
                {
                    shell.SetState(new SetRuleState(this));
                }

                return "Путь установлен";
            }
            else
            {
                return "Файла с таким названием не существует";
            }

        }
    }
}