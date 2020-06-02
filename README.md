# Ognmp

Nginx MariaDB PHP Stack for OGSteam.
Run the application as Administrator to be able to start MariaDB

## Usage

- Run installer
- Put your files into the html folder
- Run Ognmp as ADministrator
- Start All Services
- Run in your favorite Browser <https://localhost/yourwebsitefolder>

## Configuration

You can configure your Settings in Menu -> File ->  Options :

- PHP Settings as Version and Modules
- Windows Behaviors (Startup Minimized, Text Editor, ...)

## Set Up Dev Environment section

### Prerequisites

#### InnoSetup

- Download :<https://jrsoftware.org/isdl.php>
- Installer file is available at the Root folder : Onmp installer.iss

#### VC_Redist for PHP 7

- Download : https://aka.ms/vs/16/release/VC_redist.x64.exe
- To be put at same place as Onmp Installer.iss

#### Maria DB

- Download : <https://mariadb.com/downloads/> (Select 64 bits version for Windows)
- To be installed in Maria DB Folder

#### Nginx

- Download : https://nginx.org/en/download.html
- install it all subdirectories in the root folder

#### PHP

- Download : <https://windows.php.net/download#php-7.4> (Select version 64bits ThreadSafe : Example : <https://windows.php.net/downloads/releases/php-7.4.5-nts-Win32-vc15-x64.zip>)
- To be installed in PHP Folder under its version folder (7.3, 7.4, ...)
- Align Config file if necessary

#### PHPMyadmin

- Download : <https://www.phpmyadmin.net>
- Unzip it in the HTML Folder
- Align Config file if necessary

### Build

- Build the Visualstudio Solution
- Run Onmp Installer.iss. Output will be stored in compiledsetup Folder

## LICENSE

This application is based on Wnmp <https://github.com/wnmp/wnmp> and is under GPLV3 License
