﻿// Copyright (C) Sina Iravanian, Julian Verdurmen, axuno gGmbH and other contributors.
// Licensed under the MIT license.

using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NETSTANDARD

#endif

namespace YAXLib
{
#if NETSTANDARD
    /// <summary>
    ///     Implementation of AppDomain for .NetCore
    /// </summary>
    public class AppDomain
    {
        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        /*
        MIT License

        Copyright (C) 2017 by axuno gGmbH (https://github.com/axuno and http://www.axuno.net)

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
        */
        internal static AppDomain CurrentDomain { get; }

        /// <summary>
        ///     Get all assemblies
        /// </summary>
        /// <returns></returns>
        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();

            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            var assemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId);

            foreach (var assemblyName in assemblyNames)
            {
                var assembly = Assembly.Load(assemblyName);
                assemblies.Add(assembly);
            }

            return assemblies.ToArray();
        }
    }
#endif

    /// <summary>
    ///     Provides extensions as a bridge for the differences
    ///     between .Net Framework "Type" and .Net Core "TypeInfo".
    /// </summary>
    internal static class ReflectionBridgeExtensions
    {
        /*
        MIT License

        Copyright (c) 2016 to 2099 Stef Heyenrath
        Sourcecode: https://github.com/StefH/ReflectionBridge

        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.

        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
        */

        /*
        Modified by axuno gGmbH (https://github.com/axuno and http://www.axuno.net) for YAXLib
        */
        public static Assembly GetAssembly(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.IsGenericTypeDefinition;
        }

        public static bool IsInterface(this Type type)
        {
            return type.IsInterface;
        }

        public static Type BaseType(this Type type)
        {
            return type.BaseType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.IsValueType;
        }

        public static PropertyInfo GetProperty(this Type type, string name, Type[] types)
        {
            return type.GetProperty(name, types);
        }

        public static object InvokeMethod<T>(this Type type, string methodName, object target, T value)
        {
            return type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, target, new object[] {value});
        }

        public static object InvokeMethod(this Type type, string methodName, object target, object[] arg)
        {
#if NETSTANDARD
            Type[] argTypes = null;
            if (arg != null)
            {
                argTypes = new Type[arg.Length];
                for (var i = 0; i < argTypes.Length; i++) argTypes[i] = arg[i]?.GetType();
            }
            else
            {
                argTypes = Type.EmptyTypes;
            }

            if (argTypes.All(t => t != null))
            {
                var method = type.GetTypeInfo().GetMethod(methodName, argTypes);
                return method.Invoke(target, arg);
            }

            // one of the arguments is null
            var potentialMethods = type.GetTypeInfo().GetMethods().Where(m => m.Name == methodName).ToArray();
            var ex = new Exception(nameof(InvokeMethod));
            //TODO: FXCORE Trial and error for method invocation is not the most elegant way...
            foreach (var method in potentialMethods)
                try
                {
                    return method.Invoke(target, arg);
                }
                catch (Exception e)
                {
                    ex = e;
                }

            throw new TargetInvocationException(ex);
#else
            return type.InvokeMember(methodName, BindingFlags.InvokeMethod, null, target, arg);
#endif
        }

#if NETSTANDARD
        public static Attribute[] GetCustomAttributes(this Type type, bool inherit)
        {
            return (Attribute[]) type.GetTypeInfo().GetCustomAttributes(inherit).ToArray();
        }
#endif
    }
}