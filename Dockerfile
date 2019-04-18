FROM vicubuntumailandcorebase
LABEL version="0.0.1"

COPY ["fail2ban.logrotate", "OnStart.sh", "/_Data/Scripts/"]

RUN apt install -y fail2ban && \
	mkdir /_Data/Confs /_Data/Confs/Jails /_Data/InLogs /_Data/OutLogs && \
	mkdir /var/run/fail2ban && \
	sed -i.bak -e 's,logtarget\s*=\s*/var/log/fail2ban.log,logtarget = /_Data/OutLogs/fail2ban.log,' /etc/fail2ban/fail2ban.conf && \
	echo $(cat /_Data/Scripts/fail2ban.logrotate) > /etc/logrotate.d/fail2ban && \
	rm /_Data/Scripts/fail2ban.logrotate && \
	rm -R /etc/fail2ban/jail.d && \
	ln -s /_Data/Confs/Jails/ /etc/fail2ban/jail.d && \
	ln -s /_Data/Confs/Filters/ /etc/fail2ban/filter.d && \
	#ln -s /_Data/Confs/Actions/* /etc/fail2ban/action.d && \
	mv /etc/fail2ban/action.d/iptables-allports.conf /etc/fail2ban/action.d/iptables-allports.conf.base && \
	mv /etc/fail2ban/action.d/iptables-multiport.conf /etc/fail2ban/action.d/iptables-multiport.conf.base

VOLUME ["/_Data/Confs", "/_Data/InLogs", "/_Data/OutLogs", "/_Data/Util"]

ENV SenderName = "" 

COPY ["Bin/", "/_Data/CLI/"]
COPY ["ftb", "/bin/"]

ENTRYPOINT ["/_Data/Scripts/OnStart.sh"]