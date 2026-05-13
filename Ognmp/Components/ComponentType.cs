using System;

namespace Ognmp.Components
{
    /// <summary>
    /// Defines the types of components that can be managed by the Dependency Manager.
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// Nginx web server
        /// </summary>
        Nginx,

        /// <summary>
        /// PHP runtime
        /// </summary>
        PHP,

        /// <summary>
        /// MariaDB database server
        /// </summary>
        MariaDB,

        /// <summary>
        /// phpMyAdmin web interface
        /// </summary>
        PhpMyAdmin,

        /// <summary>
        /// PHP Xdebug extension
        /// </summary>
        PhpXdebug
    }

    /// <summary>
    /// Extension methods for ComponentType enum.
    /// </summary>
    public static class ComponentTypeExtensions
    {
        /// <summary>
        /// Converts ComponentType to the legacy string representation.
        /// </summary>
        public static string ToComponentName(this ComponentType type)
        {
            return type switch
            {
                ComponentType.Nginx => "Nginx",
                ComponentType.PHP => "PHP",
                ComponentType.MariaDB => "MariaDB",
                ComponentType.PhpMyAdmin => "PhpMyAdmin",
                ComponentType.PhpXdebug => "PHP Xdebug",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        /// <summary>
        /// Parses a component name string to ComponentType.
        /// </summary>
        public static ComponentType FromComponentName(string componentName)
        {
            return componentName switch
            {
                "Nginx" => ComponentType.Nginx,
                "PHP" => ComponentType.PHP,
                "MariaDB" => ComponentType.MariaDB,
                "PhpMyAdmin" => ComponentType.PhpMyAdmin,
                "PHP Xdebug" => ComponentType.PhpXdebug,
                _ => throw new ArgumentException($"Unknown component name: {componentName}", nameof(componentName))
            };
        }
    }
}
