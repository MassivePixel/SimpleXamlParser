// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleXamlParser.Interfaces;
using Xamarin.Forms;

namespace SimpleXamlParser
{

    public static class ReflectionHelpers
    {
        static IPlatformServices platformServices;
        public static IPlatformServices PlatformServices
        {
            get { return platformServices; }
            set
            {
                platformServices = value;
                Init();
            }
        }

        public static IReadOnlyDictionary<string, Type> AllTypes { get; private set; }
        public static IReadOnlyDictionary<string, Type> VisualElementTypes { get; private set; }

        public static Dictionary<Type, TypeConverter> TypeConverters { get; private set; }
        public static Dictionary<Type, PropertyInfo> ContentProperties { get; private set; }

        static void Init()
        {
            try
            {
                ContentProperties = new Dictionary<Type, PropertyInfo>();
                TypeConverters = new Dictionary<Type, TypeConverter>();

                var allTypes = new Dictionary<string, Type>();
                var types = new Dictionary<string, Type>();

                foreach (var type in platformServices.GetAssemblies()
                                                     .SelectMany(a => a.DefinedTypes))
                {
                    if (type.IsPublic && !allTypes.ContainsKey(type.Name))
                    {
                        allTypes.Add(type.Name, type.AsType());
                    }

                    if (typeof(VisualElement).GetTypeInfo().IsAssignableFrom(type) &&
                       !type.IsAbstract)
                    {
                        if (!types.ContainsKey(type.Name))
                            types.Add(type.Name, type.AsType());
                    }
                }

                AllTypes = allTypes;
                VisualElementTypes = types;

                foreach (var type in allTypes.Values)
                {
                    var tc = type.GetTypeInfo().GetCustomAttribute<TypeConverterAttribute>();
                    if (tc != null)
                    {
                        AddTypeConverter(type, tc.ConverterTypeName);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static Type GetType(string name)
        {
            Type type;
            if (VisualElementTypes.TryGetValue(name, out type))
                return type;

            return null;
        }

        public static Type GetAllType(string name)
        {
            Type type;
            if (AllTypes.TryGetValue(name, out type))
                return type;

            return null;
        }

        public static PropertyInfo GetProperty(Type owner, string propertyName)
        {
            return owner.GetRuntimeProperty(propertyName);
        }

        public static void AddTypeConverter(Type propertyType, string converterTypeName)
        {
            if (TypeConverters.ContainsKey(propertyType))
                return;

            if (converterTypeName.Contains(","))
                converterTypeName = converterTypeName.Split(new[] { ',' })[0].Trim();

            var type = AllTypes.Values.FirstOrDefault(v => v.FullName == converterTypeName);
            if (type != null)
                TypeConverters.Add(propertyType, Activator.CreateInstance(type) as TypeConverter);
        }

        public static TypeConverter GetTypeConverter(Type propertyType)
        {
            TypeConverter typeConverter;
            if (TypeConverters.TryGetValue(propertyType, out typeConverter))
                return typeConverter;
            return null;
        }

        public static PropertyInfo GetContentProperty(Type type)
        {
            if (type == null)
                return null;

            PropertyInfo prop;
            if (ContentProperties.TryGetValue(type, out prop))
                return prop;

            var typeInfo = type.GetTypeInfo();

            ContentPropertyAttribute contentAttribute = null;
            while (contentAttribute == null && typeInfo != null)
            {
                contentAttribute = typeInfo.GetCustomAttribute<ContentPropertyAttribute>();
                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }

            if (contentAttribute != null)
            {
                var props = type.GetRuntimeProperties()
                                .Where(p => p.Name == contentAttribute.Name)
                                .ToList();
                prop = props.FirstOrDefault();

                if (prop != null)
                    ContentProperties.Add(type, prop);
            }

            return prop;
        }
    }
}
