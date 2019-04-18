using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server
{
    public static class Commands
    {
        public static Dictionary<string, bool> ManageJail(IServiceContainer serviceContainer)
        {
            var result = new Dictionary<string, bool>();

            foreach (var (_, jail) in serviceContainer.Jails)
            {
                var enabled = jail.GetRule(jail.Name, RuleType.Enabled) == "true";
                result.Add(jail.Name, enabled);
            }

            return result;
        }

        public static Dictionary<string, string> ManageJail(IServiceContainer serviceContainer, string jailName)
        {
            var result = new Dictionary<string, string>();

            var confSections = serviceContainer.GetJail(jailName).ConfSetting.Sections;
            if (confSections != null)
            {
                foreach (var (name, section) in confSections)
                {
                    foreach (var (rule, value) in section.Rules)
                    {
                        result[rule.ToString()] = value;
                    }
                }
            }

            var localSections = serviceContainer.GetJail(jailName).LocalSetting.Sections;
            if (localSections != null)
            {
                foreach (var (name, section) in localSections)
                {
                    foreach (var (rule, value) in section.Rules)
                    {
                        result[rule.ToString()] = value;
                    }
                }
            }
            
            return result;
        }

        public static void ChangeJailRule(IServiceContainer serviceContainer, string jailName, string ruleName, string ruleValue, IServiceSaver serviceSaver)
        {
            if (!RuleTypeExtension.TryParse(ruleName, out var rule))
            {
                throw new VicException("Wrong rule name");
            }

            var service = serviceContainer.GetJail(jailName);
            service.SetRule(jailName, rule, ruleValue);
            serviceSaver.Save(service);
        }

        public static void PrepareMail(IServiceContainer serviceContainer, IServiceFactory serviceFactory, ISettingFactory settingFactory, string senderName, string smtpUser, string mailTo, IServiceSaver defaultSaver, IServiceSaver actionSaver)
        {
            var actionBan = "dotnet /_Data/CLI/Client/FailToBan.Client.dll _logban <ip> <name>";
            var actionUnban = "dotnet /_Data/CLI/Client/FailToBan.Client.dll _logunban <ip> <name>";


            var sendMailService = serviceContainer.GetAction("sendmail-vic") ?? 
                                  serviceFactory.BuildService("sendmail-vic");

            if (sendMailService.LocalSetting == null) sendMailService.LocalSetting = settingFactory.Build();

            var sendMailInclude = sendMailService.LocalSetting.GetOrCreateSection("INCLUDES");
            var sendMailDefinition = sendMailService.LocalSetting.GetOrCreateSection("Definition");
            var sendMailInit = sendMailService.LocalSetting.GetOrCreateSection("Init");

            sendMailInclude.SetRule(RuleType.Before, "helpers-common.conf");
            sendMailDefinition.SetRule(RuleType.Norestored, "1");
            sendMailDefinition.SetRule(RuleType.Actionban, actionBan);
            sendMailDefinition.SetRule(RuleType.Actionunban, actionUnban);
            sendMailInit.SetRule(RuleType.Name, "default");
            sendMailInit.SetRule(RuleType.Sender, smtpUser);
            var defaultService = serviceContainer.GetDefault();

            var action = defaultService.GetRule("DEFAULT", RuleType.Action_) ?? "";

            var actionBanVic = action +
                "\n\t\t\tsendmail-vic[name=%(__name__)s, logpath=%(logpath)s]";

            defaultService.SetRule("DEFAULT", RuleType.ActionVic, actionBanVic);
            defaultService.SetRule("DEFAULT", RuleType.Action, "%(action_vic)s");

            actionSaver.Save(sendMailService);
            defaultSaver.Save(defaultService);
        }

        public static void PrepareBanAction(IServiceContainer serviceContainer, IServiceFactory serviceFactory, ISettingFactory settingFactory, IServiceSaver serviceSaver)
        {
            // Action: iptables-multiport
            // Action: iptables-allports
            var multiportConf = settingFactory.Build(File.ReadAllText("/etc/fail2ban/action.d/iptables-multiport.conf.base"));
            var multiport = serviceFactory.BuildService("iptables-multiport");
            multiport.ConfSetting = multiportConf;

            var allportsConf = settingFactory.Build(File.ReadAllText("/etc/fail2ban/action.d/iptables-allports.conf.base"));
            var allports = serviceFactory.BuildService("iptables-allports");
            allports.ConfSetting = allportsConf;

            var chainRegex = new Regex("[=]?(.)(<chain>)(.)");
            var rulesToReplace = new List<RuleType>
            {
                RuleType.Actionstart,
                RuleType.Actionstop,
                RuleType.Actionban,
                RuleType.Actionunban
            };
            ReplaceAction(multiport, rulesToReplace, chainRegex);
            ReplaceAction(allports, rulesToReplace, chainRegex);

            serviceContainer.SetAction(multiport);
            serviceContainer.SetAction(allports);

            serviceSaver.Save(multiport);
            serviceSaver.Save(allports);
        }

        private static void ReplaceAction(IService service, List<RuleType> rulesToReplace, Regex chainRegex)
        {
            foreach (var rule in rulesToReplace)
            {
                var ruleValue = service.GetRule("Definition", rule);
                if (ruleValue == null)
                {
                    continue;
                }

                var actionList = new List<string>(ruleValue.Split('\n'));

                var actionNew = "";
                foreach (var action in actionList)
                {
                    if (chainRegex.IsMatch(action))
                    {
                        actionNew += " " +
                            chainRegex.Replace(action, m => m.Groups[1].Value + "INPUT" + m.Groups[3].Value) +
                            Environment.NewLine;
                        actionNew += " " +
                            chainRegex.Replace(action, m => m.Groups[1].Value + "FORWARD" + m.Groups[3].Value) +
                            Environment.NewLine;
                    }
                    else
                    {
                        actionNew += action + Environment.NewLine;
                    }
                }

                service.SetRule("Definition", rule, actionNew);
            }
        }

        public static bool LogBan(IPAddress ip, string service, string mailLogFile, string statusLogFile, string whiteListFile)
        {
            using (var writer = new StreamWriter(File.Open(mailLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"ban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip};{service}");
            }

            using (var writer = new StreamWriter(File.Open(statusLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"ban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip};{service}");
            }

            var whiteList = WhiteListStatus(whiteListFile);
            if (whiteList.Contains(ip))
            {
                var result = $"fail2ban-client set {service} unbanip '{ip}'".Bash();
                return CheckBanResult(result);
            }

            return true;
        }

        private static IEnumerable<IPAddress> WhiteListStatus(string whiteListFile)
        {
            var result = new List<IPAddress>();
            var whiteText = File.ReadAllText(whiteListFile);
            var ipListStrings = whiteText.Split(new char[] {'\n', ' ', ','}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ipListString in ipListStrings)
            {
                if (IPAddress.TryParse(ipListString, out var address))
                {
                    result.Add(address);
                }
            }

            return result;
        }

        public static bool LogUnban(IPAddress ip, string service, string mailLogFile, string statusLogFile, string blackListFile)
        {
            using (var writer = new StreamWriter(File.Open(mailLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"unban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip};{service}");
            }

            using (var writer = new StreamWriter(File.Open(statusLogFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"unban;{DateTime.Now.ToString(Constants.TimeFormat)};{ip};{service}");
            }

            var blackList = BlackListStatus(blackListFile);
            if (blackList.Contains((ip, service)))
            {
                string result = $"fail2ban-client set {service} banip '{ip}'".Bash();
                return CheckBanResult(result);
            }
            return true;
        }

        public static List<(IPAddress, string)> BlackListStatus(string blackListFile)
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

        public static IEnumerable<(IPAddress, string)> BannedListStatus(string statusLogFile)
        {
            var result = new List<(IPAddress, string)>();
            var bans = ParseIps(statusLogFile);

            foreach (var (ipAddress, info) in bans)
            {
                var service = info
                    .Where(x => x.BanTime == info.Max(y => y.BanTime))
                    .FirstOrDefault(x => x.Type == BanInfo.BanType.Ban)?
                    .Service;

                if (service != null)
                {
                    result.Add((ipAddress, service));
                }
            }

            return result;
        }

        private static Dictionary<IPAddress, List<BanInfo>> ParseIps(string file)
        {
            var bans = new Dictionary<IPAddress, List<BanInfo>>();

            if (!File.Exists(file))
            {
                return bans;
            }

            using (var reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read)))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    var args = line.Split(';');

                    BanInfo.BanType type;
                    var ip = IPAddress.Parse(args[2]);
                    var time = DateTime.ParseExact(args[1], Constants.TimeFormat, new CultureInfo("ru-RU"));
                    var service = args[3];

                    if (args[0] == "ban")
                    {
                        type = BanInfo.BanType.Ban;
                    }
                    else if (args[0] == "unban")
                    {
                        type = BanInfo.BanType.Unban;
                    }
                    else
                    {
                        Console.WriteLine("Неверный тип действия в файле ip");
                        continue;
                    }

                    if (!bans.ContainsKey(ip))
                    {
                        bans[ip] = new List<BanInfo>();
                    }

                    bans[ip].Add(new BanInfo(type, time, service));
                }
            }

            return bans;
        }

        public static bool Ban(IPAddress ip, string service, string blackListFile, string whiteListFile)
        {
            var whiteIps = WhiteListStatus(whiteListFile);
            if (whiteIps.Contains(ip))
            {
                return false;
            }
            if (!BlackListStatus(blackListFile).Contains((ip, service)))
            {
                AddToBlackList(ip, service, blackListFile);
            }
            string result = $"fail2ban-client set {service} banip '{ip}'".Bash();
            return (CheckBanResult(result));
        }

        private static void AddToBlackList(IPAddress ip, string service, string blackListFile)
        {
            using (var writer = new StreamWriter(File.Open(blackListFile, FileMode.Append, FileAccess.Write)))
            {
                writer.WriteLine($"{ip};{service}");
            }
        }

        public static bool Unban(IPAddress ip, string service, string blackListFile)
        {
            DeleteFromBlackList(ip, blackListFile);
            var result = $"fail2ban-client set {service} unbanip '{ip}'".Bash();
            return (CheckBanResult(result));
        }

        private static void DeleteFromBlackList(IPAddress ip, string blackListFile)
        {
            if (!File.Exists(blackListFile))
            {
                return;
            }

            using (var reader = new StreamReader(File.Open(blackListFile, FileMode.Open, FileAccess.Read)))
            using (var writer = new StreamWriter(File.Open(blackListFile + ".bak", FileMode.Create, FileAccess.Write)))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    if (!Regex.IsMatch(line, $@".*{ip}.*"))
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
            "fail2ban-client unban --all".Bash();

            using (var reader = new StreamReader(File.Open(blackListFile, FileMode.OpenOrCreate, FileAccess.Read)))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(";");
                    if (IPAddress.TryParse(values[0], out var ip))
                    {
                        $"fail2ban-client set {values[1]} banip {ip}".Bash();
                    }
                }
            }
        }

        public static bool SendMail(string mailLogFile)
        {
            var bans = ParseIps(mailLogFile);

            if (bans == null || bans.Count == 0)
            {
                return false;
            }

            var message = CreateMessage(bans);
            var senderName = Environment.GetEnvironmentVariable("SenderName");
            var SMTPUser = Environment.GetEnvironmentVariable("SMTPUser");
            var result = $"/_Data/Scripts/SendMail.sh --AsHtml --Title \"[Fail2Ban] Список забаненных адресов от {senderName} за {DateTime.Now.ToString(Constants.TimeFormat)}\" --MailFrom \"{senderName} <{SMTPUser}>\" \"{message}\"".Bash();

            return CheckMailResult(result, mailLogFile);
        }

        private static bool CheckMailResult(string result, string mailLogFile)
        {
            if ((result != null) && Regex.IsMatch(result, @".*Email was sent successfully!.*"))
            {
                File.WriteAllText(mailLogFile, string.Empty);
                return true;
            }

            return false;
        }

        private static string CreateMessage(Dictionary<IPAddress, List<BanInfo>> bans)
        {
            var builder = new StringBuilder();
            var now = DateTime.Now.ToString(Constants.TimeFormat);
            var banInfo = CreateBanInfo(bans);

            builder.Append("<html><body>");
            builder.Append($"<h2>Статистика произведённых действий за {now}:</h2>");

            builder.Append("<div>Список забаненных адресов:</div>");
            foreach (var ban in banInfo)
            {
                var (ip, type, time, count, services) = ban;

                if (type != BanInfo.BanType.Ban) continue;

                builder.Append($"<div style='margin-left: 40px'><b>{ip}</b> был забанен {count} раз(а). ");

                var strings = new List<string>();
                foreach (var (service, banCount) in services)
                {
                    strings.Add($"{service} : {banCount}");
                }

                builder.Append($"Время первого бана: {time}. " +
                               $"Забанен в следующих сервисах: [{string.Join(", ", strings)}]. ");
                builder.Append("</div>");
            }

            builder.Append("<div>Список разбаненных адресов:</div>");
            foreach (var ban in banInfo)
            {
                var (ip, type, time, count, services) = ban;

                if (type != BanInfo.BanType.Unban) continue;

                builder.Append($"<div style='margin-left: 40px'><b>{ip}</b> был разбанен {count} раз(а). ");

                var strings = new List<string>();
                foreach (var (service, unbanCount) in services)
                {
                    strings.Add($"{service} : {unbanCount}");
                }

                builder.Append($"Время последнего разбана: {time}. " +
                               $"Разбанен в следующих сервисах: [{string.Join(", ", strings)}]. ");
                builder.Append("</div>");
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

                var ip = ban.Key;
                var lastActionType = ban
                    .Value
                    .First(x => x.BanTime == ban.Value.Max(z => z.BanTime))
                    .Type;
                int count;
                List<(string, int)> services;
                DateTime time;

                if (lastActionType == BanInfo.BanType.Ban)
                {
                    time = banList.Max(x => x.BanTime);
                    count = banList.Count();
                    services = banList
                        .GroupBy(x => x.Service)
                        .Select(x => (x.Key, banList.Count(y => y.Service == x.Key)))
                        .ToList();
                }
                else
                {
                    time = unbanList.Min(x => x.BanTime);
                    count = unbanList.Count();
                    services = unbanList
                        .GroupBy(x => x.Service)
                        .Select(x => (x.Key, unbanList.Count(y => y.Service == x.Key)))
                        .ToList();
                }

                banInfo.Add((ip, lastActionType, time.ToString(Constants.TimeFormat), count, services));
            }

            return banInfo;
        }

        public static void PrepareFilters(string filtersPath)
        {
            using (var reader = new StreamReader(File.Open(filtersPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                var file = "";
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

        public static void PrepareWhiteList(IServiceContainer serviceContainer, string path, IServiceSaver defaultSaver)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
                return;
            }

            var ipAddresses = new List<IPAddress>();
            var builder = new StringBuilder();
            using (var reader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (IPAddress.TryParse(line, out var ipAddress))
                    {
                        ipAddresses.Add(ipAddress);
                    }
                    else if (Regex.IsMatch(line, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\/\d{1,2}$"))
                    {
                        builder.Append(line + " ");
                    }
                    else
                    {
                        Console.WriteLine($"Ip адрес {line} не распознан и не добавлен в white list");
                    }
                    line = reader.ReadLine();
                }
            }

            builder.AppendJoin(" ", ipAddresses);

            var defaultJail = serviceContainer.GetDefault();
            defaultJail.SetRule("DEFAULT", RuleType.Ignoreip, builder.ToString());
            defaultSaver.Save(defaultJail);
        }
    }
}