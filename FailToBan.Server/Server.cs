using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
            await LogDataAsync("Логгер запущен", Logger.From.Unknown, Logger.LogType.Debug);

            if (!Directory.Exists("/_Data/Confs/Jails"))
            {
                await LogDataAsync("Создание каталога для Jail'ов", Logger.From.Unknown, Logger.LogType.Debug);
                Directory.CreateDirectory("/_Data/Confs/Jails");
            }

            await StartAsync();
        }

        private static async Task StartAsync()
        {
            using (var server = new ServerPipe("VICFTB"))
            {
                try
                {
                    defaultSaver = new ServiceSaver("/etc/fail2ban");
                    jailSaver = new ServiceSaver("/_Data/Confs/Jails");
                    var customFilterSaver = new ServiceSaver("/_Data/Confs/Filters");
                    var originFilterSaver = new ServiceSaver("/etc/fail2ban/filter.d");
                    filterSaver = new ServiceSaverAdapter(customFilterSaver, originFilterSaver);
                    actionSaver = new ServiceSaver("/etc/fail2ban/action.d");

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

                    await LogDataAsync("Контейнер инициализирован", Logger.From.Server, Logger.LogType.Debug);
                    await PrepareAsync();
                }
                catch (Exception e)
                {
                    await LogDataAsync("Подготовка не удалась", Logger.From.Server, Logger.LogType.Debug);
                    //Console.WriteLine(e.Message);
                    //Console.WriteLine(e.StackTrace);
                    if (e.InnerException != null) throw e.InnerException;
                    throw e;
                }
                "fail2ban-client start".Bash();
                await LogDataAsync("Fail2Ban запущен", Logger.From.Server, Logger.LogType.Debug);
                sessions = new Dictionary<int, Shell>();

                var mailTask = MailSendProcessAsync().ContinueWith(async task =>
                {
                    await LogDataAsync($"Возникло исключение при отправле письма", Logger.From.Server, Logger.LogType.Error);
                    var exception = task.Exception;
                    await LogDataAsync(exception?.Message, Logger.From.Server, Logger.LogType.Error);
                    await LogDataAsync(exception?.InnerException.Message, Logger.From.Server, Logger.LogType.Error);
                    await LogDataAsync(exception?.StackTrace, Logger.From.Server, Logger.LogType.Error);
                }, TaskContinuationOptions.OnlyOnFaulted);

                while (true)
                {
                    var clientId = await server.WaitForConnectionAsync();
                    await LogDataAsync($"Подключился клиент с id: {clientId}", Logger.From.Server, Logger.LogType.Debug);
                    var processTask = ProcessClientAsync(server, clientId).ContinueWith(async task =>
                    {
                        await LogDataAsync($"Возникло исключения у клиента с id: {clientId}", Logger.From.Server, Logger.LogType.Error);
                        var exception = task.Exception;
                        await LogDataAsync(exception?.Message, Logger.From.Server, Logger.LogType.Error);
                        await LogDataAsync(exception?.InnerException.Message, Logger.From.Server, Logger.LogType.Error);
                        await LogDataAsync(exception?.StackTrace, Logger.From.Server, Logger.LogType.Error);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
            }
        }

        private static async Task ProcessClientAsync(ServerPipe server, int clientId)
        {
            while (server.IsConnected(clientId))
            {
                var message = await server.ReadAsync(clientId);
                try
                {
                    var (response, con) = await ProcessMessageAsync(clientId, message);
                    if (con)
                    {
                        await server.WriteAsync(response, clientId);
                    }
                    else
                    {
                        await server.WriteLastAsync(response, clientId);
                    }
                }
                catch (Exception e)
                {
                    await server.WriteLastAsync($"Возникла ошибка: {e.Message}", clientId);
                    throw e;
                }
            }
        }

        private static async Task PrepareAsync()
        {
            await LogDataAsync("Подготовка почты и действий для бана", Logger.From.Server, Logger.LogType.Debug);

            Commands.PrepareBanAction(serviceContainer, serviceFactory, settingFactory, actionSaver);

            var senderName = Environment.GetEnvironmentVariable("SenderName");
            var smtpUser = Environment.GetEnvironmentVariable("SMTPUser");
            var mailTo = Environment.GetEnvironmentVariable("MailTo");
            var mailRepeatTime = Environment.GetEnvironmentVariable("MailRepeatTime");

            if (senderName == null || smtpUser == null || mailTo == null || mailRepeatTime == null)
            {
                throw new Exception("Перед запуском нужно указать переменные окружения SMTPUser, MailTo SenderName и mailRepeatTime");
            }

            Commands.PrepareMail(serviceContainer, serviceFactory, settingFactory, senderName, smtpUser, mailTo, defaultSaver, actionSaver);
            Commands.PrepareFilters(Constants.FiltersListPath);
            Commands.PrepareWhiteList(serviceContainer, Constants.WhiteListPath, defaultSaver);
        }

        /// <summary>
        /// Читает приходящее сообщение и обрабатывает его
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <param name="result">Входящее сообщение</param>
        /// <returns>Кортеж из обработанного сообщения и логического значения, сообщающего продолжать ли серверу обработку этого клиента</returns>
        private static async Task<(string, bool)> ProcessMessageAsync(int id, string result)
        {
            await LogDataAsync($"Сообщение от клиента с id = {id}", Logger.From.Server, Logger.LogType.Debug);

            var (command, values) = ParseArgsAsync(result.Split(" ").ToList());
            await LogDataAsync($"Команда: {command}", Logger.From.Server, Logger.LogType.Debug);
            await LogDataAsync($"Значения: {string.Join(" ", values)}", Logger.From.Server, Logger.LogType.Debug);

            switch (command)
            {
                case "create":
                    await LogDataAsync("Запрошено интерактивное создание jail", Logger.From.Server, Logger.LogType.Debug);
                    var createShell = new Shell();
                    var createState = new PrepareCreateState(createShell, serviceContainer, serviceFactory,
                        settingFactory, jailSaver, filterSaver);
                    createShell.SetState(createState);
                    sessions.Add(id, createShell);
                    var createMessage = sessions[id].Get(values.ToArray());
                    return (createMessage, true);

                case "edit":
                    await LogDataAsync("Запрошено интерактивное изменение jail", Logger.From.Server, Logger.LogType.Debug);
                    var editShell = new Shell();
                    var editState = new PrepareEditState(editShell, serviceContainer, serviceFactory,
                        settingFactory, jailSaver, filterSaver);
                    editShell.SetState(editState);
                    sessions.Add(id, editShell);
                    var editMessage = sessions[id].Get(values.ToArray());
                    return (editMessage, true);
            }

            if (!sessions.ContainsKey(id))
            {
                sessions.Add(id, null);
                await LogDataAsync("Команда", Logger.From.Server, Logger.LogType.Debug);
                return (await CommandAsync(command, values, id), false);
            }

            if (result == "q")
            {
                await LogDataAsync("Клиент запросил отключение", Logger.From.Server, Logger.LogType.Debug);
                return ("Выход", false);
            }

            await LogDataAsync($"Клиент ввёл интерактивную команду \"{result}\"", Logger.From.Server, Logger.LogType.Debug);
            var message = sessions[id].Get(result.Split(" "));
            return Regex.IsMatch(message, $"{Constants.ShellTexts[Constants.ShellSteps.Exit]}") ? (message, false) : (message, true);
        }

        private static async Task MailSendProcessAsync()
        {
            var mailRepeatTime = Environment.GetEnvironmentVariable("MailRepeatTime");
            if (!int.TryParse(mailRepeatTime, out var time))
            {
                await LogDataAsync("Переменная окружения MailRepeatTime не задана или задана в неверном формате", Logger.From.Server, Logger.LogType.Error);
            }
            var currentSendTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            var nextSendTime = time + time * (currentSendTime / time);

            while (true)
            {
                currentSendTime = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

                if (currentSendTime >= nextSendTime)
                {
                    Commands.SendMail(Constants.MailLogPath);
                    nextSendTime = currentSendTime + time;

                    await LogDataAsync($"Попытка отправки письма", Logger.From.Server, Logger.LogType.Debug);
                }

                await Task.Delay(1000);
            }
        }

        private static async Task<string> CommandAsync(string command, IReadOnlyList<string> values, int id)
        {
            switch (command)
            {
                case "manage":
                    if (values.Count > 0)
                    {
                        await LogDataAsync("Запрошен вывод состояния всех jail", Logger.From.Server, Logger.LogType.Debug);
                        var builder = new StringBuilder();
                        foreach (var jail in values)
                        {
                            if (serviceContainer.GetJail(jail) == null)
                            {
                                return "Ошибка: Сервис не существует или не настроен";
                            }
                            var jailInfo = Commands.ManageJail(serviceContainer, jail);
                            var jailRules = PrintJailRules(jail, jailInfo, id);
                            builder.AppendLine(jailRules);
                        }

                        return builder.ToString();
                    }
                    else
                    {
                        await LogDataAsync("Запрошен вывод параметров jail", Logger.From.Server, Logger.LogType.Debug);
                        var jails = Commands.ManageJail(serviceContainer);
                        return PrintJailsStatusAsync(jails, id);
                    }

                case "enable":
                    await LogDataAsync("Запрошено включение jail", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 1)
                    {
                        Commands.ChangeJailRule(serviceContainer, values[0], "enabled", "true", jailSaver);
                        "fail2ban-client restart".Bash();
                        return "Сервис был включён";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }


                case "disable":
                    await LogDataAsync("Запрошено выключение jail", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 1)
                    {
                        Commands.ChangeJailRule(serviceContainer, values[0], "enabled", "false", jailSaver);
                        "fail2ban-client restart".Bash();
                        return "Сервис был выключен";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "stop":
                    await LogDataAsync("Запрошена остановка контейнера.", Logger.From.Server, Logger.LogType.Debug);
                    "fail2ban-client stop".Bash();
                    Environment.Exit(0);
                    return "Контейнер был остановлен";

                case "status":
                    await LogDataAsync("Запрошен вывод статуса забаненных адресов.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 0)
                    {
                        var builder = new StringBuilder();
                        var blackList = Commands.BlackListStatus(Constants.BlackListPath);
                        var blackAddresses = PrintAddressList(blackList, id, "Addresses in black list:");
                        builder.AppendLine(blackAddresses);
                        var bannedList = Commands.BannedListStatus(Constants.StatusLogPath);
                        var bannedAddresses = PrintAddressList(bannedList, id, "Banned addresses:");
                        builder.AppendLine(bannedAddresses);
                        return builder.ToString();
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "ban":
                    await LogDataAsync("Запрошен бан ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var bannedIp) &&
                        Commands.Ban(bannedIp, values[1], Constants.BlackListPath, Constants.WhiteListPath))
                    {
                        return "Ip адрес был забанен";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "unban":
                    // ip, service
                    await LogDataAsync("Запрошен разбан ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var unbannedIp) &&
                        Commands.Unban(unbannedIp, values[1], Constants.BlackListPath))
                    {
                        return "Ip адрес был разбанен";
                    }
                    else
                    {
                        return "Неверное количество аргументов";
                    }

                case "reban":
                    await LogDataAsync("Запрошен разбан всех ip и бан чёрного списка.", Logger.From.Server, Logger.LogType.Debug);
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
                    await LogDataAsync("Отправка письма.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 0 && Commands.SendMail(Constants.MailLogPath))
                    {
                        return "Письмо было отправлено";
                    }
                    else
                    {
                        return "Письмо не было отправлено";
                    }

                case "_logban":
                    // ip, service
                    await LogDataAsync("Запрошено логгирование бана ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var logBannedIp) &&
                        Commands.LogBan(logBannedIp, values[1], Constants.MailLogPath, Constants.StatusLogPath, Constants.WhiteListPath))
                    {
                        await LogDataAsync("Ip ban was logged", Logger.From.Server, Logger.LogType.Message);
                        return "Бан ip адреса был залогирован";
                    }
                    else
                    {
                        return "Бан ip адреса не был залогирован";
                    }

                case "_logunban":
                    // ip, service
                    await LogDataAsync("Запрошено логгирование разбана ip.", Logger.From.Server, Logger.LogType.Debug);
                    if (values.Count == 2 &&
                        IPAddress.TryParse(values[0], out var logUnbannedIp) &&
                        Commands.LogUnban(logUnbannedIp, values[1], Constants.MailLogPath, Constants.StatusLogPath, Constants.BlackListPath))
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



        private static string PrintJailsStatusAsync(Dictionary<string, bool> jails, int id)
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

        private static (string command, List<string> values) ParseArgsAsync(IReadOnlyList<string> args)
        {
            string command = null;

            if (args.Count < 1) return ("", null);

            var values = new List<string>();

            if (args.Count >= 2)
            {
                for (var i = 1; i < args.Count; i++)
                {
                    if (Constants.ValueRegex.IsMatch(args[i]))
                    {
                        values.Add(args[i]);
                    }
                }
            }

            if (Constants.CommandRegex.IsMatch(args[0]))
            {
                command = args[0];
            }

            return (command, values);

        }

        private static async Task LogDataAsync(string text, Logger.From @from, Logger.LogType type)
        {
            await logger.LogAsync(text, @from, type);
            Console.WriteLine(text);
        }
    }
}