using UnityEngine;
using ToolbarControl_NS;

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