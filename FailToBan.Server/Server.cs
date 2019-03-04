using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FailToBan.Core;
using FailToBan.Server.Shells;

namespace FailToBan.Server
{
    public class Server
    {
        private static IServiceContainer serviceContainer;
        private static IServiceSaver defaultSaver;
        private static IServiceSaver jailSaver;
        private static IServiceSaver actionSaver;
        private static IServiceSaver filterSaver;

        private static ISettingFactory settingFactory;
        private static IServiceFactory serviceFactory;
        private static Logger logger;
        private static Dictionary<int, Shell> sessions;

        private static async Task Main(string[] args)
        {
            logger = new Logger();
            LogData("Логгер запущен", Logger.From.Unknown, Logger.LogType.Debug);

            if (!Directory.Exists("/_Data/Confs/Jails"))
            {
                LogData("Создание каталога для Jail'ов", Logger.From.Unknown, Logger.LogType.Debug);
                Directory.CreateDirectory("/_Data/Confs/Jails");
            }

            try
            {
                await Start();
            }
            catch (VicException e)
            {
                LogData("Message: " + e.Message, Logger.From.Unknown, Logger.LogType.Error);
                LogData("Type: " + e.GetType().ToString(), Logger.From.Unknown, Logger.LogType.Error);
                LogData("Trace: " + e.StackTrace, Logger.From.Unknown, Logger.LogType.Error);
            }
            catch (Exception e)
            {
                LogData("Message: " + e.Message, Logger.From.Unknown, Logger.LogType.Error);
                LogData("Type: " + e.GetType().ToString(), Logger.From.Unknown, Logger.LogType.Error);
                LogData("Trace: " + e.StackTrace, Logger.From.Unknown, Logger.LogType.Error);
                throw e;
            }
        }

        private static async Task Start()
        {
            try
            {
                using (var server = new ServerPipe("VICFTB"))
                {
                    try
                    {
                        settingFactory = new SettingFactory(Constants.SectionRegex, Constants.KeyValueRegex, Constants.ContinueRegex);
                        serviceFactory = new ServiceFactory(settingFactory);
                        IServiceContainerBuilder serviceContainerBuilder = new ServiceContainerBuilder(serviceFactory, settingFactory);
                        serviceContainer = serviceContainerBuilder
                            .BuildDefault("/etc/fail2ban")
                            .BuildJails("/etc/fail2ban/jail.d")
                            .BuildJails("/_Data/Confs/Jails")
                            .BuildActions("/etc/fail2ban/action.d")
                            .BuildFilters("/etc/fail2ban/filter.d")
                            .BuildFilters("/_Data/Confs/Filters")
                            .Build();

                        LogData("Контейнер инициализирован", Logger.From.Server, Logger.LogType.Debug);
                        Prepare();
                    }
                    catch (Exception e)
                    {
                        LogData("Подготовка не удалась", Logger.From.Server, Logger.LogType.Debug);
                        throw e.InnerException;
                    }
                    "fail2ban-client start".Bash();
                    LogData("Fail2Ban запущен", Logger.From.Server, Logger.LogType.Debug);
                    sessions = new Dictionary<int, Shell>();

                    var mailTask = MailSendProcess().ContinueWith(task =>
                    {
                        LogData($"Возникло исключение при отправле письма", Logger.From.Server, Logger.LogType.Error);
                        var exception = task.Exception;
                        LogData(exception?.Message, Logger.From.Server, Logger.LogType.Error);
                        LogData(exception?.InnerException.Message, Logger.From.Server, Logger.LogType.Error);
                        LogData(exception?.StackTrace, Logger.From.Server, Logger.LogType.Error);
                    }, TaskContinuationOptions.OnlyOnFaulted);

                    while (true)
                    {
                        var clientId = await server.WaitForConnectionAsync();
                        LogData($"Подключился клиент с id: {clientId}", Logger.From.Server, Logger.LogType.Debug);
                        var processTask = ProcessClientAsync(server, clientId).ContinueWith(task =>
                        {
                            LogData($"Возникло исключения у клиента с id: {clientId}", Logger.From.Server, Logger.LogType.Error);
                            var exception = task.Exception;
                            LogData(exception?.Message, Logger.From.Server, Logger.LogType.Error);
                            LogData(exception?.InnerException.Message, Logger.From.Server, Logger.LogType.Error);
                            LogData(exception?.StackTrace, Logger.From.Server, Logger.LogType.Error);
                        }, TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static async Task ProcessClientAsync(ServerPipe server, int clientId)
        {
            while (server.IsConnected(clientId))
            {
                var message = await server.ReadAsync(clientId);
                var (response, con) = await ProcessMessageAsync(clientId, message);
                //LogData($"Ответ сервера: {response}", Logger.From.Server, Logger.LogType.Debug);
                if (con)
                {
                    await server.WriteAsync(response, clientId);
                }
                else
                {
                    await server.WriteLastAsync(response, clientId);
                }
            }
        }

        private static void Prepare()
        {
            LogData("Подготовка почты и действий для бана", Logger.From.Server, Logger.LogType.Debug);

            Commands.PrepareBanAction(serviceContainer, actionSaver);

            var senderName = Environment.GetEnvironmentVariable("SenderName");
            var smtpUser = Environment.GetEnvironmentVariable("SMTPUser");
            var mailTo = Environment.GetEnvironmentVariable("MailTo");
            var mailRepeatTime = Environment.GetEnvironmentVariable("MailRepeatTime");

            if (senderName == null || smtpUser == null || mailTo == null || mailRepeatTime == null)
            {
                throw new Exception("Перед запуском нужно указать переменные окружения SMTPUser, MailTo SenderName и mailRepeatTime");
            }

            Commands.PrepareMail(serviceContainer, serviceFactory,  senderName, smtpUser, mailTo, defaultSaver, actionSaver);
            Commands.PrepareFilters(Constants.FiltersListPath);
            Commands.PrepareWhiteList(settingContainer);
        }

        /// <summary>
        /// Читает приходящее сообщение и обрабатывает его
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <param name="result">Входящее сообщение</param>
        /// <returns>Кортеж из обработанного сообщения и логического значения, сообщающего продолжать ли серверу обработку этого клиента</returns>
        private static async Task<(string, bool)> ProcessMessageAsync(int id, string result)
        {
            LogData($"Сообщение от клиента с id = {id}", Logger.From.Server, Logger.LogType.Debug);

            var (command, values) = Extension.ParseArgs(result.Split("\n").ToList());
            LogData($"Команда: {command}", Logger.From.Server, Logger.LogType.Debug);
            LogData($"Значения: {string.Join(" ", values)}", Logger.From.Server, Logger.LogType.Debug);

            switch (command)
            {
                case "create":
                    LogData("Запрошено интерактивное создание jail", Logger.From.Server, Logger.LogType.Debug);
                    sessions.Add(id, new CreateShell(id, settingContainer, logger));
                    var createMessage = sessions[id].Get(values.ToArray());
                    LogData(createMessage, Logger.From.Server, Logger.LogType.Debug);
                    if (createMessage == $"\n{Constants.CreateTexts[Constants.CreateSteps.Exit]}")
                    {
                        return (createMessage, false);
                    }

                    return (createMessage, true);

                case "edit":
                    LogData("Запрошено интерактивное изменение jail", Logger.From.Server, Logger.LogType.Debug);
                    sessions.Add(id, new EditShell(id, settingContainer, logger));
                    var editMessage = sessions[id].Get(values.ToArray());
                    LogData(editMessage, Logger.From.Server, Logger.LogType.Debug);
                    if (editMessage == $"\n{Constants.EditTexts[Constants.EditSteps.Exit]}")
                    {
                        return (editMessage, false);
                    }

                    return (editMessage, true);
            }

            if (!sessions.ContainsKey(id))
            {
                sessions.Add(id, null);
                LogData("Команда", Logger.From.Server, Logger.LogType.Debug);
                return (Command(command, values, id), false);
            }

            if (result == "q")
            {
                LogData("Клиент запросил отключение", Logger.From.Server, Logger.LogType.Debug);
                return ("Выход", false);
            }

            LogData($"Клиент ввёл интерактивную команду \"{result}\"", Logger.From.Server, Logger.LogType.Debug);
            return (sessions[id].Get(result.Split(" ")), true);
        }

        private static async Task MailSendProcess()
        {
            var mailRepeatTime = Environment.GetEnvironmentVariable("MailRepeatTime");
            if (!int.TryParse(mailRepeatTime, out var time))
            {
                LogData("Переменная окружения MailRepeatTime не задана или задана в неверном формате", Logger.From.Server, Logger.LogType.Error);
            }
            var currentSendTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var nextSendTime = time + time * (currentSendTime / time);

            LogData($"mailRepeat: {time}", Logger.From.Server, Logger.LogType.Debug);
            LogData($"currentTime: {currentSendTime}", Logger.From.Server, Logger.LogType.Debug);
            LogData($"nextTime: {nextSendTime}", Logger.From.Server, Logger.LogType.Debug);

            while (true)
            {
                currentSendTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

                if (currentSendTime >= nextSendTime)
                {
                    Commands.SendMail();
                    nextSendTime = currentSendTime + time;

                    LogData($"currentTime: {currentSendTime}", Logger.From.Server, Logger.LogType.Debug);
                    LogData($"nextTime: {nextSendTime}", Logger.From.Server, Logger.LogType.Debug);
                }

                await Task.Delay(1000);
            }
        }

        private static string Command(string command, List<string> values, int id)
        {
            switch (command)
            {
                case "manage":
                    if (values.Count > 0)
                    {
                        LogData("Запрошен вывод состояния всех jail", Logger.From.Server, Logger.LogType.Debug);
                        var builder = new StringBuilder();
                        foreach (var jail in values)
                        {
                            var jailInfo = Commands.ManageJail(settingContainer, jail);
                            var jailRules = PrintJailRules(jail, jailInfo, id);
                            builder.AppendLine(jailRules);
                        }

                        return builder.ToString();
                    }
                    else
                    {
                        LogData("Запрошен вывод параметров jail", Logger.From.Server, Logger.LogType.Debug);
                        var jails = Commands.ManageJail(settingContainer);
                        return PrintJailsStatus(jails, id);
                    }

                case "enable":
                    LogData("Запрошено включение jail", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 1)
                    {
                        Commands.ChangeJailRule(settingContainer, values[0], "enabled", "true");
                        "fail2ban-client restart".Bash();
                        return "Сервис был включён";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }


                case "disable":
                    LogData("Запрошено выключение jail", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 1)
                    {
                        Commands.ChangeJailRule(settingContainer, values[0], "enabled", "false");
                        "fail2ban-client restart".Bash();
                        return "Сервис был выключен";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }


                case "setjail":
                    // jail, rule, value
                    LogData("Запрошено изменение jail", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 3)
                    {
                        if (values[1] == "enabled" || values[1] == "disabled")
                        {
                            return "Для включения или выключения сервисов используйте команду enable или disable";
                        }

                        Commands.ChangeJailRule(settingContainer, values[0], values[1], values[2]);
                        "fail2ban-client restart".Bash();
                        return "Правило сервиса было изменено";
                    }
                    else
                    {
                        return "Сервис не был изменён";
                    }


                case "setaction":
                    // action, section, rule, value
                    LogData("Запрошено изменение action", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 4)
                    {
                        Commands.ChangeActionRule(settingContainer, values[0], values[3], values[1], values[2]);
                        "fail2ban-client restart".Bash();
                        return "Действие было изменено";
                    }
                    else
                    {
                        return "Действие не было изменено";
                    }

                case "setfilter":
                    // filter, section, rule, value
                    LogData("Запрошено изменение filter", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 4)
                    {
                        Commands.ChangeFilterRule(settingContainer, values[0], values[1], values[2], values[3]);
                        "fail2ban-client restart".Bash();
                        return "Фильтр был изменён";
                    }
                    else
                    {
                        return "Фильтр не был изменён";
                    }

                case "stop":
                    LogData("Запрошена остановка контейнера.", Logger.From.Server, Logger.LogType.Debug);
                    "fail2ban-client stop".Bash();
                    Environment.Exit(0);
                    return "Контейнер был остановлен";

                case "status":
                    LogData("Запрошен вывод статуса забаненных адресов.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 0)
                    {
                        var builder = new StringBuilder();
                        var blackList = Commands.BlackListStatus();
                        var blackAddresses = PrintAddressList(blackList, id, "Addresses in black list:");
                        builder.AppendLine(blackAddresses);
                        var bannedList = Commands.BannedListStatus();
                        var bannedAddresses = PrintAddressList(bannedList, id, "Banned addresses:");
                        builder.AppendLine(bannedAddresses);
                        return builder.ToString();
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "ban":
                    LogData("Запрошен бан ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var bannedIp) &&
                        Commands.Ban(bannedIp, values[1]))
                    {
                        return "Ip адрес был забанен";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "unban":
                    // ip, service
                    LogData("Запрошен разбан ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var unbannedIp) &&
                        Commands.Unban(unbannedIp, values[1]))
                    {
                        return "Ip адрес был разбанен";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "reban":
                    LogData("Запрошен разбан всех ip и бан чёрного списка.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 0)
                    {
                        Commands.Reban();
                        return "Все адреса были разбанены";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "sendmail":
                    LogData("Отправка письма.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 0 && Commands.SendMail())
                    {
                        return "Письмо было отправлено";
                    }
                    else
                    {
                        return "Письмо не было отправлено";
                    }

                case "_logban":
                    // ip, service
                    LogData("Запрошено логгирование бана ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var logBannedIp) &&
                        Commands.LogBan(logBannedIp, values[1]))
                    {
                        LogData("Ip ban was logged", Logger.From.Server, Logger.LogType.Message);
                        return "Бан ip адреса был залогирован";
                    }
                    else
                    {
                        return "Бан ip адреса не был залогирован";
                    }

                case "_logunban":
                    // ip, service
                    LogData("Запрошено логгирование разбана ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var logUnbannedIp) &&
                        Commands.LogUnban(logUnbannedIp, values[1]))
                    {
                        return "Разбан ip адреса был залогирован";
                    }
                    else
                    {
                        return "Разбан ip адреса не был залогирован";
                    }

                default:
                    return "Неверная команда";
            }
        }



        private static string PrintJailsStatus(Dictionary<string, bool> jails, int id)
        {
            var builder = new StringBuilder();
            foreach (var jail in jails.Where(s => s.Value))
            {
                builder.AppendLine($"{jail.Key} : true");
            }

            foreach (var jail in jails.Where(s => s.Value == false))
            {
                builder.AppendLine($"{jail.Key} : false");
            }

            return builder.ToString();
        }

        private static string PrintJailRules(string jail, Dictionary<string, string> rules, int id)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[{jail}]");
            foreach (var (rule, value) in rules)
            {
                builder.AppendLine($"{rule} = {value}");
            }
            LogData(builder.ToString(), Logger.From.Server, Logger.LogType.Debug);

            return builder.ToString();
        }

        private static string PrintAddressList(IEnumerable<(IPAddress, string)> list, int id, string message)
        {
            var builder = new StringBuilder();
            builder.AppendLine(message);
            foreach (var (ipAddress, jail) in list)
            {
                builder.AppendLine($"{ipAddress} in jail {jail}");
            }

            return builder.ToString();
        }

        public static void LogData(string text, Logger.From @from, Logger.LogType type)
        {
            logger.Log(text, @from, type);
            Console.WriteLine(text);
        }
    }
}