using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server
{
    public class EditShell : Shell, IDisposable
    {
        private Constants.EditSteps step = Constants.EditSteps.Prepare;

        public EditShell(int clientId, SettingContainer settingContainer, Logger logger)
        {
            this.clientId = clientId;
            this.settingContainer = settingContainer;
            this.logger = logger;

            LogData($"Создана интерактивная оболочка для клиента с id = {clientId}", Logger.LogType.Debug);
        }

        public override string Get(string[] values)
        {
            LogData($"Оболочка получила команду \"{string.Join(" ", values)}\"", Logger.LogType.Debug);
            var message = "";
            switch (step)
            {
                case Constants.EditSteps.Prepare:
                    step++;
                    message = "Оболочка создана";
                    Console.WriteLine($"Step now is {Constants.EditTexts[step]}");
                    break;

                case Constants.EditSteps.RuleName:
                    LogData("Установка сервиса", Logger.LogType.Debug);
                    if (values[0] == "--List")
                    {
                        message = PrintRules();
                        break;
                    }
                    if (values.Length != 0)
                    {
                        currentName = values[0];
                        if (settingContainer.HasSetting($"{values[0]}.local", Setting.SettingType.jail))
                        {
                            currentFullName = $"{values[0]}.local";
                        }
                        else if (settingContainer.HasSetting($"{values[0]}.conf", Setting.SettingType.jail))
                        {
                            currentFullName = $"{values[0]}.local";
                        }
                        else
                        {
                            message = "Сервиса с таким именем не существует\n" +
                                      "Для создания сервиса используйте команду create";
                            break;
                        }

                        currentSetting = settingContainer.GetOrCreateSetting(currentFullName, Setting.SettingType.jail);
                        var rules = Commands.ManageJail(settingContainer, currentName);
                        LogData($"Правила для сервиса {currentName}:\n {string.Join(" : ", rules.Keys.ToList())}", Logger.LogType.Debug);

                        message = "Сервис найден";
                        if (!rules.ContainsKey("port"))
                        {
                            step += 1;
                            break;
                        }
                        else if (!rules.ContainsKey("logpath"))
                        {
                            step += 2;
                            break;
                        }
                        else if (!rules.ContainsKey("filter"))
                        {
                            step += 3;
                            break;
                        }
                        else
                        {
                            step += 5;
                            break;
                        }
                    }

                    LogData(ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.Error), Logger.LogType.Debug);
                    message = "Неверное количество аргументов";
                    break;

                case Constants.EditSteps.Ports:
                    LogData("Установка порта", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        var port = new StringBuilder("");
                        foreach (var value in values)
                        {
                            port.Append(value);
                        }

                        if (Constants.PortRegex.IsMatch(port.ToString()))
                        {
                            currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.port, port.ToString());
                            var rules = Commands.ManageJail(settingContainer, currentName);

                            message = $"Порт установлен";
                            if (!rules.ContainsKey("logpath"))
                            {
                                step += 1;
                                break;
                            }
                            else if (!rules.ContainsKey("filter"))
                            {
                                step += 2;
                                break;
                            }
                            else
                            {
                                step += 4;
                                break;
                            }
                        }
                        else
                        {
                            message = "Ошибка\n" + "Порты не соответствуют требованиям";
                            break;
                        }
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.EditSteps.Path:
                    LogData("Установка пути к файлу", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        var path = values[0];
                        if ((path[0] == '/') || (path[0] == '\\'))
                        {
                            path = path.Substring(1);
                        }

                        if (File.Exists("/_Data/Logs/" + path))
                        {
                            currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.logpath, "/_Data/Logs/" + path);
                            Dictionary<string, string> rules = Commands.ManageJail(settingContainer, currentName);

                            message = "Путь установлен";
                            if (!rules.ContainsKey("filter"))
                            {
                                step += 1;
                                break;
                            }
                            else
                            {
                                step += 3;
                                break;
                            }
                        }
                        else
                        {
                            message = "Файла с таким названием не существует";
                            break;
                        }
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.EditSteps.Filter:
                    LogData("Установка фильтра", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        var filterBuilder = new StringBuilder("");
                        foreach (var value in values)
                            filterBuilder.Append(value);

                        if (Constants.FilterRegex.IsMatch(filterBuilder.ToString()))
                        {
                            var regex = "";
                            regex = filterBuilder.ToString();
                            currentFilter = new Setting(Path.Combine(settingContainer.FiltersPath, currentFullName), Setting.SettingType.filter, false);
                            currentFilter.GetOrCreateSection("INCLUDES").SetOrCreateRule(Rule.RuleType.before, "common.conf");
                            currentFilter.GetOrCreateSection("Definition").SetOrCreateRule(Rule.RuleType.failregex, regex);
                            currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.filter, currentFilter.FullName);

                            step++;
                            message = "Фильтр установлен";
                            break;
                        }
                        else
                        {
                            message = "Фильтр не соотвутствует требованиям";
                            break;
                        }
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.EditSteps.TestFilter:
                    LogData("Тестирование фильтра", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        if ((values[0] == "y") || (values[0] == "n"))
                        {
                            if (values[0] == "y")
                            {
                                if (TestFilter())
                                {
                                    step++;
                                    message = "Фильтр успешно протестирован";
                                    break;
                                }

                                step -= 2;
                                message = "Фильтр не прошёл тесты";
                                break;
                            }

                            step++;
                            message = "Продолжение без тестирования работы фильтра";
                            break;
                        }

                        LogData(ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.Error), Logger.LogType.Debug);
                        return ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.Error);
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.EditSteps.Set:
                    if (values[0] == "save")
                    {
                        if (TestSettings(out var result))
                        {
                            settingContainer.AddSetting(currentSetting);
                            if (currentFilter != null)
                            {
                                settingContainer.AddSetting(currentFilter);
                            }

                            settingContainer.Save();
                            "fail2ban-client restart".Bash();

                            message = "Jail успешно сохранён";
                            break;
                        }

                        step -= 3;
                        message = "Jail не прошёл тестирование и не был сохранён\n" +
                                  $"Исправьте следующие ошибки: {result}";
                        break;

                    }
                    else if (Rule.GetRuleType(values[0]) != Rule.RuleType.undefined)
                    {
                        LogData($"setting rule {values[0]} to value {values[1]}", Logger.LogType.Debug);
                        currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(values[0], values[1]);

                        message = "Значение правила установлено";
                        break;
                    }
                    else
                    {
                        message = "Неверное название правила";
                        break;
                    }

                case Constants.EditSteps.Exit:
                    message = "";
                    break;
            }

            message += "\n" + Constants.EditTexts[step];
            return message;
        }

        protected override bool TestSettings(out string result)
        {
            result = "";
            if (currentSetting == null)
            {
                return false;
            }

            try
            {
                currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.enabled, "true");
                currentSetting.Save();
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
                currentSetting.Delete();
            }
        }

        private string PrintRules()
        {
            var rules = Commands.ManageJail(settingContainer, currentName);
            var builder = new StringBuilder();
            builder.AppendLine(ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.Part) + " Текущие правила:\n");
            LogData(ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.Part) + " Текущие правила:\n" + builder, Logger.LogType.Message);
            foreach (var (rule, value) in rules)
            {
                LogData($"{rule} : {value}", Logger.LogType.Message);
                builder.AppendLine($"{rule} : {value}");
            }

            return builder.ToString();
        }
    }
}