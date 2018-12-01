/*
DKW.NMEA
Copyright (C) 2018 Doug Wilson

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Demo
{
    [DebuggerStepThrough]
    internal static class RH
    {
        private static readonly IDictionary<String, String> Cache = new Dictionary<String, String>();

        /// <summary>
        /// Looks in the assembly and namespace that contains <see cref="RH"/> for a resource named <paramref name="resource"/> and returns it as a <seealso cref="String"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <exception cref="ApplicationException">Assembly could not be loaded or <paramref name="resource"/> was not found.</exception>
        public static String GS(String resource)
        {
            var type = typeof(RH);
            return GS(resource, type);
        }

        /// <summary>
        /// Looks in the assembly and namespace that contains <typeparamref name="T"/> for a resource named <paramref name="resource"/> and returns it as a <seealso cref="String"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <exception cref="ApplicationException">Assembly could not be loaded or <paramref name="resource"/> was not found.</exception>
        public static String GS<T>(String resource)
        {
            var type = typeof(T);
            return GS(resource, type);
        }

        private static String GS(String resource, Type type)
        {
            var cachekey = type.FullName + "::" + resource;
            if (!Cache.ContainsKey(cachekey)) {
                Debug.WriteLine("Cache Miss: " + cachekey, "Resource Helper");
                try {
                    var assembly = Assembly.GetAssembly(type);
                    if (assembly == null) {
                        throw new ApplicationException("Could not load the assembly.");
                    }

                    var key = type.Namespace + "." + resource;

                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var reader = new StreamReader(assembly.GetManifestResourceStream(key))) {
                        Cache[cachekey] = reader.ReadToEnd();
                    }
                }
                catch (Exception ex) {
                    throw new ApplicationException("ResourceHelper.GetString('" + resource + "') failed to load the resource code from " + type.AssemblyQualifiedName, ex);
                }
            }

            return Cache[cachekey];
        }

        /// <summary>
        /// Looks in the assembly and namespace that contains <see cref="RH"/> for a resource named <paramref name="resource"/> and returns it as a <seealso cref="Stream"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <exception cref="ApplicationException">Assembly could not be loaded or <paramref name="resource"/> was not found.</exception>
        public static Stream GetResourceStream(String resourceCode)
        {
            var type = typeof(RH);
            return GetResourceStream(type, resourceCode);
        }

        /// <summary>
        /// Looks in the assembly and namespace that contains <typeparamref name="T"/> for a resource named <paramref name="resource"/> and returns it as a <seealso cref="Stream"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <exception cref="ApplicationException">Assembly could not be loaded or <paramref name="resource"/> was not found.</exception>
        public static Stream GetResourceStream<T>(String resourceCode)
        {
            var type = typeof(T);
            return GetResourceStream(type, resourceCode);
        }

        private static Stream GetResourceStream(Type type, String resourceCode)
        {
            var assembly = Assembly.GetAssembly(type);

            if (assembly == null)
            {
                throw new ApplicationException("Could not load the assembly.");
            }

            var key = type.Namespace + "." + resourceCode;

            return assembly.GetManifestResourceStream(key);
        }
    }
}