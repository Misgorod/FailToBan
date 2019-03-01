﻿using System;
using System.Collections.Generic;
using System.Data;
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
            foreach (var (name, section) in confSections)
            {
                foreach (var (rule, value) in section.Rules)
                {
                    result[rule.ToString()] = value;
                }
            }

            var localSections = serviceContainer.GetJail(jailName).LocalSetting.Sections;
            foreach (var (name, section) in localSections)
            {
                foreach (var (rule, value) in section.Rules)
                {
                    result[rule.ToString()] = value;
                }
            }

            return result;
        }

        public static void ToggleJail(IServiceContainer serviceContainer, string jailName, bool value, IServiceSaver serviceSaver)
        {
            var toggle = value ? "true" : "false";
            var service = serviceContainer.GetJail(jailName);
            service.SetRule(jailName, RuleType.Enabled, toggle);

            serviceSaver?.Save(service);
        }

        public static void ToggleJails(IServiceContainer serviceContainer, IEnumerable<string> jails, bool value, IServiceSaver serviceSaver)
        {
            foreach (var jail in jails)
            {
                ToggleJail(serviceContainer, jail, value, serviceSaver);
            }
        }

        public static void ChangeJailRule(IServiceContainer serviceContainer, string jailName, string ruleName, string ruleValue, IServiceSaver serviceSaver)
        {
            if (!RuleTypeExtension.TryParse(ruleName, out var rule))
            {
                throw new VicException("Wrong rule name");
            }

            var service = serviceContainer.GetJail(jailName);
            service.SetRule(jailName, rule, ruleValue);
            serviceSaver?.Save(service);
        }

        public static void ChangeActionRule(IServiceContainer serviceContainer, string jailName, string sectionName, string ruleName, string ruleValue, IServiceSaver serviceSaver)
        {
            if (!RuleTypeExtension.TryParse(ruleName, out var rule))
            {
                throw new VicException("Wrong rule name");
            }

            var service = serviceContainer.GetAction(jailName);
            service.SetRule(sectionName, rule, ruleValue);
            serviceSaver?.Save(service);
        }

        public static void ChangeFilterRule(IServiceContainer serviceContainer, string jailName, string sectionName, string ruleName, string ruleValue, IServiceSaver serviceSaver)
        {
            if (!RuleTypeExtension.TryParse(ruleName, out var rule))
            {
                throw new VicException("Wrong rule name");
            }

            var service = serviceContainer.GetFilter(jailName);
            service.SetRule(sectionName, rule, ruleValue);
            serviceSaver?.Save(service);
        }

        public static void PrepareMail(IServiceContainer serviceContainer, IServiceFactory serviceFactory, string senderName, string smtpUser, string mailTo, IServiceSaver serviceSaver)
        {
            var actionBan = "dotnet /_Data/CLI/VICFailToBan.dll _logban <ip> <name>";
            var actionUnban = "dotnet /_Data/CLI/VICFailToBan.dll _logunban <ip> <name>";


            var sendMailService = serviceContainer.GetAction("sendmail-vic") ?? 
                                  serviceFactory.BuildService("sendmail-vic");

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

            serviceSaver?.Save(sendMailService);
            serviceSaver?.Save(defaultService);
        }

        public static void PrepareBanAction(IServiceContainer serviceContainer, IServiceSaver serviceSaver)
        {
            // Action: iptables-multiport
            // Action: iptables-allports
            var multiport = serviceContainer.GetAction("iptables-multiport") ?? throw new VicException("There is no iptables-multiport");
            var allports = serviceContainer.GetAction("iptables-allports") ?? throw new VicException("There is no iptables-allports");

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

            serviceSaver?.Save(multiport);
            serviceSaver?.Save(allports);
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
                        actionNew +=
                            chainRegex.Replace(action, (m) => m.Groups[1].Value + "INPUT" + m.Groups[3].Value) +
                            Environment.NewLine;
                        actionNew +=
                            chainRegex.Replace(action, (m) => m.Groups[1].Value + "FORWARD" + m.Groups[3].Value) +
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

        public static bool LogBan(IPAddress ip, string service, string mailLogFile, string statusLogFile)
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

        public static bool LogUnban(IPAddress ip, string service, string mailLogFile, string statusLogFile, string blackListFile)
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

            var blackList = BlackListStatus(blackListFile);
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

        private static Dictionary<IPAddress, List<BanInfo>> ParseIps(string file)
        {
            var bans = new Dictionary<IPAddress, List<BanInfo>>();

            if (!File.Exists(file))
            {
                return bans;
            }

            using (var reader = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read)))
            {
                string line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    string[] args = line.Split(';');

                    BanInfo.BanType type;
                    var ip = IPAddress.Parse(args[2]);
                    var time = DateTime.ParseExact(args[1], Constants.TimeFormat, new CultureInfo("ru-RU"));
                    string service = args[3];

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