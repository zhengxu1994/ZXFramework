namespace Bepop.Core
{
    public partial class Log
    {
        /// <summary>
        /// 基础日志
        /// </summary>
        public static Logger BASE = new Logger("BASE", "whrite");

        /// <summary>
        /// UI日志
        /// </summary>
        public static Logger UI = new Logger("UI", "green");

        /// <summary>
        /// 网络日志
        /// </summary>
        public static Logger NET = new Logger("NET", "blue");

        /// <summary>
        /// 消息日志
        /// </summary>
        public static Logger MSG = new Logger("MSG", "orange");

        /// <summary>
        /// 示例日志
        /// </summary>
        public static Logger EX = new Logger("EXAMPLE", "yellow");



        public static bool DebugOut = true;

    }
}

