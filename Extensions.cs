using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Utf8Json;

namespace functors
{
    public static class Extensions
    {
        public static T Show<T>(this T obj, string name = null, Expression<Func<T, object>> select = null, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int? line = null)
        {
            object result = obj;
            var selectStr = select?.ToString();
            if (obj != null)
            {
                result = select?.Compile().Invoke(obj) ?? obj;
                if (select?.Body.NodeType == ExpressionType.Convert && select.Body is UnaryExpression unary)
                    selectStr = $"{selectStr.Substring(0, selectStr.IndexOf('>') + 1)} {unary.Operand}";
            }
            var selectorStr = select != null ? $"selector {selectStr} for" : string.Empty;
            Console.WriteLine($"{JsonSerializer.PrettyPrint(JsonSerializer.Serialize(result))} Show of {selectorStr} type {obj?.GetType().GetFriendlyName() ?? "(unknown runtime type)"} in {file}:line {line}");
            return obj;
        }

        public static T NullCheck<T>(this T isNull, string name = null, [CallerMemberName] string member = null, [CallerFilePath] string file = null, [CallerLineNumber] int? line = null)
        {
            if (isNull == null)
            {
                if (name != null)
                {
                    Console.WriteLine($"NullCheck failed in {member}: '{name}' is NULL in {file}:line {line}");
                }
                else
                {
                    Console.WriteLine($"NullCheck failed in {member}: NULL in {file}:line {line}");
                }
            }
            return isNull;
        }

        private static readonly Dictionary<Type, string> _typeToFriendlyName = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(sbyte), "sbyte" },
            { typeof(float), "float" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName;
            if (_typeToFriendlyName.TryGetValue(type, out friendlyName))
            {
                return friendlyName;
            }

            friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int backtick = friendlyName.IndexOf('`');
                if (backtick > 0)
                {
                    friendlyName = friendlyName.Remove(backtick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; i++)
                {
                    string typeParamName = typeParameters[i].GetFriendlyName();
                    friendlyName += (i == 0 ? typeParamName : ", " + typeParamName);
                }
                friendlyName += ">";
            }

            if (type.IsArray)
            {
                return type.GetElementType().GetFriendlyName() + "[]";
            }

            return friendlyName;
        }
    }
}
