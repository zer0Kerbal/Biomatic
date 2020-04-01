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
        /// <summary>
        /// sends the specific message to ingame mail and screen if Debug is defined
        /// For debugging use only.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="params">The parameters.</param>
        [ConditionalAttribute("DEBUG")]
        internal static void dbg(string msg, params object[] @params)
        {
            msg += "[Biomatic] : {0";
            if (0 != @params.Length) msg = string.Format(msg, @params);
            ScreenMessages.PostScreenMessage(msg, 1, ScreenMessageStyle.UPPER_CENTER, true);
            print(String.Format("[Biomatic v{0}] {1}", Version.Text, msg));
            //Debug.Log(String.Format("[Biomatic v{0}] {1}", Version.Text, msg));
            // UnityEngine.Debug.Log("[Biomatic] " + msg);
        }
    }
}
