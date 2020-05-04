using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sodao.FastSocket.SocketBase.Messaging {

    /// <summary>
    /// command line message.
    /// </summary>
    [TypeConverter(typeof(CommandLineMessageTypeConverter))]
    public class CommandLineMessage {
        #region Public Members
        /// <summary>
        /// get the current command name.
        /// </summary>
        public string CmdName { get; }
        /// <summary>
        /// 参数
        /// </summary>
        public readonly string[] Parameters;
        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        public CommandLineMessage(Queue<string> queue) : this(queue.Dequeue(), queue) {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="parameters"></param>
        public CommandLineMessage(string cmdName, Queue<string> parameters) : this(cmdName, parameters.ToArray()) {

        }

        /// <summary>
        /// new
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException">cmdName is null</exception>
        public CommandLineMessage(string cmdName, params string[] parameters) {
            this.CmdName = cmdName ?? throw new ArgumentNullException("cmdName");
            this.Parameters = parameters;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => FullCommandLine;

        /// <summary>
        /// 
        /// </summary>
        public string FullCommandLine {
            get {
                var sb = new StringBuilder();
                sb.Append(CmdName);
                if (Parameters != null) {
                    foreach (var p in Parameters) {
                        sb.Append(" " + p);
                    }
                }
                return sb.ToString();
            }
        }


        #endregion
    }
}