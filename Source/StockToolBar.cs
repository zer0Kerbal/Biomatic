using System.IO;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;

namespace Biomatic
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class StockToolbar : MonoBehaviour
    {
        private static Texture2D shownOnButton;
        private static Texture2D shownOffButton;
        private static Texture2D hiddenOnButton;
        private static Texture2D hiddenOffButton;
        private static Texture2D unavailableButton;

        private ApplicationLauncherButton stockToolbarBtn;

        private bool buttonNeeded = false;
        public bool ButtonNeeded
        {
            get { return buttonNeeded; }
            set { buttonNeeded = value; }
        }

        void Start()
        {
            if (Biomatic.UseStockToolBar)
            {
                Load(ref shownOnButton, "BiomaticGreyOn.png");
                Load(ref shownOffButton, "BiomaticGreyOff.png");
                Load(ref hiddenOnButton, "BiomaticColourOn.png");
                Load(ref hiddenOffButton, "BiomaticColourOff.png");
                Load(ref unavailableButton, "BiomaticUnavailable.png");

                GameEvents.onGUIApplicationLauncherReady.Add(CreateButton);
            }
            DontDestroyOnLoad(this);
        }

        private void Load(ref Texture2D tex, string file)
        {
            if (tex == null)
            {
                tex = new Texture2D(36, 36, TextureFormat.RGBA32, false);
                tex.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), file)));
            }
        }

        public void CreateButton()
        {
            buttonNeeded = Biomatic.IsRelevant();
            if (buttonNeeded)
            {
                MakeButton();
            }
            else if (stockToolbarBtn != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarBtn);
            }
        }

        private void MakeButton()
        {
            if (stockToolbarBtn != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarBtn);
            }

            stockToolbarBtn = ApplicationLauncher.Instance.AddModApplication(
                BiomaticHide, BiomaticShow, null, null, null, null,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, GetTexture());

            if (!Biomatic.ToolbarShowSettings)
            {
                stockToolbarBtn.SetTrue(false);
            }
            else
            {
                stockToolbarBtn.SetFalse(false);
            }
        }

        public void RefreshButtonTexture()
        {
            if (stockToolbarBtn != null)
            {
                stockToolbarBtn.SetTexture(GetTexture());
            }
        }

        private void BiomaticHide()
        {
            if (Biomatic.ToolbarShowSettings)
            {
                Biomatic.ToolbarShowSettings = false;
                RefreshButtonTexture();
            }
        }

        private void BiomaticShow()
        {
            if (!Biomatic.ToolbarShowSettings)
            {
                Biomatic.ToolbarShowSettings = true;
                RefreshButtonTexture();
            }
        }

        private Texture2D GetTexture()
        {
            Texture2D tex;

            if (TechChecker.TechAvailable)
            {
                if (Biomatic.SystemOn)
                {
                    tex = (Biomatic.ToolbarShowSettings ? shownOnButton : hiddenOnButton);
                }
                else
                {
                    tex = (Biomatic.ToolbarShowSettings ? shownOffButton : hiddenOffButton);
                }
            }
            else
            {
                tex = unavailableButton;
            }

            return tex;
        }

        private void OnDestroy()
        {
            if (stockToolbarBtn != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarBtn);
            }
        }
    }
}
