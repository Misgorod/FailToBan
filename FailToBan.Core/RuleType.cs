namespace FailToBan.Core
{
    public enum RuleType
    {
        /// <summary>
        /// Включает сервисы
        /// enabled = true
        /// </summary>
        Enabled,
        /// <summary>
        /// Время в секундах, на которое банится хост
        /// bantime = 10m
        /// </summary>
        Bantime,
        After,
        Before,
        /// <summary>
        /// Позволяет использовать базы данных для поиска уже забаненных ip-адресов для увеличения стандартного времени бана, используя специальную формулу
        /// Формула по умолчанию: bantime * 1, 2, 4, 8, 16, 32...
        /// bantime.increment = true
        /// </summary>
        BantimeIncrement,
        /// <summary>
        /// Максимальное количество секунд, которое используется для испоьзования со случайным временем, чтобы обойти "умных" ботнетов, высчитавающих точное время, на которое IP будет снова разбанено
        /// bantime.rndtime = 10
        /// </summary>
        BantimeRndtime,
        /// <summary>
        /// Максимальное количество секунд, на которое банится IP
        /// bantime.maxtime = 500
        /// </summary>
        BantimeMaxtime,
        /// <summary>
        /// Коэффициент для расчёта экспоненциального роста в формуле или обычном множителе
        /// Стандартное значение - это 1 и для него время бана растёт на 1, 2, 4, 8, 16, 32...
        /// bantime.factor = 1
        /// </summary>
        BantimeFactor,
        /// <summary>
        /// Используется по умолчанию для рассчёта следующего значения для времени бана
        /// Формула по умолчанию: ban.Time * (1 << (ban.Count if ban.Count < 20 else 20)) * banFactor
        /// bantime.formula = ban.Time * math.exp(float(ban.Count+1)*banFactor)/math.exp(1*banFactor)
        /// </summary>
        BantimeFormula,
        /// <summary>
        /// Используется для рассчёта следущего значения времени бана вместо формулы, соответствуя предыдущему времени бана и заданному "bantime.factor"
        /// bantime.multipliers = 1 5 30 60 300 720 1440 2880
        /// </summary>
        BantimeMultipliers,
        /// <summary>
        /// Если true, выполняет поиск в базе Ip-адресов по всем сервисам
        /// Если false, выполняет поиск в базе Ip-адресов по текущему сервису
        /// bantime.overalljails = false
        /// </summary>
        BantimeOveralljails,
        /// <summary>
        /// Устанвливает, должен ли локальный Ip-адрес быть забанен
        /// По умолчанию true
        /// ignoreself = true
        /// </summary>
        Ignoreself,
        /// <summary>
        /// Список Ip-адресов, масок или DNS хостов
        /// Fail2Ban не будет банить хост, который находится в этом списке
        /// Несколько адресов отделяются пробелом или запятой
        /// ignoreip = 127.0.0.1/8 ::1
        /// </summary>
        Ignoreip,
        /// <summary>
        /// Внешняя команда, которая берёт Ip-адрес в теге и возвращает true, если он игнорируется. False в противном случае
        /// ignorecommand = /path/to/command <ip>
        /// </summary>
        Ignorecommand,
        /// <summary>
        /// Хост банится если неудачные попытки повторяются в течение последних "findtime" секунд
        /// findtime  = 10m
        /// </summary>
        Findtime,
        /// <summary>
        /// Количество неудачных попыток перед баном
        /// maxretry = 5
        /// </summary>
        Maxretry,
        /// <summary>
        /// Определяет как fail2ban будет контролировать логи
        /// Доступные значения: "pyinotify", "gamin", "polling", "systemd" and "auto"
        /// backend = auto
        /// </summary>
        Backend,
        /// <summary>
        /// Определяет, используется ли в блокировке обратный DNS 
        /// Доступные значения: "yes", "no", "warn", "raw"
        /// no - fail2ban будет блокировать IP-адреса вместо имен хостов 
        /// warn - попытается использовать обратный DNS для поиска имени хоста и его блокировки, но будет регистрировать активность в логе.
        /// usedns = warn
        /// </summary>
        Usedns,
        /// <summary>
        /// Определяет кодировку логов
        /// Например: "ascii", "utf-8"
        /// logencoding = auto
        /// </summary>
        Logencoding,
        /// <summary>
        /// Определяет режим фильтрации
        /// mode = normal
        /// </summary>
        Mode,
        /// <summary>
        /// Путь до логов
        /// </summary>
        Logpath,
        /// <summary>
        /// Опередляет фильтр, используемый в сервисе
        /// По умолчанию используется фильтр с названием как у сервиса
        /// filter = %(__name__)s[mode=%(mode)s]
        /// </summary>
        Filter,
        /// <summary>
        /// Это адрес, на который будет отправлено уведомление (если fail2ban поддерживает оповещения по почте)
        /// destemail = root@localhost
        /// </summary>
        Destemail,
        /// <summary>
        /// Это имя будет использоваться при отправке генерируемых уведомлений
        /// sender = root@<fq-hostname>
        /// </summary>
        Sender,
        /// <summary>
        /// Агент передачи почты, который будет использоваться для отправки уведомлений
        /// mta = sendmail
        /// </summary>
        Mta,
        /// <summary>
        /// Стандартный протокол
        /// protocol = tcp
        /// </summary>
        Protocol,
        /// <summary>
        /// Это цепочка, которая будет настроена для отправки трафика в последовательность fail2ban
        /// chain = <known/chain>
        /// </summary>
        Chain,
        /// <summary>
        /// Порты, которые банятся
        /// port = 0:65535
        /// </summary>
        Port,
        Tcpport,
        Udpport,
        // TODO: Добавить комментарий к действию
        Fail2BanAgent,
        /// <summary>
        /// Устанавливает действие, которое будет использоваться при достижении порогового значения maxretry.
        /// banaction = iptables-multiport
        /// </summary>
        Banaction,
        /// <example>
        /// banaction_allports = iptables-allports
        /// </example>
        BanactionAllports,
        /// <summary>
        /// Только блокировка
        /// action_ = %(banaction)s[name=%(__name__)s, port="%(port)s", protocol="%(protocol)s", chain="%(chain)s"]
        /// </summary>
        Action_,
        /// <summary>
        /// Бан и отпрвка письма с Ip-адресом на destmail
        /// action_mw = %(banaction)s[name=%(__name__)s, port="%(port)s", protocol="%(protocol)s", chain="%(chain)s"]
        ///             %(mta) s-whois[name =% (__name__)s, sender = "%(sender)s", dest = "%(destemail)s", protocol = "%(protocol)s", chain = "%(chain)s"]
        /// </summary>
        ActionMw,
        /// <summary>
        /// Бан, отправка письма с Ip-адресом и строками из лог-файлов на destmail
        /// action_mwl = %(banaction)s[name=%(__name__)s, port="%(port)s", protocol="%(protocol)s", chain="%(chain)s"]
        ///              %(mta) s-whois-lines[name =% (__name__)s, sender = "%(sender)s", dest = "%(destemail)s", logpath =% (logpath)s, chain = "%(chain)s"]
        /// </summary>
        ActionMwl,
        /// <summary>
        /// Бан, отправка xarf письма на destmail
        /// action_xarf = %(banaction)s[name=%(__name__)s, port="%(port)s", protocol="%(protocol)s", chain="%(chain)s"]
        ///               xarf-login-attack[service=%(__name__) s, sender = "%(sender)s", logpath =% (logpath)s, port = "%(port)s"]
        /// </summary>
        ActionXarf,
        /// <summary>
        /// Бан Ip-адреса на CloufFlare и отправка письма на sendmail с строками лога и Ip-адресом
        /// action_cf_mwl = cloudflare[cfuser="%(cfemail)s", cftoken="%(cfapikey)s"]
        ///                 %(mta) s-whois-lines[name =% (__name__)s, sender = "%(sender)s", dest = "%(destemail)s", logpath =% (logpath)s, chain = "%(chain)s"]
        /// </summary>
        ActionCfMwl,
        // TODO: Добавить комментарий к действию
        ActionBlocklistDe,
        /// <summary>
        /// Бан с помощью badips.com и использует как чёрный список
        /// Должен быть последним правилом в сервисе
        /// action_badips = badips.py[category="%(__name__)s", banaction="%(banaction)s", agent="%(fail2ban_agent)s"]
        /// </summary>
        ActionBadips,
        /// <summary>
        /// Бан с помощью badips.com
        /// action_badips_report = badips[category="%(__name__)s", agent="%(fail2ban_agent)s"]
        /// </summary>
        ActionBadipsReport,
        /// <summary>
        /// Бан с помощью abuseopdb.com
        /// action_abuseipdb = abuseipdb
        /// </summary>
        ActionAbuseipdb,
        /// <summary>
        /// Стандартное действие
        /// Для изменения переопределите значение интерполяцией выбранного действия
        /// action = %(action_)s
        /// </summary>
        Action,
        Actionstart,
        Actionstop,
        Actioncheck,
        Actionban,
        Actionunban,
        ActionVic,
        KnockingUrl,
        Blocktype,
        Returntype,
        Norestored,
        Name,
        Failregex,
        Null
    }
}