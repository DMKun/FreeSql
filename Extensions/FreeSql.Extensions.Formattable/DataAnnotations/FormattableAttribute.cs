using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql.DataAnnotations
{
    /// <summary>
    /// 当实现IFormattable接口时，调用IFormattable.ToString方法映射存储
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FormattableAttribute : Attribute
    {
        /// <summary>
        /// 要使用的格式。
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 要用于格式化值的提供程序。IFormatProvider类型
        /// </summary>
        public object FormatProvider { get; set; }
    }
}
