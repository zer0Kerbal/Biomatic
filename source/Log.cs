using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Biomatic
{
    class Log :MonoBehaviour
    {

        internal static string MsgBase = String.Format("[Biomatic v{0}] ", Version.Text);

        /// <summary>
        /// sends the specific message to ingame mail and screen if Debug is defined
        /// For debugging use only.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="params">The parameters.</param>
        [ConditionalAttribute("DEBUG")]
        internal static void dbg(string msg, params object[] @params)
        {
            if (0 != @params.Length) msg = string.Format(String.Format("{0}{1}", MsgBase, msg), @params);
            ScreenMessages.PostScreenMessage(msg, 1, ScreenMessageStyle.UPPER_CENTER, true);
            print(String.Format("{0}{1}", MsgBase, msg));
        }

        internal static void Info(string msg)
        {
            Debug.Log(String.Format("{0}{1}", MsgBase, msg));
        }
    }
    internal class Debug : MonoBehaviour
    {

        internal static string MsgBase = String.Format("[Biomatic v{0}] ", Version.Text);

        internal static void Log(string msg, params object[] @params)
        {
            if (0 != @params.Length) msg = string.Format(String.Format("{0}{1}", MsgBase, msg), @params);
            ScreenMessages.PostScreenMessage(msg, 1, ScreenMessageStyle.UPPER_CENTER, true);
            print(String.Format("{0}{1}", MsgBase, msg));
        }
    }
}
