using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class TestFilterState : ShellState
    {
        public TestFilterState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.TestFilter];
        }

        public override string Handle(string[] values)
        {
            if (values.Length == 0)
            {
                return "Неверное количество аргументов";
            }

            if ((values[0] != "y") && (values[0] != "n"))
            {
                return "Введите y или n";
            }

            if (values[0] == "y")
            {
                if (TestFilter(out var logResult))
                {
                    shell.SetState(new SetRuleState(this));
                    return "Фильтр успешно протестирован";
                }
                else
                {
                    shell.SetState(new SetLogPathState(this));
                    return logResult;
                }
            }

            shell.SetState(new SetRuleState(this));
            return "Продолжение без тестирования работы фильтра";

        }

        private bool TestFilter(out string logResult)
        {
            logResult = "";
            var regex = currentFilter.GetRule("Definition", RuleType.Failregex);
            var logPath = currentJail.GetRule(currentJail.Name, RuleType.Logpath);

            if (regex == null || logPath == null)
            {
                throw new VicException("Фильтр или путь до логов ещё не выставлен");
            }

            var pattern = Regex.Replace(regex, @"(<HOST>)", @"(?:::f{4,6}:)?(?<host>\S+)");
            using (var reader = new StreamReader(File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    var host = Regex.Match(line, pattern).Groups["host"].Value;
                    if (!IPAddress.TryParse(host, out IPAddress address))
                    {
                        logResult += $"Строка {line} не прошла регулярное выражением";
                        return false;
                    }
                }
            }

            return true;
        }
    }
}