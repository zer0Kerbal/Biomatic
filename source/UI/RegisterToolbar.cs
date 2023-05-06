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

using ToolbarControl_NS;
using UnityEngine;
using KSP.UI.Screens;

namespace Biomatic
{
    /// <summary>RegisterToolbar</summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {

        internal static RegisterToolbar Instance;

        internal void Start()
        {
            ToolbarControl.RegisterMod(BiomaticUI.MODID, BiomaticUI.MODNAME);
            if (Instance != null)
            {
                // Reloading of GameDatabase causes another copy of addon to spawn at next opportunity. Suppress it.
                // see: https://forum.kerbalspaceprogram.com/index.php?/topic/7542-x/&do=findComment&comment=3574980
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        internal void OnDestroy()
        {
            if (Instance != null)
            {
                Instance.OnDestroy();
                Destroy(Instance);
            }
            Instance = null;
        }
    }
}
