using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server
{
    public class CreateShell : Shell, IDisposable
    {
        private Constants.CreateSteps step = Constants.CreateSteps.Prepare;

        public CreateShell(int clientId, Logger logger)
        {
            this.clientId = clientId;
            this.logger = logger;

            LogData($"Создана интерактивная оболочка для клиента с id = {clientId}", Logger.LogType.Debug);
        }

        public override string Get(string[] values)
        {
            LogData($"Оболочка получила команду \"{String.Join(" ", values)}\"", Logger.LogType.Debug);
            var message = "";
            switch (step)
            {
                case Constants.CreateSteps.Prepare:
                    step++;
                    message = "Оболочка создана";
                    break;

                case Constants.CreateSteps.RuleName:
                    LogData("Установка имени сервиса", Logger.LogType.Debug);
                    if (values.Length == 0)
                    {
                        LogData("Error", Logger.LogType.Debug);
                        message = "Неверное количество аргументов";
                        break;
                    }

                    if (values[0] == "--List")
                    {
                        var confRules = ServiceContainer
                            .GetDefault()
                            .ConfSetting
                            .GetSection("DEFAULT")
                            .Rules;
                        var localRules = ServiceContainer
                            .GetDefault()
                            .LocalSetting
                            .GetSection("DEFAULT")
                            .Rules;
                        foreach (var (type, value) in confRules)
                        {
                            if (!localRules.ContainsKey(type))
                            {
                                localRules.Add(type, value);
                            }
                        }

                        var builder = new StringBuilder();
                        builder.AppendLine("Список правил, установленных по умолчанию:");
                        foreach (var (key, value) in localRules)
                        {
                            builder.AppendLine($"{key} : {value}");
                        }
                        LogData(builder.ToString(), Logger.LogType.Debug);
                        message = builder.ToString();
                        break;
                    }

                    currentName = values[0];
                    currentFullName = $"{values[0]}.local";

                    if (settingContainer.HasSetting(values[0], Setting.SettingType.jail))
                    {
                        LogData("Сервис с таким именем уже существует \nДля изменения существующего сервиса используйте команду edit",
                            Logger.LogType.Message);
                        message = "Сервис с таким именем уже существует \nДля изменения существующего сервиса используйте команду edit";
                        break;
                    }

                    currentSetting = new Setting(Path.Combine(settingContainer.JailsPath, currentFullName), Setting.SettingType.jail, false);
                    step++;
                    LogData(ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.OK), Logger.LogType.Debug);
                    message = $"Сервис не найден";
                    break;

                case Constants.CreateSteps.Ports:
                    LogData("Установка порта", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        var port = new StringBuilder("");
                        foreach (var t in values)
                            port.Append(t);

                        if (Constants.PortRegex.IsMatch(port.ToString()))
                        {
                            currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.port, port.ToString());
                            step++;
                            message = $"Порт установлен";
                            break;
                        }
                        else
                        {
                            message = "Ошибка\n" + "Порты не соответствуют требованиям";
                            break;
                        }
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.CreateSteps.Path:
                    LogData("Установка пути к файлу", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        string Path = values[0];
                        if ((Path[0] == '/') || (Path[0] == '\\'))
                            Path = Path.Substring(1);
                        if (File.Exists("/_Data/Logs/" + Path))
                        {
                            currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.logpath, "/_Data/Logs/" + Path);
                            logPath = "/_Data/Logs/" + Path;
                            step++;
                            LogData(ConstantsExtention.StringConnectionCommands(Constants.ConnectionCommands.OK), Logger.LogType.Debug);
                            message = "Путь установлен";
                            break;
                        }
                        else
                        {
                            message = "Файл с логами не существует";
                            break;
                        }
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.CreateSteps.Filter:
                    LogData("Установка фильтра", Logger.LogType.Debug);
                    if (values.Length != 0)
                    {
                        var filterBuilder = new StringBuilder("");
                        foreach (var t in values)
                        {
                            filterBuilder.Append(t);
                        }

                        if (Constants.FilterRegex.IsMatch(filterBuilder.ToString()))
                        {
                            regex = filterBuilder.ToString();
                            currentFilter = new Setting(Path.Combine(settingContainer.FiltersPath, currentFullName), Setting.SettingType.filter, false);
                            currentFilter.GetOrCreateSection("INCLUDES").SetOrCreateRule(Rule.RuleType.before, "common.conf");
                            currentFilter.GetOrCreateSection("Definition").SetOrCreateRule(Rule.RuleType.failregex, regex);
                            currentSetting.GetOrCreateSection(currentName).SetOrCreateRule(Rule.RuleType.filter, currentFilter.Name);
                            step++;
                            message = "Фильтр установлен";
                            break;
                        }

                        message = "Фильтр не соответствует требованиям";
                        break;
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.CreateSteps.TestFilter:
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
                            message = "Продолжение без тестирования фильтра";
                            break;
                        }

                        message = "Введите y или n";
                        break;
                    }

                    message = "Неверное количество аргументов";
                    break;

                case Constants.CreateSteps.Other:
                    LogData("Настройка дополнительных параметров", Logger.LogType.Debug);
                    LogData($"values = {string.Join(" ", values)}", Logger.LogType.Debug);
                    if (values.Length == 0)
                    {
                        message = "Неверное количество аргументов";
                        break;
                    }

                    if (values[0] == "save")
                    {
                        Console.WriteLine($"ln -s {currentFilter.Path} /etc/fail2ban/filter.d/{currentFilter.FullName}".Bash());

                        if (TestSettings(out var result))
                        {
                            File.AppendAllText(Constants.FiltersListPath, currentFilter.FullName);
                            settingContainer.AddSetting(currentSetting);
                            settingContainer.AddSetting(currentFilter);
                            settingContainer.Save();
                            "fail2ban-client restart".Bash();

                            step++;
                            message = "Jail успешно сохранён";
                            break;
                        }

                        message = $"Jail не прошёл тестирование и не был сохранён\n" +
                                  $"Исправьте следующие ошибки: {result}";

                    }
                    else if (Regex.IsMatch(string.Join(" ", values), @"(\w+) \?"))
                    {
                        var rule = Regex.Match(string.Join(" ", values), @"(\w+) \?").Groups[1].Value;
                        var value = settingContainer.JailConf.GetRuleValue(rule) ?? settingContainer.JailLocal.GetRuleValue(rule);
                        LogData($"Запрос значения правила {rule} по умолчанию", Logger.LogType.Debug);
                        if (value != null)
                        {
                            message = $"Значение правила по умолчанию = {value}";
                            break;
                        }

                        message = "Значение неизвестно";
                    }
                    else
                    {
                        if (values.Length != 2)
                        {
                            message = "Неверное количество аргументов";
                            break;
                        }

                        var rule = values[0];
                        var value = values[1];
                        Rule.RuleType ruleType = Rule.GetRuleType(rule);
                        if (ruleType != Rule.RuleType.undefined)
                        {
                            message = "Значение правила выставлено";
                            break;
                        }

                        message = "Неизвестное правило";
                    }

                    break;

                case Constants.CreateSteps.Exit:
                    message = "";
                    break;
            }

            message += $"\n" + Constants.CreateTexts[step];
            return message;
        }
    }
}