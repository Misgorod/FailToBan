using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server
{
    public class Constants
    {
        public const string JailConfPath = "/etc/fail2ban/";
        public const string JailsPath = "/_Data/Confs/Jails";
        public const string ActionsPath = "/_Data/Confs/Actions";
        public const string FiltersPath = "/_Data/Confs/Filters";
        public const string WhiteListPath = "/_Data/Util/WhiteList";
        public const string FiltersListPath = "/_Data/Util/FilterList";
        public const string StatusLogPath = "/_Data/Util/StatusLog";
        public const string MailLogPath = "/_Data/Util/MailLog";
        public const string ResultPath = "/_Data/Util/Result";
        public const string BlackListPath = "/_Data/Util/BlackList";
        public const string TimeFormat = "dd'/'MM'/'yyyy' 'HH':'mm':'ss";

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
            public RuleType Type;
            public RuleCategory Category;
            public string Description;
        }

        public static IReadOnlyDictionary<string, RuleInfo> Rules = new Dictionary<string, RuleInfo>
        {
            { "enabled", new RuleInfo() { Type = RuleType.Enabled, Category = RuleCategory.Specific, Description = "Определяет, включён ли сервис" } },
            { "action", new RuleInfo() { Type = RuleType.Action, Category = RuleCategory.Specific, Description = "Команда, запускаемая при бане ip адремас" } },
            { "action_", new RuleInfo() { Type = RuleType.Action_, Category = RuleCategory.Specific } },
            { "action_abuseipdb", new RuleInfo() { Type = RuleType.ActionAbuseipdb, Category = RuleCategory.Specific } },
            { "action_badips", new RuleInfo() { Type = RuleType.ActionBadips, Category = RuleCategory.Specific } },
            { "action_badips_report", new RuleInfo() { Type = RuleType.ActionBadipsReport, Category = RuleCategory.Specific } },
            { "action_blocklist_de", new RuleInfo() { Type = RuleType.ActionBlocklistDe, Category = RuleCategory.Specific } },
            { "action_cf_mwl", new RuleInfo() { Type = RuleType.ActionCfMwl, Category = RuleCategory.Specific } },
            { "action_mw", new RuleInfo() { Type = RuleType.ActionMw, Category = RuleCategory.Specific } },
            { "action_mwl", new RuleInfo() { Type = RuleType.ActionMwl, Category = RuleCategory.Specific } },
            { "action_xarf", new RuleInfo() { Type = RuleType.ActionXarf, Category = RuleCategory.Specific } },
            { "actionstart", new RuleInfo() { Type = RuleType.Actionstart, Category = RuleCategory.Specific } },
            { "actionstop", new RuleInfo() { Type = RuleType.Actionstop, Category = RuleCategory.Specific } },
            { "actioncheck", new RuleInfo() { Type = RuleType.Actioncheck, Category = RuleCategory.Specific } },
            { "actionban", new RuleInfo() { Type = RuleType.Actionban, Category = RuleCategory.Specific } },
            { "actionunban", new RuleInfo() { Type = RuleType.Actionunban, Category = RuleCategory.Specific } },
            { "after", new RuleInfo() { Type = RuleType.After, Category = RuleCategory.Specific } },
            { "backend", new RuleInfo() { Type = RuleType.Backend, Category = RuleCategory.Specific, Description = "Определяет способ оповещения об изменениии логов" } },
            { "banaction", new RuleInfo() { Type = RuleType.Banaction, Category = RuleCategory.Specific } },
            { "banaction_allports", new RuleInfo() { Type = RuleType.BanactionAllports, Category = RuleCategory.Specific } },
            { "bantime", new RuleInfo() { Type = RuleType.Bantime, Category = RuleCategory.Recommended,
                Description = "Время в секундах, на которое банится хост \n\tнапример: 10m"} },
            { "bantime.factor", new RuleInfo() { Type = RuleType.BantimeFactor, Category = RuleCategory.Additional,
                Description = "Коэффициент для расчёта экспоненциального роста в формуле или обычном множителе\n\tСтандартное значение - это 1 и для него время бана растёт на 1, 2, 4, 8, 16, 32...\n\tПример: 1"} },
            { "bantime.formula", new RuleInfo() { Type = RuleType.BantimeFormula, Category = RuleCategory.Additional,
                Description = "Используется по умолчанию для рассчёта следующего значения для времени бана\n\tФормула по умолчанию: ban.Time * (1 << (ban.Count if ban.Count < 20 else 20)) * banFactor\n\tНапример: ban.Time * math.exp(float(ban.Count+1)*banFactor)/math.exp(1*banFactor)"} },
            { "bantime.increment", new RuleInfo() { Type = RuleType.BantimeIncrement, Category = RuleCategory.Additional,
                Description = "Позволяет использовать базы данных для поиска уже забаненных ip-адресов для увеличения стандартного времени бана, используя специальную формулу\n\tПример: true" } },
            { "bantime.maxtime", new RuleInfo() { Type = RuleType.BantimeMaxtime, Category = RuleCategory.Additional,
                Description = "Максимальное количество секунд, на которое банится IP\n\tПример: 500"} },
            { "bantime.multipliers", new RuleInfo() { Type = RuleType.BantimeMultipliers, Category = RuleCategory.Additional,
                Description = "Используется для рассчёта следущего значения времени бана вместо формулы, соответствуя предыдущему времени бана и заданному \"bantime.factor\"\n\t Пример: bantime.multipliers = 1 5 30 60 300 720 1440 2880" } },
            { "bantime.overalljails", new RuleInfo() { Type = RuleType.BantimeOveralljails, Category = RuleCategory.Additional,
                Description = "Если true, выполняет поиск в базе Ip-адресов по всем сервисам\n\tЕсли false, выполняет поиск в базе Ip-адресов по текущему сервису \n\tПример: bantime.overalljails = false" } },
            { "bantime.rndtime", new RuleInfo() { Type = RuleType.BantimeRndtime, Category = RuleCategory.Additional,
                Description = "Максимальное количество секунд, которое используется для использования со случайным временем, чтобы обойти \"умных\" ботнетов, высчитавающих точное время, на которое IP будет снова разбанено\n\tПример: 10" } },
            { "before", new RuleInfo() { Type = RuleType.Before, Category = RuleCategory.Specific } },
            { "blocktype", new RuleInfo() { Type = RuleType.Blocktype, Category = RuleCategory.Specific } },
            { "chain", new RuleInfo() { Type = RuleType.Chain, Category = RuleCategory.Specific } },
            { "destemail", new RuleInfo() { Type = RuleType.Destemail, Category = RuleCategory.Specific } },
            { "fail2ban_agent", new RuleInfo() { Type = RuleType.Fail2BanAgent, Category = RuleCategory.Specific } },
            { "filter", new RuleInfo() { Type = RuleType.Filter, Category = RuleCategory.Required } },
            { "findtime", new RuleInfo() { Type = RuleType.Findtime, Category = RuleCategory.Additional,
                Description = "Хост банится если неудачные попытки повторяются в течение последних \"findtime\" секунд\n\tПример: 10m" } },
            { "ignorecommand", new RuleInfo() { Type = RuleType.Ignorecommand, Category = RuleCategory.Specific } },
            { "ignoreip", new RuleInfo() { Type = RuleType.Ignoreip, Category = RuleCategory.Specific } },
            { "igonoreself", new RuleInfo() { Type = RuleType.Ignoreself, Category = RuleCategory.Specific} },
            { "knocking_url", new RuleInfo() { Type = RuleType.KnockingUrl, Category = RuleCategory.Specific } },
            { "logencoding", new RuleInfo() { Type = RuleType.Logencoding, Category = RuleCategory.Additional,
                Description = "Определяет кодировку логов\n\tНапример: \"ascii\", \"utf-8\"\n\tПо умолчанию: auto" } },
            { "logpath", new RuleInfo() { Type = RuleType.Logpath, Category = RuleCategory.Required } },
            { "maxretry", new RuleInfo() { Type = RuleType.Maxretry, Category = RuleCategory.Recommended,
                Description = "Количество неудачных попыток перед баном\n\tПример: 5" } },
            { "mode", new RuleInfo() { Type = RuleType.Mode, Category = RuleCategory.Specific } },
            { "mta", new RuleInfo() { Type = RuleType.Mta, Category = RuleCategory.Specific } },
            { "port", new RuleInfo() { Type = RuleType.Port, Category = RuleCategory.Required } },
            { "protocol", new RuleInfo() { Type = RuleType.Protocol, Category = RuleCategory.Recommended,
                Description = "Стандартный протокол\n\tПример: tcp" } },
            { "returntype", new RuleInfo() { Type = RuleType.Returntype, Category = RuleCategory.Specific } },
            { "sender", new RuleInfo() { Type = RuleType.Sender, Category = RuleCategory.Specific } },
            { "tcpport", new RuleInfo() { Type = RuleType.Tcpport, Category = RuleCategory.Specific } },
            { "udpport", new RuleInfo() { Type = RuleType.Udpport, Category = RuleCategory.Specific } },
            { "usedns", new RuleInfo() { Type = RuleType.Usedns, Category = RuleCategory.Additional,
                Description = "Определяет, используется ли в блокировке обратный DNS\n\tДоступные значения: \"yes\", \"no\", \"warn\", \"raw\"\n\tno - fail2ban будет блокировать IP-адреса вместо имен хостов\n\twarn - попытается использовать обратный DNS для поиска имени хоста и его блокировки, но будет регистрировать активность в логе.\n\tПример: warn" } },
            { "norestored", new RuleInfo() { Type = RuleType.Norestored, Category = RuleCategory.Specific } },
            { "name", new RuleInfo() { Type = RuleType.Name, Category = RuleCategory.Specific } },

            { "action_vic", new RuleInfo() { Type = RuleType.ActionVic, Category = RuleCategory.Specific } },
            { "failregex", new RuleInfo() { Type = RuleType.Failregex, Category = RuleCategory.Specific, Description = "Регулярное выражение, по которому выбираются неудачные попытки"} }
        };

        public enum ShellSteps { Prepare, CreateRuleName, EditRuleName, Ports, LogPath, Filter, TestFilter, SetRule, Exit }
        public static IReadOnlyDictionary<ShellSteps, string> ShellTexts = new Dictionary<ShellSteps, string>()
        {
            { ShellSteps.Prepare, "Запуск CLI для создания нового сервиса" },
            { ShellSteps.CreateRuleName, "Введите имя сервиса для которого вы хотите создать правило (для просмотра преднастроенных правил введите --List)" },
            { ShellSteps.EditRuleName, "Введите имя сервиса, значения правил которого вы хотите изменить (для просмотра настроенных правил введите --List)" },
            { ShellSteps.Ports, "Введите список портов в формате \"xxxx, xxxx, xxxx\"" },
            { ShellSteps.LogPath, "Введите путь до каталога с файлом лога авторизации" },
            { ShellSteps.Filter, "Введите регулярное выражение для поиска строки с сообщением о неуспешной авторизации\nПримечание: формат обычный regexp, для указания хоста на его месте введите <HOST>" },
            { ShellSteps.TestFilter, "Хотите ли вы протестировать работу регулярного выражения(y/n)? Примечание: для тестирование в файле лога должна быть запись о неудачной авторизации" },
            { ShellSteps.SetRule, "Введите save для сохранения\n" +
                                 "Введите дополнительные значения для правила в формате 'rule value'\n" +
                                 "Введите название правила и ? в формате 'rule ?' для получения значения по умолчанию для этого правила" },
            { ShellSteps.Exit, "Выход из интерактивного режима" }
        };
    }
}