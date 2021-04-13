using System;
using System.Reflection;

namespace FInject
{
    public static class Utility
    {
        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetGetMethod();
            if(getMethod != null)
            {
                return getMethod.IsStatic;
            }

            var setMethod = propertyInfo.GetSetMethod();
            return setMethod.IsStatic;
        }
    }
}