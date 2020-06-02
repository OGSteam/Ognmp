# Ognmp
Nginx MariaDB PHP Stack for OGSteam.
Run the application as Administrator to be able to start MariaDB

## Set Up Dev Environment section :

####Prerequisites :

###InnoSetup

https://jrsoftware.org/isdl.php
Current Version 6.0.4
Installer file is available at the Root folder : Onmp installer.iss

###VC_Redist for PHP 7
https://aka.ms/vs/16/release/VC_redist.x64.exe
To be put at same place as Onmp Installer.iss

###Maria DB
Download :
https://mariadb.com/downloads/
Select 64 bits version for Windows
To be installed in Maria DB Folder

###Nginx :
https://nginx.org/en/download.html
To be installed with it subdirectories in the root folder

###PHP :
Download Link - https://windows.php.net/download#php-7.4

Select version 64bits ThreadSafe : Example : https://windows.php.net/downloads/releases/php-7.4.5-nts-Win32-vc15-x64.zip
To be installed in PHP Folder under its version folder (7.3, 7.4, ...)

PHPMyadmin :
- Download from https://www.phpmyadmin.net
- Unzip it in the HTML Folder

###Build :

- Build the Visualstudio Solution
- Run Onmp Installer.iss. Output will be stored in compiledsetup Folder

Signature :

https://revolution.screenstepslive.com/s/revolution/m/10695/l/563371-signing-installers-you-create-with-inno-setup
