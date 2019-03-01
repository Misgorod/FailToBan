using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace FailToBan.Server
{
    public class Constants
    {
        //#if DEBUG
        //public const string JailConfPath = "D:/docker_test/fail2ban";
        //public const string JailsPath = "/docker_test/fail2ban";
        //#else
        public const string JailConfPath = "/etc/fail2ban/";
        public const string JailsPath = "/_Data/Confs/Jails";
        public const string ActionsPath = "/_Data/Confs/Actions";
        public const string FiltersPath = "/_Data/Confs/Filters";
        //#endif
        public const string WhiteListPath = "/_Data/Util/WhiteList";
        public const string FiltersListPath = "/_Data/Util/FilterList";
        public const string StatusLogPath = "/_Data/Util/StatusLog";
        public const string MailLogPath = "/_Data/Util/MailLog";
        public const string ResultPath = "/_Data/Util/Result";
        public const string BlackListPath = "/_Data/Util/BlackList";
        public const string TimeFormat = "dd'/'MM'/'yyyy' 'HH':'mm':'ss";
        //public readonly static string WhiteListPath = Path.Combine(JailConfPath, "WhiteList");
        //public readonly static string JailDPath = Path.Combine(JailConfPath, "jail.d");
        //public readonly static string ActionDPath = Path.Combine(JailConfPath, "action.d");

        public readonly static Regex SectionRegex = new Regex(@"^\[(.+)\]\s*$", RegexOptions.Singleline);
        public readonly static Regex KeyValueRegex = new Regex(@"^([^#\s]\S*)\s*=\s*(.+)", RegexOptions.Singleline);
        public readonly static Regex ValueOnlyRegex = new Regex(@"^[^#]\S*\s*=\s*(.+)", RegexOptions.Singleline);
        // Строка, на которой находится продолжение значения для правила
        public readonly static Regex ContinueRegex = new Regex(@"^\s+(.+)\s*", RegexOptions.Singleline);
        public readonly static Regex CommandRegex = new Regex(@"^([^-]\w+)$", RegexOptions.Singleline);
        //public readonly static Regex ValueRegex = new Regex(@"^([\w\-_.\d:]+)$", RegexOptions.Singleline);
        public readonly static Regex ValueRegex = new Regex(@"^(.+)$", RegexOptions.Singleline);
        public readonly static Regex ConfRegex = new Regex(@".+[\\/]([\w\-_]+).(conf)$", RegexOptions.Singleline);
        public readonly static Regex LocalRegex = new Regex(@".+[\\/]([\w\-_]+).(local)$", RegexOptions.Singleline);
        public readonly static Regex BaseRegex = new Regex(@".+[\\/]([\w\-_]+).(conf).base$", RegexOptions.Singleline);
        #region
        public readonly static Regex PortRegex = new Regex(@"^(\d{1,5})([\s,]\d{1,5})*$", RegexOptions.Singleline);
        public readonly static Regex FilterRegex = new Regex(@"<HOST>", RegexOptions.Singleline);
        #endregion

        public enum RuleCategory
        {
            Required, Recommended, Additional, Specific
        }
        public struct RuleInfo
        {
            public Rule.RuleType Type;
            public RuleCategory Category;
            public string Description;
        }

        public static IReadOnlyDictionary<string, RuleInfo> Rules = new Dictionary<string, RuleInfo>
        {
            { "enabled", new RuleInfo() { Type = Rule.RuleType.enabled, Category = RuleCategory.Specific, Description = "Определяет, включён ли сервис" } },
            { "action", new RuleInfo() { Type = Rule.RuleType.action, Category = RuleCategory.Specific, Description = "Команда, запускаемая при бане ip адремас" } },
            { "action_", new RuleInfo() { Type = Rule.RuleType.action_, Category = RuleCategory.Specific } },
            { "action_abuseipdb", new RuleInfo() { Type = Rule.RuleType.action_abuseipdb, Category = RuleCategory.Specific } },
            { "action_badips", new RuleInfo() { Type = Rule.RuleType.action_badips, Category = RuleCategory.Specific } },
            { "action_badips_report", new RuleInfo() { Type = Rule.RuleType.action_badips_report, Category = RuleCategory.Specific } },
            { "action_blocklist_de", new RuleInfo() { Type = Rule.RuleType.action_blocklist_de, Category = RuleCategory.Specific } },
            { "action_cf_mwl", new RuleInfo() { Type = Rule.RuleType.action_cf_mwl, Category = RuleCategory.Specific } },
            { "action_mw", new RuleInfo() { Type = Rule.RuleType.action_mw, Category = RuleCategory.Specific } },
            { "action_mwl", new RuleInfo() { Type = Rule.RuleType.action_mwl, Category = RuleCategory.Specific } },
            { "action_xarf", new RuleInfo() { Type = Rule.RuleType.action_xarf, Category = RuleCategory.Specific } },
            { "actionstart", new RuleInfo() { Type = Rule.RuleType.actionstart, Category = RuleCategory.Specific } },
            { "actionstop", new RuleInfo() { Type = Rule.RuleType.actionstop, Category = RuleCategory.Specific } },
            { "actioncheck", new RuleInfo() { Type = Rule.RuleType.actioncheck, Category = RuleCategory.Specific } },
            { "actionban", new RuleInfo() { Type = Rule.RuleType.actionban, Category = RuleCategory.Specific } },
            { "actionunban", new RuleInfo() { Type = Rule.RuleType.actionunban, Category = RuleCategory.Specific } },
            { "after", new RuleInfo() { Type = Rule.RuleType.after, Category = RuleCategory.Specific } },
            { "backend", new RuleInfo() { Type = Rule.RuleType.backend, Category = RuleCategory.Specific, Description = "Определяет способ оповещения об изменениии логов" } },
            { "banaction", new RuleInfo() { Type = Rule.RuleType.banaction, Category = RuleCategory.Specific } },
            { "banaction_allports", new RuleInfo() { Type = Rule.RuleType.banaction_allports, Category = RuleCategory.Specific } },
            { "bantime", new RuleInfo() { Type = Rule.RuleType.bantime, Category = RuleCategory.Recommended,
                Description = "Время в секундах, на которое банится хост \n\tнапример: 10m"} },
            { "bantime.factor", new RuleInfo() { Type = Rule.RuleType.bantimeFactor, Category = RuleCategory.Additional,
                Description = "Коэффициент для расчёта экспоненциального роста в формуле или обычном множителе\n\tСтандартное значение - это 1 и для него время бана растёт на 1, 2, 4, 8, 16, 32...\n\tПример: 1"} },
            { "bantime.formula", new RuleInfo() { Type = Rule.RuleType.bantimeFormula, Category = RuleCategory.Additional,
                Description = "Используется по умолчанию для рассчёта следующего значения для времени бана\n\tФормула по умолчанию: ban.Time * (1 << (ban.Count if ban.Count < 20 else 20)) * banFactor\n\tНапример: ban.Time * math.exp(float(ban.Count+1)*banFactor)/math.exp(1*banFactor)"} },
            { "bantime.increment", new RuleInfo() { Type = Rule.RuleType.bantimeIncrement, Category = RuleCategory.Additional,
                Description = "Позволяет использовать базы данных для поиска уже забаненных ip-адресов для увеличения стандартного времени бана, используя специальную формулу\n\tПример: true" } },
            { "bantime.maxtime", new RuleInfo() { Type = Rule.RuleType.bantimeMaxtime, Category = RuleCategory.Additional,
                Description = "Максимальное количество секунд, на которое банится IP\n\tПример: 500"} },
            { "bantime.multipliers", new RuleInfo() { Type = Rule.RuleType.bantimeMultipliers, Category = RuleCategory.Additional,
                Description = "Используется для рассчёта следущего значения времени бана вместо формулы, соответствуя предыдущему времени бана и заданному \"bantime.factor\"\n\t Пример: bantime.multipliers = 1 5 30 60 300 720 1440 2880" } },
            { "bantime.overalljails", new RuleInfo() { Type = Rule.RuleType.bantimeOveralljails, Category = RuleCategory.Additional,
                Description = "Если true, выполняет поиск в базе Ip-адресов по всем сервисам\n\tЕсли false, выполняет поиск в базе Ip-адресов по текущему сервису \n\tПример: bantime.overalljails = false" } },
            { "bantime.rndtime", new RuleInfo() { Type = Rule.RuleType.bantimeRndtime, Category = RuleCategory.Additional,
                Description = "Максимальное количество секунд, которое используется для использования со случайным временем, чтобы обойти \"умных\" ботнетов, высчитавающих точное время, на которое IP будет снова разбанено\n\tПример: 10" } },
            { "before", new RuleInfo() { Type = Rule.RuleType.before, Category = RuleCategory.Specific } },
            { "blocktype", new RuleInfo() { Type = Rule.RuleType.blocktype, Category = RuleCategory.Specific } },
            { "chain", new RuleInfo() { Type = Rule.RuleType.chain, Category = RuleCategory.Specific } },
            { "destemail", new RuleInfo() { Type = Rule.RuleType.destemail, Category = RuleCategory.Specific } },
            { "fail2ban_agent", new RuleInfo() { Type = Rule.RuleType.fail2ban_agent, Category = RuleCategory.Specific } },
            { "filter", new RuleInfo() { Type = Rule.RuleType.filter, Category = RuleCategory.Required } },
            { "findtime", new RuleInfo() { Type = Rule.RuleType.findtime, Category = RuleCategory.Additional,
                Description = "Хост банится если неудачные попытки повторяются в течение последних \"findtime\" секунд\n\tПример: 10m" } },
            { "ignorecommand", new RuleInfo() { Type = Rule.RuleType.ignorecommand, Category = RuleCategory.Specific } },
            { "ignoreip", new RuleInfo() { Type = Rule.RuleType.ignoreip, Category = RuleCategory.Specific } },
            { "igonoreself", new RuleInfo() { Type = Rule.RuleType.ignoreself, Category = RuleCategory.Specific} },
            { "knocking_url", new RuleInfo() { Type = Rule.RuleType.knocking_url, Category = RuleCategory.Specific } },
            { "logencoding", new RuleInfo() { Type = Rule.RuleType.logencoding, Category = RuleCategory.Additional,
                Description = "Определяет кодировку логов\n\tНапример: \"ascii\", \"utf-8\"\n\tПо умолчанию: auto" } },
            { "logpath", new RuleInfo() { Type = Rule.RuleType.logpath, Category = RuleCategory.Required } },
            { "maxretry", new RuleInfo() { Type = Rule.RuleType.maxretry, Category = RuleCategory.Recommended,
                Description = "Количество неудачных попыток перед баном\n\tПример: 5" } },
            { "mode", new RuleInfo() { Type = Rule.RuleType.mode, Category = RuleCategory.Specific } },
            { "mta", new RuleInfo() { Type = Rule.RuleType.mta, Category = RuleCategory.Specific } },
            { "port", new RuleInfo() { Type = Rule.RuleType.port, Category = RuleCategory.Required } },
            { "protocol", new RuleInfo() { Type = Rule.RuleType.protocol, Category = RuleCategory.Recommended,
                Description = "Стандартный протокол\n\tПример: tcp" } },
            { "returntype", new RuleInfo() { Type = Rule.RuleType.returntype, Category = RuleCategory.Specific } },
            { "sender", new RuleInfo() { Type = Rule.RuleType.sender, Category = RuleCategory.Specific } },
            { "tcpport", new RuleInfo() { Type = Rule.RuleType.tcpport, Category = RuleCategory.Specific } },
            { "udpport", new RuleInfo() { Type = Rule.RuleType.udpport, Category = RuleCategory.Specific } },
            { "usedns", new RuleInfo() { Type = Rule.RuleType.usedns, Category = RuleCategory.Additional,
                Description = "Определяет, используется ли в блокировке обратный DNS\n\tДоступные значения: \"yes\", \"no\", \"warn\", \"raw\"\n\tno - fail2ban будет блокировать IP-адреса вместо имен хостов\n\twarn - попытается использовать обратный DNS для поиска имени хоста и его блокировки, но будет регистрировать активность в логе.\n\tПример: warn" } },
            { "norestored", new RuleInfo() { Type = Rule.RuleType.norestored, Category = RuleCategory.Specific } },
            { "name", new RuleInfo() { Type = Rule.RuleType.name, Category = RuleCategory.Specific } },

            { "action_vic", new RuleInfo() { Type = Rule.RuleType.action_vic, Category = RuleCategory.Specific } },
            { "undefined", new RuleInfo() { Type = Rule.RuleType.undefined, Category = RuleCategory.Specific } },
            { "failregex", new RuleInfo() { Type = Rule.RuleType.failregex, Category = RuleCategory.Specific, Description = "Регулярное выражение, по которому выбираются неудачные попытки"} }
        };

        public enum ConnectionCommands { OK, Part, Error, End, Back, Forward }
        public static IReadOnlyDictionary<ConnectionCommands, string> ConnectionCommand = new Dictionary<ConnectionCommands, string>
        {
            { ConnectionCommands.OK, "OK" },
            { ConnectionCommands.Part, "Part" },
            { ConnectionCommands.Error, "Error" },
            { ConnectionCommands.End, "End" },
            { ConnectionCommands.Back, "Back" },
            { ConnectionCommands.Forward, "Forward" },
        };

        public enum ClientMode { Normal, Create, Edit }

        public enum CreateSteps { Prepare, RuleName, Ports, Path, Filter, TestFilter, Other, Exit }
        public static IReadOnlyDictionary<CreateSteps, string> CreateTexts = new Dictionary<CreateSteps, string>
        {
            { CreateSteps.Prepare, "Запуск CLI для создания нового сервиса" },
            { CreateSteps.RuleName, "Введите имя сервиса для которого вы хотите создать правило (для просмотра преднастроенных правил введите --List)" },
            { CreateSteps.Ports, "Введите список портов в формате \"xxxx, xxxx, xxxx\"" },
            { CreateSteps.Path, "Введите путь до каталога с файлом лога авторизации" },
            { CreateSteps.Filter, "Введите регулярное выражение для поиска строки с сообщением о неуспешной авторизации\nПримечание: формат обычный regexp, для указания хоста на его месте введите <HOST>" },
            { CreateSteps.TestFilter, "Хотите ли вы протестировать работу регулярного выражения(y/n)? Примечание: для тестирование в файле лога должна быть запись о неудачной авторизации" },
            { CreateSteps.Other, "Введите save для сохранения\n" +
                                 "Введите дополнительные значения для правила в формате 'rule value'\n" +
                                 "Введите название правила и ? в формате 'rule ?' для получения значения по умолчанию для этого правила" },
            { CreateSteps.Exit, "Выход из интерактивного режима" }
        };

        public enum EditSteps { Prepare, RuleName, Ports, Path, Filter, TestFilter, Set, Exit }
        public static IReadOnlyDictionary<EditSteps, string> EditTexts = new Dictionary<EditSteps, string>
        {
            { EditSteps.Prepare, "Запуск CLI для изменения существующего сервиса" },
            { EditSteps.RuleName, "Введите имя сервиса, значения правил которого вы хотите изменить (для просмотра настроенных правил введите --List)" },
            { EditSteps.Ports, "Введите список портов в формате \"xxxx, xxxx, xxxx\"" },
            { EditSteps.Path, "Введите путь до каталога с файлом лога авторизации" },
            { EditSteps.Filter, "Введите регулярное выражение для поиска строки с сообщением о неуспешной авторизации\nПримечание: формат обычный regexp, для указания хоста на его месте введите <HOST>" },
            { EditSteps.Set, "Введите правило и значение которые хотите изменить в формате \"rule value\"\nДля сохранения введите save" },
            { EditSteps.Exit, "Выход из интерактивного режима" }
        };
    }
}