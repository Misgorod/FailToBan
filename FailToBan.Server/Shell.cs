using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using FailToBan.Core;

namespace FailToBan.Server
{
    public class Shell : IDisposable
    {
        protected int clientId;
        protected ShellState state;
        protected IService currentFilter;
        protected IServiceFactory serviceFactory;
        protected IServiceSaver jailSaver;
        protected IServiceSaver filterSaver;
        protected Logger logger;

        protected string logPath = "";
        protected string regex = "";
        protected string currentName = "";

        public IService CurrentJail { get; }
        public IServiceContainer ServiceContainer { get; }

        public virtual string Get(string[] values)
        {
            return "";
        }

        protected void LogData(string message, Logger.LogType logType)
        {
            Console.WriteLine(message);
            logger.Log(message, Logger.From.Server, logType);
        }

        public void SetState(ShellState state)
        {
            this.state = state;
        }

        protected virtual bool TestSettings(out string result)
        {
            result = "";
            if (CurrentJail == null)
            {
                throw new VicException("Jail is null while testing");
            }

            var lastEnabled = CurrentJail.GetRule(CurrentJail.Name, RuleType.Enabled) ?? "false";
            try
            {
                CurrentJail.SetRule(CurrentJail.Name, RuleType.Enabled, "true");
                jailSaver.Save(CurrentJail);
                filterSaver.Save(currentFilter);

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
                CurrentJail.SetRule(CurrentJail.Name, RuleType.Enabled, lastEnabled);
                jailSaver.Delete(CurrentJail);
                filterSaver.Delete(currentFilter);
            }
        }

        protected bool TestFilter()
        {
            bool result = true;
            LogData("Начало работы тестирования фильтра", Logger.LogType.Debug);

            if (regex == "" || logPath == "")
            {
                throw new VicException("Фильтр или путь до логов ещё не выставлен");
            }

            var pattern = Regex.Replace(regex, @"(<HOST>)", @"(?:::f{4,6}:)?(?<host>\S+)");
            LogData($"Полученное регулярное выражение: {pattern}", Logger.LogType.Debug);

            using (var reader = new StreamReader(File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var line = "";
                while ((line = reader.ReadLine()) != null)
                {
                    var host = Regex.Match(line, pattern).Groups["host"].Value;
                    LogData($"Строка {line} проверяется регулярным выражением", Logger.LogType.Debug);
                    if (!IPAddress.TryParse(host, out IPAddress address))
                    {
                        LogData($"Строка {line} не прошла регулярное выражением", Logger.LogType.Debug);
                        result = false;
                        return result;
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            jailSaver.Delete(CurrentJail);
            jailSaver.Delete(currentFilter);
        }
    }
}