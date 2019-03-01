using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FailToBan.Core;
using Microsoft.VisualBasic;

namespace FailToBan.Server
{
    public static class Commands
    {
        public static Dictionary<string, bool> ManageJail(SettingContainer settingContainer)
        {
            var result = new Dictionary<string, bool>();

            GetEnabled(settingContainer.JailConf.Sections);

            foreach (var setting in settingContainer.JailConfSettings)
            {
                GetEnabled(setting.Sections);
            }

            GetEnabled(settingContainer.JailLocal.Sections);

            foreach (var setting in settingContainer.JailLocalSettings)
            {
                GetEnabled(setting.Sections);
            }

            return result;

            // Nested method
            void GetEnabled(List<Section> sections)
            {
                foreach (var section in sections)
                {
                    if (section.Name == "DEFAULT")
                    {
                        continue;
                    }

                    if (!result.ContainsKey(section.Name))
                    {
                        result.Add(section.Name, false);
                    }

                    if (section.HasRule(Rule.RuleType.enabled))
                    {
                        if (section.GetRuleValue(Rule.RuleType.enabled) == "true")
                        {
                            result[section.Name] = true;
                        }
                        else if (section.GetRuleValue(Rule.RuleType.enabled) == "false")
                        {
                            result[section.Name] = false;
                        }
                        else
                        {
                            throw new Exception("Неправильное значение правила enabled");
                        }
                    }
                }
            }
        }

        public static Dictionary<string, string> ManageJail(SettingContainer settingContainer, string jailName)
        {
            var result = new Dictionary<string, string>();

            GetRules(settingContainer.JailConf.Sections);

            foreach (var setting in settingContainer.JailConfSettings)
            {
                GetRules(setting.Sections);
            }

            GetRules(settingContainer.JailLocal.Sections);

            foreach (var setting in settingContainer.JailLocalSettings)
            {
                GetRules(setting.Sections);
            }

            return result;

            // Nested method
            void GetRules(List<Section> sections)
            {
                foreach (var section in sections)
                {
                    if (section.Name == jailName || section.Name == "DEFAULT")
                    {
                        foreach (var rule in section.Rules)
                        {
                            if (Constants.Rules[rule.Key].Category != Constants.RuleCategory.Specific)
                            {
                                result[rule.Key] = rule.Value;
                            }
                        }
                    }
                }
            }
        }

        public static void ToggleJail(SettingContainer settingContainer, string jailName, bool value, bool save = true)
        {
            settingContainer.SetJailValue(jailName, Rule.RuleType.enabled, value.ToString().ToLower());

            if (save)
            {
                settingContainer.Save();
            }
        }

        public static void ToggleJails(SettingContainer settingContainer, IEnumerable<string> jails, bool value, bool save = true)
        {
            foreach (var jail in jails)
            {
                settingContainer.SetJailValue(jail, Rule.RuleType.enabled, value.ToString().ToLower());
            }

            if (save)
            {
                settingContainer.Save();
            }
        }

        public static void ChangeJailRule(SettingContainer settingContainer, string jailName, string ruleName, string ruleValue, bool save = true)
        {
            settingContainer.SetJailValue(jailName, ruleName, ruleValue);

            if (save)
            {
                settingContainer.Save();
            }
        }

        public static void ChangeActionRule(SettingContainer settingContainer, string actionName, string sectionName, string ruleName, string ruleValue, bool save = true)
        {
            settingContainer.SetActionFilterValue(actionName, sectionName, ruleName, ruleValue, Setting.SettingType.action);

            if (save)
            {
                settingContainer.Save();
            }
        }

        public static void ChangeFilterRule(SettingContainer settingContainer, string filterName, string sectionName, string ruleName, string ruleValue, bool save = true)
        {
            settingContainer.SetActionFilterValue(filterName, sectionName, ruleName, ruleValue, Setting.SettingType.filter);

            if (save)
            {
                settingContainer.Save();
            }
        }

        public static void PrepareMail(SettingContainer settingContainer, string senderName, string SMTPUser, string mailTo, bool save = true)
        {
            //string actionBan = $"/_Data/Scripts/SendMail.sh --Title \"Subject: [Fail2Ban] <name>: banned <ip> from {senderName}\" --MailFrom \"" +
            //$"{senderName} <<sender>>\" \"Hi,\\n" +
            //"\n The IP <ip> has just been banned by Fail2Ban after" +
            //"\n <failures> attempts against <name>.\\n" +
            //"\n Regards,\\n" +
            //"\n Fail2Ban\"";

            string actionBan = "dotnet /_Data/CLI/VICFailToBan.dll _logban <ip> <name>";
            string actionUnban = "dotnet /_Data/CLI/VICFailToBan.dll _logunban <ip> <name>";

            Setting sendMailSetting = settingContainer.GetOrCreateSetting("sendmail-vic.local", Setting.SettingType.action);

            Section sendMailInclude = sendMailSetting.GetOrCreateSection("INCLUDES");
            Section sendMailDefinition = sendMailSetting.GetOrCreateSection("Definition");
            Section sendMailInit = sendMailSetting.GetOrCreateSection("Init");

            sendMailInclude.SetOrCreateRule(Rule.RuleType.before, "helpers-common.conf");
            sendMailDefinition.SetOrCreateRule(Rule.RuleType.norestored, "1");
            sendMailDefinition.SetOrCreateRule(Rule.RuleType.actionban, actionBan);
            sendMailDefinition.SetOrCreateRule(Rule.RuleType.actionunban, actionUnban);
            sendMailInit.SetOrCreateRule(Rule.RuleType.name, "default");
            sendMailInit.SetOrCreateRule(Rule.RuleType.sender, SMTPUser);

            Section jailConfDefault = settingContainer.JailConf.GetOrCreateSection("DEFAULT");

            string action = "";
            if (jailConfDefault.HasRule("action_"))
            {
                action = jailConfDefault.GetRuleValue("action_");
            }

            string actionBanVIC = action +
                "\n\t\t\tsendmail-vic[name=%(__name__)s, logpath=%(logpath)s]";

            Section jailLocalDefault = settingContainer.JailLocal.GetOrCreateSection("DEFAULT");

            jailLocalDefault.SetOrCreateRule(Rule.RuleType.action_vic, actionBanVIC);
            jailLocalDefault.SetOrCreateRule(Rule.RuleType.action, "%(action_vic)s");

            if (save)
            {
                settingContainer.Save();
            }
        }

        public static void PrepareBanAction(string path = Constants.JailConfPath)
        {
            string oldPath = Path.Combine(path, "action.d", "iptables-multiport.conf.base");
            string newPath = Path.Combine(path, "action.d", "iptables-multiport.conf");

            ReplaceChain("iptables-multiport");
            File.Move(oldPath, newPath);

            oldPath = Path.Combine(path, "action.d", "iptables-allports.conf.base");
            newPath = Path.Combine(path, "action.d", "iptables-allports.conf");

            ReplaceChain("iptables-allports");
            File.Move(oldPath, newPath);

            void ReplaceChain(string fileName)
            {
                Setting setting = new Setting(oldPath, Setting.SettingType.action);

                Regex chainRegex = new Regex("[=]?(.)(<chain>)(.)");

                List<Rule.RuleType> rulesToReplace = new List<Rule.RuleType>
                {
                    Rule.RuleType.actionstart,
                    Rule.RuleType.actionstop,
                    Rule.RuleType.actionban,
                    Rule.RuleType.actionunban
                };

                foreach (Rule.RuleType rule in rulesToReplace)
                {
                    if (!setting.HasRule(rule))
                    {
                        continue;
                    }

                    string actionOld = setting.GetRuleValue(rule);

                    var actionList = new List<string>(actionOld.Split('\n'));

                    string actionNew = "";
                    foreach (string action in actionList)
                    {
                        if (chainRegex.IsMatch(action))
                        {
                            actionNew += chainRegex.Replace(action, (m) => m.Groups[1].Value + "INPUT" + m.Groups[3].Value) + "\n ";
                            actionNew += chainRegex.Replace(action, (m) => m.Groups[1].Value + "FORWARD" + m.Groups[3].Value) + "\n ";
                        }
                        else
                        {
                            actionNew += action + '\n';
                        }
                    }

                    setting.SetRule(rule, actionNew);

                }

                setting.Save();

            }
        }

        public static bool LogBan(IPAddress ip, string service, string mailLogFile = Constants.MailLogPath, string statusLogFile = Constants.StatusLogPath)
        {
            Console.WriteLine("START WRITE TO MAIL LOG");
            using (var writer = new StreamWriter(File.Open(mailLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"ban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip.ToString()};{service}");
            }

            Console.WriteLine("START WRITE TO STATUS LOG");
            using (var writer = new StreamWriter(File.Open(statusLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"ban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip.ToString()};{service}");
            }

            return true;
        }

        public static bool LogUnban(IPAddress ip, string service, string mailLogFile = Constants.MailLogPath, string statusLogFile = Constants.StatusLogPath)
        {
            Console.WriteLine("START LOG UNBAN");
            Console.WriteLine("START WRITE TO MAIL LOG");
            using (var writer = new StreamWriter(File.Open(mailLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"unban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip.ToString()};{service}");
            }

            Console.WriteLine("START WRITE TO STATUS LOG");
            using (var writer = new StreamWriter(File.Open(statusLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"unban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip.ToString()};{service}");
            }

            var blackList = BlackListStatus();
            Console.WriteLine("GOT BLACK LIST");
            if (blackList.Contains((ip, service)))
            {
                Console.WriteLine("START BAN BY FAIL2BAN");
                string result = $"fail2ban-client set {service} banip '{ip.ToString()}'".Bash();
                Console.WriteLine("FINISH BAN BY FAIL2BAN");
                Console.WriteLine("FINISH LOG UNBAN");
                return CheckBanResult(result);
            }
            Console.WriteLine("FINISH LOG UNBAN");
            return true;
        }

        public static List<(IPAddress, string)> BlackListStatus(string blackListFile = Constants.BlackListPath)
        {
            var result = new List<(IPAddress, string)>();
            using (var reader = new StreamReader(File.Open(blackListFile, FileMode.OpenOrCreate, FileAccess.Read)))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(";");
                    if (IPAddress.TryParse(values[0], out IPAddress ip))
                    {
                        result.Add((ip, values[1]));
                    }

                }
            }

            return result;
        }

        public static List<(IPAddress, string)> BannedListStatus(string statusLogFile = Constants.StatusLogPath)
        {
            var result = new List<(IPAddress, string)>();
            var bans = Extension.ParseIps(statusLogFile);

            foreach (var ban in bans)
            {
                string service = ban.Value.Where(x =>
                {
                    return (x.BanTime == ban.Value.Max(y => y.BanTime));
                }).FirstOrDefault(x => x.Type == BanInfo.BanType.Ban)?.Service;

                if (service != null)
                {
                    result.Add((ban.Key, service));
                }
            }

            return result;
        }

        public static bool Ban(IPAddress ip, string service)
        {
            Console.WriteLine("START BAN");
            if (!BlackListStatus().Contains((ip, service)))
            {
                AddToBlackList(ip, service);
            }
            string result = $"fail2ban-client set {service} banip '{ip.ToString()}'".Bash();
            Console.WriteLine("FINISH BAN");
            return (CheckBanResult(result));
        }

        private static void AddToBlackList(IPAddress ip, string service, string blackListFile = Constants.BlackListPath)
        {
            using (var writer = new StreamWriter(File.Open(blackListFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"{ip.ToString()};{service}");
            }
        }

        public static bool Unban(IPAddress ip, string service)
        {
            DeleteFromBlackList(ip);
            Console.WriteLine("START UNBAN");
            string result = $"fail2ban-client set {service} unbanip '{ip.ToString()}'".Bash();
            //LogUnban(ip, service);
            Console.WriteLine("FINISH UNBAN");
            return (CheckBanResult(result));
        }

        private static void DeleteFromBlackList(IPAddress ip, string blackListFile = Constants.BlackListPath)
        {
            if (!File.Exists(blackListFile))
            {
                return;
            }

            using (var reader = new StreamReader(File.Open(blackListFile, FileMode.Open, FileAccess.Read)))
            using (var writer = new StreamWriter(File.Open(blackListFile + ".bak", FileMode.Create, FileAccess.Write)))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (!Regex.IsMatch(line, $@".*{ip.ToString()}.*"))
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            if (File.Exists(blackListFile))
            {
                File.Delete(blackListFile);
            }
            File.Move(blackListFile + ".bak", blackListFile);
        }

        private static bool CheckBanResult(string result)
        {
            return ((result != null) && Regex.IsMatch(result, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"));
        }

        public static void Reban(string blackListFile = Constants.BlackListPath)
        {
            $"fail2ban-client unban --all".Bash();

            using (var reader = new StreamReader(File.Open(blackListFile, FileMode.OpenOrCreate, FileAccess.Read)))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    string[] values = line.Split(";");
                    if (IPAddress.TryParse(values[0], out IPAddress ip))
                    {
                        $"fail2ban-client set {values[1]} banip {ip.ToString()}".Bash();
                    }
                }
            }
        }

        public static bool SendMail(string mailLogFile = Constants.MailLogPath)
        {
            var bans = Extension.ParseIps(mailLogFile);

            if (bans == null || bans.Count == 0)
            {
                return false;
            }

            string message = CreateMessage(bans);
            string senderName = Environment.GetEnvironmentVariable("SenderName");
            string SMTPUser = Environment.GetEnvironmentVariable("SMTPUser");
            string result = $"/_Data/Scripts/SendMail.sh --AsHtml --Title \"[Fail2Ban] Список забаненных адресов от {senderName} за {DateTime.Now.ToString(Constants.TimeFormat)}\" --MailFrom \"{senderName} <{SMTPUser}>\" \"{message}\"".Bash();

            return CheckMailResult(result, mailLogFile);
        }

        private static bool CheckMailResult(string result, string mailLogFile = Constants.MailLogPath)
        {
            if ((result != null) && Regex.IsMatch(result, @".*Email was sent successfully!.*"))
            {
                File.WriteAllText(mailLogFile, string.Empty);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string CreateMessage(Dictionary<IPAddress, List<BanInfo>> bans)
        {
            StringBuilder builder = new StringBuilder();
            string now = DateTime.Now.ToString(Constants.TimeFormat);
            var banInfo = CreateBanInfo(bans);

            builder.Append($"<html><body>");
            builder.Append($"<h2>Статистика произведённых действий за {now}:</h2>");

            builder.Append($"<div>Список забаненных адресов:</div>");
            foreach (var ban in banInfo)
            {
                var (ip, type, time, count, services) = ban;

                if (type == BanInfo.BanType.Ban)
                {
                    builder.Append($"<div style='margin-left: 40px'><b>{ip}</b> был забанен {count} раз(а). ");

                    List<string> strings = new List<string>();
                    foreach (var service in services)
                    {
                        strings.Add($"{service.Item1} : {service.Item2}");
                    }

                    builder.Append($"Время первого бана: {time}. " +
                        $"Забанен в следующих сервисах: [{string.Join(", ", strings)}]. ");
                    builder.Append("</div>");
                }
            }

            builder.Append($"<div>Список разбаненных адресов:</div>");
            foreach (var ban in banInfo)
            {
                var (ip, type, time, count, services) = ban;

                if (type == BanInfo.BanType.Unban)
                {
                    builder.Append($"<div style='margin-left: 40px'><b>{ip}</b> был разбанен {count} раз(а). ");

                    List<string> strings = new List<string>();
                    foreach (var service in services)
                    {
                        strings.Add($"{service.Item1} : {service.Item2}");
                    }

                    builder.Append($"Время последнего разбана: {time}. " +
                        $"Разбанен в следующих сервисах: [{string.Join(", ", strings)}]. ");
                    builder.Append("</div>");
                }
            }

            builder.Append("</body></html>");

            return builder.ToString();
        }

        private static List<(IPAddress, BanInfo.BanType, string, int, List<(string, int)>)> CreateBanInfo(Dictionary<IPAddress, List<BanInfo>> bans)
        {
            var banInfo = new List<(IPAddress, BanInfo.BanType, string, int, List<(string, int)>)>();

            foreach (var ban in bans)
            {
                var banList = ban.Value.Where(x => x.Type == BanInfo.BanType.Ban);
                var unbanList = ban.Value.Where(x => x.Type == BanInfo.BanType.Unban);

                IPAddress ip = ban.Key;
                BanInfo.BanType lastActionType = ban.Value.First(x => x.BanTime == ban.Value.Max(z => z.BanTime)).Type;
                DateTime time = new DateTime();
                int count = 0;
                List<(string, int)> services = new List<(string, int)>();

                if (lastActionType == BanInfo.BanType.Ban)
                {
                    time = banList.First(x => x.BanTime == banList.Max(z => z.BanTime)).BanTime; // last ban time
                    count = banList.Count();
                    services = banList.GroupBy(x => x.Service).Select(x => (x.Key, banList.Count(y => y.Service == x.Key))).ToList();
                }
                else
                {
                    time = unbanList.First(x => x.BanTime == unbanList.Min(z => z.BanTime)).BanTime; // first unban time
                    count = unbanList.Count();
                    services = unbanList.GroupBy(x => x.Service).Select(x => (x.Key, unbanList.Count(y => y.Service == x.Key))).ToList();
                }

                banInfo.Add((ip, lastActionType, time.ToString(Constants.TimeFormat), count, services));
            }

            return banInfo;
        }

        public static void PrepareFilters(string filtersPath)
        {
            using (var reader = new StreamReader(File.Open(filtersPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                string file = "";
                while (file != null)
                {
                    file = reader.ReadLine();
                    if (File.Exists(file))
                    {
                        $"ln -s /_Data/confs/Filters/{file} /etc/fail2ban/filter.d/{file}".Bash();
                    }
                }
            }
        }
    }
}