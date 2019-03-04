using System;
using System.Data;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public class SetRuleState : ShellState
    {
        public SetRuleState(ShellState state) : base(state)
        {
            Message = Constants.ShellTexts[Constants.ShellSteps.SetRule];
        }

        public override string Handle(string[] values)
        {
            if (values[0] == "save")
            {
                if (TestSettings(out var result))
                {
                    serviceContainer.SetJail(currentJail);
                    if (currentFilter != null)
                    {
                        serviceContainer.SetFilter(currentFilter);
                    }

                    jailSaver.Save(currentJail);
                    filterSaver.Save(currentFilter);
                    "fail2ban-client restart".Bash();

                    return "Jail успешно сохранён";
                }
                else
                {
                    shell.SetState(new SetPortState(this));
                    return "Jail не прошёл тестирование и не был сохранён\n" +
                           $"Исправьте следующие ошибки: {result}";
                }
            }
            else if (RuleTypeExtension.TryParse(values[0], out var rule))
            {
                currentJail.SetRule(currentJail.Name, rule, values[1]);

                return "Значение правила установлено";
            }
            else
            {
                return "Неверное название правила";
            }
        }

        private bool TestSettings(out string result)
        {
            result = "";
            if (currentJail == null)
            {
                return false;
            }

            try
            {
                currentJail.SetRule(currentJail.Name, RuleType.Enabled, "true");
                jailSaver.Save(currentJail);

                if (currentFilter != null)
                {
                    filterSaver.Save(currentFilter);
                }

                result = "fail2ban-client -t".Bash();

                return Regex.IsMatch(result, @"OK: ");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Got exception while testing jail");
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
                return false;
            }
            finally
            {
                currentJail.SetRule(currentJail.Name, RuleType.Enabled, "false");
                jailSaver.Delete(currentJail);

                if (currentFilter != null)
                {
                    filterSaver.Delete(currentFilter);
                }
            }
        }
    }
}