using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace Proximity
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class StockToolbar : MonoBehaviour
    {
        internal static StockToolbar Instance;

        ToolbarControl toolbarControl = null;
        
        public bool ButtonNeeded
        {
            get;
            set;
        }

        const string ShownOnButton = "ProximityGreyOn";
        const string ShownOffButton = "ProximityGreyOff";
        const string HiddenOnButton = "ProximityColourOn";
        const string HiddenOffButton = "ProximityColourOff";
        const string UnavailableButton = "ProximityUnavailable";

        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        internal const string dataPath = "GameData/KSP_Proximity/PluginData/";
        internal const string buttonDirPath = "KSP_Proximity/ToolbarIcons/";

        string ButtonPath(string buttonName)
        {
            return Path.Combine(buttonDirPath, buttonName);
        }

        public void CreateButton()
        {
            ButtonNeeded = Proximity.IsRelevant();
            if (ButtonNeeded)
            {
                MakeButton();
            }
            else if (toolbarControl != null)
            {
                toolbarControl.buttonActive = false;
            }
        }

        internal const string MODID = "Proximity_NS";
        internal const string MODNAME = "Proximity";

        private void MakeButton()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(ProximityHide, ProximityShow,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                    MODID,
                    "proximityButton",
                    GetTextureName(38),
                    GetTextureName(24),
                    MODNAME
                );
                toolbarControl.AddLeftRightClickCallbacks(null, ToggleOnRightClick);
            }
            toolbarControl.buttonActive = true;
            if (!Proximity.ToolbarShowSettings)
            {
                toolbarControl.SetTrue(false);
            }
            else
            {
                toolbarControl.SetFalse(false);
            }

        }
        void ToggleOnRightClick()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                List<Proximity> prox = FlightGlobals.ActiveVessel.FindPartModulesImplementing<Proximity>();

                if (prox != null && prox.Count > 0)
                {
                    Proximity p = prox.FirstOrDefault();
                    if (p != null)
                        p.ToggleSystemOn();
                    return;
                }
            }
        }

        public void RefreshButtonTexture()
        {
            if (toolbarControl != null)
            {
                toolbarControl.SetTexture(GetTextureName(38), GetTextureName(24));
            }
        }

        private void ProximityHide()
        {
            if (Proximity.ToolbarShowSettings)
            {
                Proximity.ToolbarShowSettings = false;
                RefreshButtonTexture();
            }
        }

        private void ProximityShow()
        {
            if (!Proximity.ToolbarShowSettings)
            {
                Proximity.ToolbarShowSettings = true;
                RefreshButtonTexture();
            }
        }

        private string GetTextureName(int btnSize)
        {
            if (TechChecker.TechAvailable)
            {
                switch (btnSize)
                {
                    case 24:

                        string path24 = buttonDirPath + "ProxS";

                        path24 += Proximity.toolbarShowSettings ? "G" : "C";

                        if (!Proximity.SystemOn)
                        {
                            path24 += "X";
                        }
                        return path24.Replace(@"\", "/");                    

                    case 38:
                        if (Proximity.SystemOn)
                        {
                            return (buttonDirPath + (Proximity.ToolbarShowSettings ? ShownOnButton : HiddenOnButton)).Replace(@"\", "/");
                        }
                        else
                        {
                            return (buttonDirPath + (Proximity.ToolbarShowSettings ? ShownOffButton : HiddenOffButton)).Replace(@"\", "/");
                        }      
                }
                return "";
            }
            else
            {
                switch (btnSize)
                {
                    case 24:
                        return (buttonDirPath + "ProxSG2").Replace(@"\", "/");
                    case 38:
                        return (buttonDirPath + UnavailableButton).Replace(@"\", "/");
                }
                return "";
            }
        }

        private void OnDestroy()
        {
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
        }
    }
}
