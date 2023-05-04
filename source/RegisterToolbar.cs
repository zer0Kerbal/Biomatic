using ToolbarControl_NS;
using UnityEngine;
using KSP.UI.Screens;

namespace Biomatic
{

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
