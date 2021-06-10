if "%time:~0,2%" lss "10" (set hh=0%time:~1,1%) else (set hh=%time:~0,2%)
set "file=%date:~,4%%date:~5,2%%date:~8,2%%hh%%time:~3,2%%time:~6,2%"
osql -S 127.0.0.1,44333 -U sa -P i1Cy6lquNT5IhC5T -d master -Q"BACKUP DATABASE [BetWin] to disk='D:\Backup\BetWin_%file%.bak'"
rar a "%file%.rar" -df -m5 -pBetWin@%file% *.bak
ncftpput -u backup-yongli1688 -p yongli1688.com 175.41.21.106 \ "%file%.rar"
del %file%.rar
