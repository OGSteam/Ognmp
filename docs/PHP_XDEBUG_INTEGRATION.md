# PHP Xdebug Integration

## Overview

Le Dependency Manager d'Ognmp inclut maintenant le support pour télécharger et installer automatiquement l'extension PHP Xdebug.

## Fonctionnalités

### Installation Automatique
- Télécharge la DLL Xdebug appropriée pour votre version de PHP
- Place la DLL dans le dossier `ext` de PHP
- Configure automatiquement `php.ini` avec les paramètres Xdebug recommandés

### Versions Disponibles

Xdebug est disponible pour toutes les versions majeures de PHP:
- **PHP 8.3** - Xdebug 3.3.x
- **PHP 8.2** - Xdebug 3.3.x
- **PHP 8.1** - Xdebug 3.3.x

## Installation via Dependency Manager

### Étapes

1. **Ouvrir le Dependency Manager**
   - Menu: **Tools** ? **Download Components...**

2. **Sélectionner PHP Xdebug**
   - Cliquez sur "PHP Xdebug" dans la liste

3. **Choisir la Version**
   - Sélectionnez la version Xdebug correspondant à votre PHP
   - Exemple: "Xdebug 3.3.1 for PHP 8.3"

4. **Installer**
   - Cliquez sur "Install Selected"
   - Attendez la fin du téléchargement

## Configuration Automatique

Lors de l'installation, le système ajoute automatiquement à `php.ini`:

```ini
; Xdebug Configuration
zend_extension=php_xdebug-3.3.1-8.3-vs16-x86_64.dll
xdebug.mode=debug
xdebug.start_with_request=yes
xdebug.client_port=9003
xdebug.client_host=127.0.0.1
```

## Chemins d'Installation

Les DLL Xdebug sont installées dans:
- **PHP 8.3**: `Ognmp\php\8.3\ext\php_xdebug-*.dll`
- **PHP 8.2**: `Ognmp\php\8.2\ext\php_xdebug-*.dll`
- **PHP 8.1**: `Ognmp\php\8.1\ext\php_xdebug-*.dll`

## Paramètres Xdebug par Défaut

### Mode de Débogage
```ini
xdebug.mode=debug
```
Active le mode débogueur (step debugging)

### Démarrage Automatique
```ini
xdebug.start_with_request=yes
```
Démarre Xdebug automatiquement avec chaque requête

### Port de Communication
```ini
xdebug.client_port=9003
```
Port utilisé pour la communication avec l'IDE (standard Xdebug 3.x)

### Hôte Client
```ini
xdebug.client_host=127.0.0.1
```
Adresse IP de l'IDE (localhost par défaut)

## Vérification de l'Installation

### 1. Via PHP CLI
```bash
php -v
```
Devrait afficher:
```
with Xdebug v3.3.1, Copyright (c) 2002-2024, by Derick Rethans
```

### 2. Via phpinfo()
Créez un fichier `info.php`:
```php
<?php
phpinfo();
?>
```
Recherchez la section "Xdebug" dans la sortie.

### 3. Via Logs
Vérifiez les logs Ognmp pour confirmer:
```
PHP Xdebug installed successfully
Xdebug configured in php.ini for PHP 8.3
```

## Configuration Personnalisée

Pour personnaliser Xdebug après l'installation, éditez `php.ini`:

### Profiling
```ini
xdebug.mode=debug,profile
xdebug.output_dir="C:/source/Ognmp/php/8.3/logs"
```

### Couverture de Code
```ini
xdebug.mode=coverage
```

### Trace
```ini
xdebug.mode=trace
xdebug.trace_output_dir="C:/source/Ognmp/php/8.3/logs"
```

### Multiple Modes
```ini
xdebug.mode=debug,develop,trace
```

## Utilisation avec un IDE

### Visual Studio Code

1. **Installer l'extension PHP Debug**
   - Marketplace: `PHP Debug` par Xdebug

2. **Configurer `.vscode/launch.json`**:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Listen for Xdebug",
            "type": "php",
            "request": "launch",
            "port": 9003,
            "pathMappings": {
                "/var/www/html": "${workspaceFolder}"
            }
        }
    ]
}
```

### PhpStorm

1. **Settings** ? **PHP** ? **Debug**
2. Configurer:
   - **Xdebug port**: 9003
   - **Can accept external connections**: Oui

3. **Run** ? **Start Listening for PHP Debug Connections**

## Dépannage

### Xdebug ne se charge pas

**Vérifier php.ini**:
```bash
php --ini
```
Vérifiez que le bon fichier est chargé.

**Vérifier le chemin DLL**:
```ini
zend_extension=php_xdebug-3.3.1-8.3-vs16-x86_64.dll
```
Peut nécessiter le chemin complet:
```ini
zend_extension=C:/source/Ognmp/php/8.3/ext/php_xdebug-3.3.1-8.3-vs16-x86_64.dll
```

### Le débogueur ne se connecte pas

**Vérifier le firewall**:
- Autoriser les connexions sur le port 9003

**Vérifier la configuration**:
```bash
php -i | grep xdebug
```

### Erreur "Cannot find or open Xdebug"

**Solution**:
1. Vérifiez que la DLL existe dans `php/x.x/ext/`
2. Réinstallez via Dependency Manager
3. Vérifiez les permissions du fichier

## Désinstallation

Pour désactiver Xdebug:

1. **Commenter dans php.ini**:
```ini
; zend_extension=php_xdebug-3.3.1-8.3-vs16-x86_64.dll
```

2. **Redémarrer PHP**

3. **Supprimer la DLL** (optionnel):
   - Supprimer le fichier de `php/x.x/ext/`

## Ressources

- **Documentation Xdebug**: https://xdebug.org/docs/
- **Configuration Xdebug 3**: https://xdebug.org/docs/upgrade_guide
- **Wizard Xdebug**: https://xdebug.org/wizard

## Notes Importantes

1. **Performance**: Xdebug peut ralentir PHP, désactivez-le en production
2. **Versions**: Assurez-vous que la version Xdebug correspond à votre version PHP
3. **Architecture**: Utilisez toujours la version x64 (x86_64)
4. **Visual Studio**: Utilisez la version vs16 pour compatibilité
