using System;
using System.ComponentModel;
using System.Globalization;

namespace Sodao.FastSocket.SocketBase.Messaging {
    /// <summary>
    /// 
    /// </summary>
    public abstract class TextMessageTypeConverter<TMessage> : TypeConverter {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null) throw new ArgumentNullException();
            return (value is string text) ? CreateMessage(text) : throw new ArgumentOutOfRangeException($"不支持的类型：{value.GetType().FullName}"); //(, null);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected abstract TMessage CreateMessage(string text);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return destinationType == typeof(string);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (value == null) throw new ArgumentNullException();
            if (destinationType != typeof(string)) throw new ArgumentOutOfRangeException();

            return (value is TMessage msg) ? GetText(msg) : throw new ArgumentOutOfRangeException($"不支持的类型：{value.GetType().FullName}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract string GetText(TMessage message);
    }
}