// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Avalonia.Labs.Controls
{
    /// <summary>
    ///   This class is a helper class for the <see cref="MultiSelectionComboBox"/>. 
    ///   It uses the <see cref="TypeConverter"/> for the elements <see cref="Type"/>. If you need more control
    ///   over the conversion you should create your own class which implements <see cref="IParseStringToObject"/>
    /// </summary>
    public class DefaultStringToObjectParser : IParseStringToObject
    {
        public static readonly DefaultStringToObjectParser Instance = new();

        /// <inheritdoc />
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "TypeDescriptor is used intentionally; users of DefaultStringToObjectParser accept this trade-off.")]
        public bool TryCreateObjectFromString(string? input,
                                              out object? result,
                                              CultureInfo? culture = null,
                                              string? stringFormat = null,
                                              [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type? targetType = null)
        {
            try
            {
                // If the input is null the result is also null
                if (input is null)
                {
                    result = null;
                    return true;
                }

                // If we don't know the target type we cannot convert in this class.
                // Either provide the target type or roll your own class implementing IParseStringToObject
                if (targetType is null)
                {
                    result = null;
                    return false;
                }

                result = TypeDescriptor.GetConverter(targetType).ConvertFromString(default!, culture ?? CultureInfo.InvariantCulture, input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the elements <see cref="Type"/> for a given <see cref="IEnumerable"/>
        /// </summary>
        /// <param name="list">Any collection of elements</param>
        /// <returns>the elements <see cref="Type"/></returns>
        [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Reflection is used intentionally to detect element type; callers accept the trimming trade-off.")]
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Reflection is used intentionally to detect element type; callers accept the trimming trade-off.")]
        public Type? GetElementType(IEnumerable? list)
        {
            if (list is null)
            {
                return null;
            }

            var listType = list.GetType();

            if (listType.IsGenericType)
                return listType.GetGenericArguments().FirstOrDefault();

            if (listType.IsArray)
                return listType.GetElementType();

            // Fallback: inspect implemented interfaces for IEnumerable<T> to handle
            // non-generic wrapper types (e.g., custom collections that are not themselves generic).
            foreach (var iface in listType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    return iface.GetGenericArguments()[0];
            }

            return null;
        }
    }
}
