namespace Bepop.Core
{
    public partial class Log
    {
        /// <summary>
        /// ������־
        /// </summary>
        public static Logger BASE = new Logger("BASE", "whrite");

        /// <summary>
        /// UI��־
        /// </summary>
        public static Logger UI = new Logger("UI", "green");

        /// <summary>
        /// ������־
        /// </summary>
        public static Logger NET = new Logger("NET", "blue");

        /// <summary>
        /// ��Ϣ��־
        /// </summary>
        public static Logger MSG = new Logger("MSG", "orange");

        /// <summary>
        /// ʾ����־
        /// </summary>
        public static Logger EX = new Logger("EXAMPLE", "yellow");



        public static bool DebugOut = true;

    }
}

