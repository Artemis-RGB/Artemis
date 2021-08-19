using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using Artemis.Core;
using Artemis.Core.Services;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides UI-oriented utilities for types
    /// </summary>
    public static class TypeUtilities
    {
        internal static INodeService? NodeService;

        /// <summary>
        ///     Creates a tuple containing a color and a slightly darkened color for a given type
        /// </summary>
        /// <param name="type">The type to create the color for</param>
        public static (Color, Color) GetTypeColors(Type type)
        {
            if (type == typeof(object))
                return (SKColors.White.ToColor(), SKColors.White.Darken(0.35f).ToColor());

            TypeColorRegistration? typeColorRegistration = NodeService?.GetTypeColor(type);
            if (typeColorRegistration != null)
                return (typeColorRegistration.Color.ToColor(), typeColorRegistration.DarkenedColor.ToColor());

            // Come up with a random color based on the type name that should be the same each time
            MD5 md5Hasher = MD5.Create();
            byte[] hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(type.FullName!));
            int hash = BitConverter.ToInt32(hashed, 0);
            
            SKColor baseColor = SKColor.FromHsl(hash % 255, 50 + hash % 50, 50);
            SKColor darkenedColor = baseColor.Darken(0.35f);

            return (baseColor.ToColor(), darkenedColor.ToColor());
        }
    }
}