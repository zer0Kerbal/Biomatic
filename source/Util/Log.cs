#region GPL-3.0 License
/* Biomatic (Biome sensor) for Kerbal Space Program
  *
  * Copyright (C) and (TM) Matt Reed, 2014
  * Copyright (C) and (TM) zer0Kerbal, 2019-2023
  *
  * This program is free software: you can redistribute it and/or modify
  * it under the terms of the GNU General Public License as published by
  * the Free Software Foundation, either version 3 of the License, or
  * (at your option) any later version.
  *
  * This program is distributed in the hope that it will be useful,
  * but WITHOUT ANY WARRANTY; without even the implied warranty of
  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  * GNU General Public License for more details.
  *
  * You should have received a copy of the GNU General Public License
  * along with this program.  If not, see <http://www.gnu.org/licenses/>.
  */
#endregion

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

        internal static string MsgBase = String.Format("[Biomatic v{0}] ", Version.SText);

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

        internal static string MsgBase = String.Format("[Biomatic v{0}] ", Version.SText);

        internal static void Log(string msg, params object[] @params)
        {
            if (0 != @params.Length) msg = string.Format(String.Format("{0}{1}", MsgBase, msg), @params);
            ScreenMessages.PostScreenMessage(msg, 1, ScreenMessageStyle.UPPER_CENTER, true);
            print(String.Format("{0}{1}", MsgBase, msg));
        }
    }
}
