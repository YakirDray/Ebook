BACKUP DATABASE MyEBookLibrary
TO DISK = 'C:\MyEBookLibrary_backup.bak'
WITH FORMAT, 
    MEDIANAME = 'SQLServerBackups',
    NAME = 'Full Backup of MyEBookLibrary';