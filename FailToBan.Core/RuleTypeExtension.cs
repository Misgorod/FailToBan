using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FailToBan.Core
{
    public static class RuleTypeExtension
    {
        public static bool TryParse(this RuleType type, string name, out RuleType result)
        {
            if (mapping.ContainsKey(name))
            {
                result = mapping[name];
                return true;
            }
            else
            {
                result = RuleType.Null;
                return false;
            }
        }

        public static string ToString(this RuleType type)
        {
            return mapping
                .First(x => x.Value == type)
                .Key;
        }

        private static readonly Dictionary<string, RuleType> mapping = new Dictionary<string, RuleType>()
        {
            { "enabled", RuleType.Enabled },
            { "action", RuleType.Action },
            { "action_", RuleType.Action_ },
            { "action_abuseipdb", RuleType.ActionAbuseipdb },
            { "action_badips", RuleType.ActionBadips },
            { "action_badips_report", RuleType.ActionBadipsReport },
            { "action_blocklist_de", RuleType.ActionBlocklistDe },
            { "action_cf_mwl", RuleType.ActionCfMwl },
            { "action_mw", RuleType.ActionMw },
            { "action_mwl", RuleType.ActionMwl },
            { "action_xarf", RuleType.ActionXarf },
            { "actionstart", RuleType.Actionstart },
            { "actionstop", RuleType.Actionstop },
            { "actioncheck", RuleType.Actioncheck },
            { "actionban", RuleType.Actionban },
            { "actionunban", RuleType.Actionunban },
            { "after", RuleType.After },
            { "backend", RuleType.Backend },
            { "banaction", RuleType.Banaction },
            { "banaction_allports", RuleType.BanactionAllports },
            { "bantime", RuleType.Bantime },
            { "bantime.factor", RuleType.BantimeFactor },
            { "bantime.formula", RuleType.BantimeFormula },
            { "bantime.increment", RuleType.BantimeIncrement },
            { "bantime.maxtime", RuleType.BantimeMaxtime },
            { "bantime.multipliers", RuleType.BantimeMultipliers },
            { "bantime.overalljails", RuleType.BantimeOveralljails },
            { "bantime.rndtime", RuleType.BantimeRndtime },
            { "before", RuleType.Before },
            { "blocktype", RuleType.Blocktype },
            { "chain", RuleType.Chain },
            { "destemail", RuleType.Destemail },
            { "fail2ban_agent", RuleType.Fail2BanAgent },
            { "filter", RuleType.Filter },
            { "findtime", RuleType.Findtime },
            { "ignorecommand", RuleType.Ignorecommand },
            { "ignoreip", RuleType.Ignoreip },
            { "igonoreself", RuleType.Ignoreself },
            { "knocking_url", RuleType.KnockingUrl },
            { "logencoding", RuleType.Logencoding },
            { "logpath", RuleType.Logpath },
            { "maxretry", RuleType.Maxretry },
            { "mode", RuleType.Mode },
            { "mta", RuleType.Mta },
            { "port", RuleType.Port },
            { "protocol", RuleType.Protocol },
            { "returntype", RuleType.Returntype },
            { "sender", RuleType.Sender },
            { "tcpport", RuleType.Tcpport },
            { "udpport", RuleType.Udpport },
            { "usedns", RuleType.Usedns },
            { "norestored", RuleType.Norestored },
            { "action_vic", RuleType.ActionVic },
            { "failregex", RuleType.Failregex },
        };

        //public static IReadOnlyDictionary<string, RuleInfo> Rules = new Dictionary<string, RuleInfo>
        //{
        //    { "enabled", new RuleInfo() { Type = RuleType.Enabled, Category = RuleCategory.Specific, Description = "Определяет, включён ли сервис" } },
        //    { "action", new RuleInfo() { Type = RuleType.Action, Category = RuleCategory.Specific, Description = "Команда, запускаемая при бане ip адремас" } },
        //    { "action_", new RuleInfo() { Type = RuleType.Action_, Category = RuleCategory.Specific } },
        //    { "action_abuseipdb", new RuleInfo() { Type = RuleType.ActionAbuseipdb, Category = RuleCategory.Specific } },
        //    { "action_badips", new RuleInfo() { Type = RuleType.ActionBadips, Category = RuleCategory.Specific } },
        //    { "action_badips_report", new RuleInfo() { Type = RuleType.ActionBadipsReport, Category = RuleCategory.Specific } },
        //    { "action_blocklist_de", new RuleInfo() { Type = RuleType.ActionBlocklistDe, Category = RuleCategory.Specific } },
        //    { "action_cf_mwl", new RuleInfo() { Type = RuleType.ActionCfMwl, Category = RuleCategory.Specific } },
        //    { "action_mw", new RuleInfo() { Type = RuleType.ActionMw, Category = RuleCategory.Specific } },
        //    { "action_mwl", new RuleInfo() { Type = RuleType.ActionMwl, Category = RuleCategory.Specific } },
        //    { "action_xarf", new RuleInfo() { Type = RuleType.ActionXarf, Category = RuleCategory.Specific } },
        //    { "actionstart", new RuleInfo() { Type = RuleType.Actionstart, Category = RuleCategory.Specific } },
        //    { "actionstop", new RuleInfo() { Type = RuleType.Actionstop, Category = RuleCategory.Specific } },
        //    { "actioncheck", new RuleInfo() { Type = RuleType.Actioncheck, Category = RuleCategory.Specific } },
        //    { "actionban", new RuleInfo() { Type = RuleType.Actionban, Category = RuleCategory.Specific } },
        //    { "actionunban", new RuleInfo() { Type = RuleType.Actionunban, Category = RuleCategory.Specific } },
        //    { "after", new RuleInfo() { Type = RuleType.After, Category = RuleCategory.Specific } },
        //    { "backend", new RuleInfo() { Type = RuleType.Backend, Category = RuleCategory.Specific, Description = "Определяет способ оповещения об изменениии логов" } },
        //    { "banaction", new RuleInfo() { Type = RuleType.Banaction, Category = RuleCategory.Specific } },
        //    { "banaction_allports", new RuleInfo() { Type = RuleType.BanactionAllports, Category = RuleCategory.Specific } },
        //    { "bantime", new RuleInfo() { Type = RuleType.Bantime, Category = RuleCategory.Recommended,
        //        Description = "Время в секундах, на которое банится хост \n\tнапример: 10m"} },
        //    { "bantime.factor", new RuleInfo() { Type = RuleType.BantimeFactor, Category = RuleCategory.Additional,
        //        Description = "Коэффициент для расчёта экспоненциального роста в формуле или обычном множителе\n\tСтандартное значение - это 1 и для него время бана растёт на 1, 2, 4, 8, 16, 32...\n\tПример: 1"} },
        //    { "bantime.formula", new RuleInfo() { Type = RuleType.BantimeFormula, Category = RuleCategory.Additional,
        //        Description = "Используется по умолчанию для рассчёта следующего значения для времени бана\n\tФормула по умолчанию: ban.Time * (1 << (ban.Count if ban.Count < 20 else 20)) * banFactor\n\tНапример: ban.Time * math.exp(float(ban.Count+1)*banFactor)/math.exp(1*banFactor)"} },
        //    { "bantime.increment", new RuleInfo() { Type = RuleType.BantimeIncrement, Category = RuleCategory.Additional,
        //        Description = "Позволяет использовать базы данных для поиска уже забаненных ip-адресов для увеличения стандартного времени бана, используя специальную формулу\n\tПример: true" } },
        //    { "bantime.maxtime", new RuleInfo() { Type = RuleType.BantimeMaxtime, Category = RuleCategory.Additional,
        //        Description = "Максимальное количество секунд, на которое банится IP\n\tПример: 500"} },
        //    { "bantime.multipliers", new RuleInfo() { Type = RuleType.BantimeMultipliers, Category = RuleCategory.Additional,
        //        Description = "Используется для рассчёта следущего значения времени бана вместо формулы, соответствуя предыдущему времени бана и заданному \"bantime.factor\"\n\t Пример: bantime.multipliers = 1 5 30 60 300 720 1440 2880" } },
        //    { "bantime.overalljails", new RuleInfo() { Type = RuleType.BantimeOveralljails, Category = RuleCategory.Additional,
        //        Description = "Если true, выполняет поиск в базе Ip-адресов по всем сервисам\n\tЕсли false, выполняет поиск в базе Ip-адресов по текущему сервису \n\tПример: bantime.overalljails = false" } },
        //    { "bantime.rndtime", new RuleInfo() { Type = RuleType.BantimeRndtime, Category = RuleCategory.Additional,
        //        Description = "Максимальное количество секунд, которое используется для использования со случайным временем, чтобы обойти \"умных\" ботнетов, высчитавающих точное время, на которое IP будет снова разбанено\n\tПример: 10" } },
        //    { "before", new RuleInfo() { Type = RuleType.Before, Category = RuleCategory.Specific } },
        //    { "blocktype", new RuleInfo() { Type = RuleType.Blocktype, Category = RuleCategory.Specific } },
        //    { "chain", new RuleInfo() { Type = RuleType.Chain, Category = RuleCategory.Specific } },
        //    { "destemail", new RuleInfo() { Type = RuleType.Destemail, Category = RuleCategory.Specific } },
        //    { "fail2ban_agent", new RuleInfo() { Type = RuleType.Fail2BanAgent, Category = RuleCategory.Specific } },
        //    { "filter", new RuleInfo() { Type = RuleType.Filter, Category = RuleCategory.Required } },
        //    { "findtime", new RuleInfo() { Type = RuleType.Findtime, Category = RuleCategory.Additional,
        //        Description = "Хост банится если неудачные попытки повторяются в течение последних \"findtime\" секунд\n\tПример: 10m" } },
        //    { "ignorecommand", new RuleInfo() { Type = RuleType.Ignorecommand, Category = RuleCategory.Specific } },
        //    { "ignoreip", new RuleInfo() { Type = RuleType.Ignoreip, Category = RuleCategory.Specific } },
        //    { "igonoreself", new RuleInfo() { Type = RuleType.Ignoreself, Category = RuleCategory.Specific} },
        //    { "knocking_url", new RuleInfo() { Type = RuleType.KnockingUrl, Category = RuleCategory.Specific } },
        //    { "logencoding", new RuleInfo() { Type = RuleType.Logencoding, Category = RuleCategory.Additional,
        //        Description = "Определяет кодировку логов\n\tНапример: \"ascii\", \"utf-8\"\n\tПо умолчанию: auto" } },
        //    { "logpath", new RuleInfo() { Type = RuleType.Logpath, Category = RuleCategory.Required } },
        //    { "maxretry", new RuleInfo() { Type = RuleType.Maxretry, Category = RuleCategory.Recommended,
        //        Description = "Количество неудачных попыток перед баном\n\tПример: 5" } },
        //    { "mode", new RuleInfo() { Type = RuleType.Mode, Category = RuleCategory.Specific } },
        //    { "mta", new RuleInfo() { Type = RuleType.Mta, Category = RuleCategory.Specific } },
        //    { "port", new RuleInfo() { Type = RuleType.Port, Category = RuleCategory.Required } },
        //    { "protocol", new RuleInfo() { Type = RuleType.Protocol, Category = RuleCategory.Recommended,
        //        Description = "Стандартный протокол\n\tПример: tcp" } },
        //    { "returntype", new RuleInfo() { Type = RuleType.Returntype, Category = RuleCategory.Specific } },
        //    { "sender", new RuleInfo() { Type = RuleType.Sender, Category = RuleCategory.Specific } },
        //    { "tcpport", new RuleInfo() { Type = RuleType.Tcpport, Category = RuleCategory.Specific } },
        //    { "udpport", new RuleInfo() { Type = RuleType.Udpport, Category = RuleCategory.Specific } },
        //    { "usedns", new RuleInfo()
        //                    {
        //                        Type = RuleType.Usedns,
        //                        Category = RuleCategory.Additional,
        //                        Description = "Определяет, используется ли в блокировке обратный DNS\n\t" +
        //                                      "Доступные значения: \"yes\", \"no\", \"warn\", \"raw\"\n\t" +
        //                                      "no - fail2ban будет блокировать IP-адреса вместо имен хостов\n\t" +
        //                                      "warn - попытается использовать обратный DNS для поиска имени хоста и его блокировки, но будет регистрировать активность в логе.\n\t" +
        //                                      "Пример: warn"
        //                    }
        //    },
        //    { "norestored", new RuleInfo() { Type = RuleType.norestored, Category = RuleCategory.Specific } },
        //    { "name", new RuleInfo() { Type = RuleType.Name, Category = RuleCategory.Specific } },

        //    { "action_vic", new RuleInfo() { Type = RuleType.ActionVic, Category = RuleCategory.Specific } },
        //    { "failregex", new RuleInfo() { Type = RuleType.Failregex, Category = RuleCategory.Specific, Description = "Регулярное выражение, по которому выбираются неудачные попытки"} }
        //};
    }
}
