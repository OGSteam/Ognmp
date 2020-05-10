# Ognmp
Nginx MariaDB PHP Stack for OGSteam

## Installer section :

####Prerequisites :

###InnoSetup

https://jrsoftware.org/isdl.php
Current Version 6.0.4

###VC_Redist for PHP
https://aka.ms/vs/16/release/VC_redist.x64.exe

To be put at same place as Onmp Installer.iss

###Maria DB
Download :
https://mariadb.com/downloads/
Select 64 bits version for Windows
To be installed in Maria DB Folder

###Nginx :
https://nginx.org/en/download.html

To be installed in the root folder

###PHP :
https://windows.php.net/download#php-7.4

Select version 64bits ThreadSafe : Example : https://windows.php.net/downloads/releases/php-7.4.5-nts-Win32-vc15-x64.zip
To be installed in PHP Folder under its version folder (7.3, 7.4, ...)


###Build :

Run Onmp Installer.iss. Output will be stored in compiledsetup Folder

Signature :

https://revolution.screenstepslive.com/s/revolution/m/10695/l/563371-signing-installers-you-create-with-inno-setup
