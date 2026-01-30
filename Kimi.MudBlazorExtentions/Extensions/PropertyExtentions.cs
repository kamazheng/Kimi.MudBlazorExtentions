// ***********************************************************************
// Author           : MOLEX\kzheng
// Created          : 01/17/2025
// ***********************************************************************

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;

namespace Kimi.MudBlazorExtentions.Extensions
{
    /// <summary>
    /// Defines extension methods for working with properties.
    /// </summary>
    public static class PropertyExtentions
    {
        #region Methods

        /// <summary>
        /// Excludes the properties that belong to the specified interface from the given collection of properties.
        /// </summary>
        /// <typeparam name="Interface">The interface type.</typeparam>
        /// <param name="properties">The collection of properties.</param>
        /// <returns>The filtered collection of properties.</returns>
        public static IEnumerable<PropertyInfo> ExculdeInterfaceProperty<Interface>(this IEnumerable<PropertyInfo> properties)
        {
            var interfaceProperpties = (typeof(Interface)).GetProperties();
            return properties.Where(p => interfaceProperpties.All(i => i.Name != p.Name)).ToArray();
        }

        /// <summary>
        /// Gets the specified attribute, including attributes defined in metadata classes.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <returns>The attribute, or null if not found.</returns>
        public static Attribute? GetAttributeIncludingMetadata(this MemberInfo propertyInfo, Type attributeType)
        {
            // First, check for the attribute on the property itself
            if (propertyInfo == null || attributeType == null)
            {
                return null;
            }
            var attribute = Attribute.GetCustomAttribute(propertyInfo, attributeType);
            if (attribute != null)
            {
                return attribute;
            }

            // If the attribute is not found on the property, check for a MetadataTypeAttribute on the class
            if (propertyInfo.DeclaringType == null)
            {
                return null;
            }
            var metadataTypeAttribute = Attribute.GetCustomAttribute(propertyInfo.DeclaringType, typeof(MetadataTypeAttribute)) as MetadataTypeAttribute;
            if (metadataTypeAttribute != null)
            {
                // If a MetadataTypeAttribute is found, use reflection to get the property from the
                // metadata class
                var metadataProperty = metadataTypeAttribute.MetadataClassType.GetProperty(propertyInfo.Name);
                if (metadataProperty != null)
                {
                    // If the property is found in the metadata class, check for the attribute on the
                    // metadata property
                    attribute = Attribute.GetCustomAttribute(metadataProperty, attributeType);
                    if (attribute != null)
                    {
                        return attribute;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the display label for the specified property expression.
        /// </summary>
        /// <param name="expression">The property expression.</param>
        /// <returns>The display label, or null if not found.</returns>
        public static string GetDisplayLabel(Expression<Func<object?>> expression)
        {
            var property = GetPropertyInfo(expression.Body);
            return property.GetDisplayLabel();
        }

        /// <summary>
        /// Gets the display label for the specified property expression.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="expression">The property expression.</param>
        /// <returns>The display label, or null if not found.</returns>
        public static string GetDisplayLabel<T>(Expression<Func<T, object?>> expression)
        {
            var property = GetPropertyInfo(expression.Body);
            return property.GetDisplayLabel();
        }

        /// <summary>
        /// Gets the display label for the specified property.
        /// </summary>
        /// <param name="property">The property information.</param>
        /// <returns>The display label.</returns>
        public static string GetDisplayLabel(this MemberInfo property)
        {
            ArgumentNullException.ThrowIfNull(property);
            if (property.GetAttributeIncludingMetadata(typeof(DisplayAttribute)) is DisplayAttribute displayAttr
                && !string.IsNullOrEmpty(displayAttr.Name))
            {
                var resourceManager = displayAttr.ResourceType != null ? new ResourceManager(displayAttr.ResourceType) : null;
                var culture = CultureInfo.CurrentUICulture;
                return resourceManager?.GetString(displayAttr.Name, culture) ?? displayAttr.Name;
            }
            else
            {
                return property.Name.SplitAndCapitalize();
            }
        }

        /// <summary>
        /// Gets the properties of the specified type, excluding properties that belong to the specified exclude types.
        /// </summary>
        /// <param name="thisType">The type to get properties from.</param>
        /// <param name="excludePropertyTypes">The types to exclude.</param>
        /// <returns>The filtered collection of properties.</returns>
        public static IEnumerable<PropertyInfo> GetExcludedProperties(this Type thisType, params Type[] excludePropertyTypes)
        {
            var properties = thisType.GetProperties();
            foreach (var t in excludePropertyTypes)
            {
                bool isImplement = t.IsAssignableFrom(thisType);
                if (isImplement)
                {
                    var tProperties = t.GetProperties();
                    properties = properties.Where(p => !tProperties.Any(tp => tp.Name == p.Name)).ToArray();
                }
            }
            return properties;
        }

        /// <summary>
        /// Gets the full path of the property, including nested properties.
        /// </summary>
        /// <param name="expression">The property expression.</param>
        /// <returns>The full property path.</returns>
        public static string GetFullPropertyName(this Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                string prefix = GetFullPropertyName(memberExpression.Expression!);
                return string.IsNullOrEmpty(prefix) ? memberExpression.Member.Name : prefix + "." + memberExpression.Member.Name;
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                return GetFullPropertyName(unaryExpression.Operand);
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the property information from the specified expression.
        /// </summary>
        /// <param name="expression">The property expression.</param>
        /// <returns>The property information.</returns>
        public static PropertyInfo GetPropertyInfo(this Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo property)
                    return property;

                throw new ArgumentException("Expression is not a property access");
            }

            if (expression is UnaryExpression unaryExpression)
                return GetPropertyInfo(unaryExpression.Operand);

            throw new ArgumentException("Invalid expression");
        }

        /// <summary>
        /// Get the property info 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyLambda"></param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T>> propertyLambda)
        {
            if (propertyLambda.Body is MemberExpression member)
            {
                if (member.Member is PropertyInfo propInfo)
                    return propInfo;
            }
            else if (propertyLambda.Body is UnaryExpression unary && unary.Operand is MemberExpression member2)
            {
                if (member2.Member is PropertyInfo propInfo)
                    return propInfo;
            }
            return null!;
        }

        /// <summary>
        /// Gets the value of the specified property by using an expression.
        /// </summary>
        /// <typeparam name="Tobj">The type of the object.</typeparam>
        /// <param name="self">The object instance.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property, or null if not found.</returns>
        public static object? GetPropertyValueByExpression<Tobj>(this Tobj self, string propertyName) where Tobj : class
        {
            try
            {
                var param = Expression.Parameter(typeof(Tobj), "value");
                var getter = Expression.Property(param, propertyName);
                var boxer = Expression.TypeAs(getter, typeof(object));
                var getPropValue = Expression.Lambda<Func<Tobj, object>>(boxer, param).Compile();
                return getPropValue(self);
            }
            catch
            {
                return null;
            }
        }

        #region 新增：多级属性显示名拼接（支持 TakeLastLevels）
        /// <summary>
            /// 获取表达式对应的属性显示名，并按需要拼接多级的显示名。
            /// </summary>
            /// <param name="expression">如 () => p.Tool.Name</param>
            /// <param name="takeLastLevels">
            /// 取末尾多少级的显示名进行拼接：
            /// 1 = 仅末级（默认行为）；2 = 末两级（如 "Tool + Name"）；
            /// 0 或 &lt;0 或 大于总层级数 = 取全部层级。
            /// </param>
            /// <param name="separator">拼接分隔符（默认 " + "）</param>
        public static string GetDisplayLabel(this Expression<Func<object?>> expression, int takeLastLevels, string separator = " / ")
            => GetDisplayLabelInternal(expression.Body, takeLastLevels, separator);

        public static string GetDisplayLabel<T>(this Expression<Func<T>> expression, int takeLastLevels, string separator = " / ")
            => GetDisplayLabelInternal(expression.Body, takeLastLevels, separator);

        /// <summary>
            /// 泛型版本：获取表达式对应的属性显示名，并按需要拼接多级的显示名。
            /// </summary>
        public static string GetDisplayLabel<T>(this Expression<Func<T, object?>> expression, int takeLastLevels, string separator = " / ")
            => GetDisplayLabelInternal(expression.Body, takeLastLevels, separator);

        /// <summary>
            /// 兼容旧语义：includeParents = true 等价于 takeLastLevels = 全部层级；false 等价于 1。
            /// </summary>
        public static string GetDisplayLabel(this Expression<Func<object?>> expression, bool includeParents, string separator = " / ")
            => GetDisplayLabelInternal(expression.Body, includeParents ? 0 : 1, separator);

        public static string GetDisplayLabel<T>(this Expression<Func<T, object?>> expression, bool includeParents, string separator = " / ")
            => GetDisplayLabelInternal(expression.Body, includeParents ? 0 : 1, separator);
        public static string GetDisplayLabel<T>(this Expression<Func<T>> expression, bool includeParents, string separator = " / ")
            => GetDisplayLabelInternal(expression.Body, includeParents ? 0 : 1, separator);
        private static string GetDisplayLabelInternal(this Expression body, int takeLastLevels, string separator)
        {
            var chain = GetMemberChain(body); // 根->叶，如 [Tool, Name]
            if (chain.Count == 0)
                return string.Empty;

            // 计算需要的切片范围
            if (takeLastLevels <= 0 || takeLastLevels >= chain.Count)
            {
                // 取全部
                return string.Join(separator, chain.Select(m => m.GetDisplayLabel()));
            }

            // 取末尾 N 级
            var slice = chain.Skip(chain.Count - takeLastLevels);
            return string.Join(separator, slice.Select(m => m.GetDisplayLabel()));
        }

        /// <summary>
            /// 解析表达式为成员链（根->叶）。仅收集属性（PropertyInfo）。
            /// </summary>
        private static List<MemberInfo> GetMemberChain(Expression expression)
        {
            var list = new List<MemberInfo>();

            void Walk(Expression exp)
            {
                switch (exp)
                {
                    case MemberExpression me:
                        Walk(me.Expression!);
                        if (me.Member is PropertyInfo) list.Add(me.Member);
                        break;

                    case UnaryExpression ue:
                        Walk(ue.Operand);
                        break;

                    case MethodCallExpression mce:
                        // 忽略索引器或方法名（如 get_Item），但继续下探对象部分以尽可能提取属性链
                        if (mce.Object != null) Walk(mce.Object);
                        break;

                    case ParameterExpression:
                    case ConstantExpression:
                        break;

                    default:
                        throw new NotSupportedException($"不支持的表达式类型：{exp.NodeType}");
                }
            }

            Walk(expression);
            return list;
        }
        #endregion

        /// <summary>
        /// Determines whether the specified property is human-readable.
        /// </summary>
        /// <param name="property">The property information.</param>
        /// <returns>True if the property is human-readable, otherwise false.</returns>
        public static bool IsHumanReadable(this PropertyInfo property)
        {
            var p = property;
            return (!p.PropertyType.IsClass || p.PropertyType == typeof(string))
                && (!p.PropertyType.IsGenericType || !typeof(IEnumerable<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()))
                && (!p.PropertyType.IsGenericType || !typeof(ICollection<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()));
        }

        /// <summary>
        /// Determines whether the specified property is numeric or nullable numeric.
        /// </summary>
        /// <param name="property">The property information.</param>
        /// <returns>True if the property is numeric or nullable numeric, otherwise false.</returns>
        public static bool IsNumericOrNullableNumeric(this PropertyInfo property)
        {
            var type = property.PropertyType;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.IsPrimitive)
            {
                return type != typeof(bool) &&
                       type != typeof(char) &&
                       type != typeof(nint) &&
                       type != typeof(nuint);
            }
            return type == typeof(decimal);
        }

        /// <summary>
        /// Determines whether the specified property is of the specified type or nullable type.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <param name="property">The property information.</param>
        /// <returns>True if the property is of the specified type or nullable type, otherwise false.</returns>
        public static bool IsTypeOrNullableType<T>(this PropertyInfo property)
        {
            var type = property.PropertyType;
            type = Nullable.GetUnderlyingType(type) ?? type;
            return type == typeof(T);
        }

        /// <summary>
        /// Sets the value of the specified property by using an expression.
        /// </summary>
        /// <typeparam name="Tobj">The type of the object.</typeparam>
        /// <param name="self">The object instance.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        public static void SetPropertyValue<Tobj>(this Tobj self, string propertyName, object? value)
        {
            if (self is null) return;
            if (value == default || string.IsNullOrEmpty(value.ToString()))
            {
                self.GetType().GetProperty(propertyName)?.SetValue(self, default);
            }
            else
            {
                var convertValue = TypeExtensions.ChangeType(value, self.GetType().GetProperty(propertyName)?.PropertyType!);
                self.GetType().GetProperty(propertyName)?.SetValue(self, convertValue);
            }
        }

        /// <summary>
        /// Sets the values of the properties of the specified object from another object of the same type.
        /// </summary>
        /// <typeparam name="Tobj">The type of the object.</typeparam>
        /// <param name="self">The object instance.</param>
        /// <param name="sourceObj">The source object.</param>
        public static void SetPropertyValues<Tobj>(this Tobj self, Tobj sourceObj)
        {
            var properties = typeof(Tobj).GetProperties();
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    self.SetPropertyValue(property.Name, property.GetValue(sourceObj));
                }
            }
        }

        /// <summary>
        /// Builds a sorting function for MudTable based on the specified property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Func<object, object> BuildMudTableHeaderSortBy(this PropertyInfo property)
        {
            var ptype = property.PropertyType;
            var underlying = Nullable.GetUnderlyingType(ptype) ?? ptype;
            var typeCode = Type.GetTypeCode(underlying);
            switch (typeCode)
            {
                case TypeCode.String:
                    return new Func<object, object>(x => (object)(property.GetValue(x)?.ToString() ?? string.Empty));
                case TypeCode.DateTime:
                    return new Func<object, object>(x => (object)Convert.ToDateTime(property.GetValue(x) ?? DateTime.MinValue));
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    return new Func<object, object>(x => (object)Convert.ToDouble(property.GetValue(x) ?? 0));
                default:
                    return new Func<object, object>(x => (object)(property.GetValue(x)?.ToString() ?? string.Empty));
            }
        }

        #endregion
    }
}
