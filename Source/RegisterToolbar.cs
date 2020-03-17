using ToolbarControl_NS;
using UnityEngine;
using KSP.UI.Screens;

namespace Biomatic
{

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(Biomatic.MODID, Biomatic.MODNAME);
        }
    }
}
