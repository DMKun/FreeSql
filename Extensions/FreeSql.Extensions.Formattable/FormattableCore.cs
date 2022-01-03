using FreeSql.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

public static class FormattableCoreExtensions
{
    static int _isAoped = 0;
    static ConcurrentDictionary<Type, bool> _dicTypes = new ConcurrentDictionary<Type, bool>();
    static MethodInfo MethodToString = typeof(IFormattable).GetMethod("ToString", new[] { typeof(string), typeof(IFormatProvider) });

    /// <summary>
    /// 当实体类实现IFormattable接口时，并且标记特性 [FormattableAttribute] 时，该属性将调用IFormattable.ToString方法映射存储
    /// </summary>
    public static void UseFormattable(this IFreeSql that)
    {
        if (Interlocked.CompareExchange(ref _isAoped, 1, 0) != 0)
        {
            return;
        }

        that.Aop.ConfigEntityProperty += (s, e) =>
        {
            Type propType = e.Property.PropertyType;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propType = Nullable.GetUnderlyingType(propType);
            }
            var isFormattable = e.Property.GetCustomAttributes(typeof(FormattableAttribute), false).Any()
                                && typeof(IFormattable).IsAssignableFrom(propType);
            if (isFormattable)
            {
                var attr = e.Property.GetCustomAttributes(typeof(FormattableAttribute), false).OfType<FormattableAttribute>().First();
                e.ModifyResult.MapType = typeof(string);
                e.ModifyResult.StringLength = -2;

                if (_dicTypes.TryAdd(e.Property.PropertyType, true))
                {
                    FreeSql.Internal.Utils.GetDataReaderValueBlockExpressionObjectToStringIfThenElse.Add((LabelTarget returnTarget, Expression valueExp, Expression elseExp, Type type) =>
                    {
                        return Expression.IfThenElse(
                            Expression.TypeIs(valueExp, e.Property.PropertyType),
                            Expression.Return(returnTarget, Expression.Call(Expression.Convert(valueExp, typeof(IFormattable)), MethodToString, Expression.Constant(attr.Format, typeof(string)), Expression.Constant(attr.FormatProvider, typeof(IFormatProvider)))),
                            elseExp
                            );
                    });
                }
            }
        };
    }
}

