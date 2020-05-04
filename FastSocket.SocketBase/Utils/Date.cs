using System;

namespace Sodao.FastSocket.SocketBase.Utils
{
    /// <summary>
    /// 关于时间的一些操作
    /// </summary>
    static public class Date
    {

        #region Public Methods
        /// <summary>
        /// Gets the current utc time in an optimized fashion.
        /// </summary>
        static public DateTime Now
        {
            get
            {
                DateTime dt = DateTime.Now;
                return dt;
            }
        }
        #endregion
    }
}