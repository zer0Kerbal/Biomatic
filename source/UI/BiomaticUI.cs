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
using System.Linq;
using System.Text;
using System.Threading;//.Tasks;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;
using KSP.Localization;
using ToolbarControl_NS;
using UnityEngine.Networking;
using Biomatic.Extensions;
using static GameEvents;
using Input = UnityEngine.Input;

namespace Biomatic
{
    /// <summary>class: BiomaticUI</summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class BiomaticUI : MonoBehaviour
    {
        internal static BiomaticUI Instance; // internal -> private

        internal const string MODID = "Biomatic_ID";
        internal const string MODNAME = "Biomatic";

        const string shownOnButton = "BiomaticGreyOn";
        const string shownOffButton = "BiomaticGreyOff";
        const string hiddenOnButton = "BiomaticColourOn";
        const string hiddenOffButton = "BiomaticColourOff";
        const string unavailableButton = "BiomaticUnavailable";
        const string IconPath = "Biomatic/Plugins/ToolbarIcons/";
        //const string IconPath = "Biomatic/Plugins/PluginData/ToolbarIcons/";

        private readonly int BiomaticWinID = (DateTime.Now.Hour* 3600 + DateTime.Now.Minute* 60 + DateTime.Now.Second) * 1000 + DateTime.Now.Millisecond + 1;

        private static Rect windowPos = new();
        //private static Rect windowPos = new Rect();

        private static string warpButtonText = Localizer.Format("#BIO-warp-none");
        private static bool deWarp = false;
        private static bool includeAlt = false;
        private static bool showHistory = false;
        private static bool showDescription = false;
        private static bool perVessel = false;

        private Vessel ActiveVessel = FlightGlobals.ActiveVessel;

        private static List<string> listIgnore = null;

        private bool isPowered = true;
        private bool wasPowered = true;

        private bool lostToStaging = false;

        private GUIStyle styleTextArea = null;
        private GUIStyle styleButton = null;
        private GUIStyle styleValue = null;
        private GUIStyle styleToggle = null;

        private float fixedwidth = 255f;
        private readonly float defaultwidth = 255f;
        private readonly float margin = 20f;

        private static bool prevConditionalShow = false;
        private static bool ConditionalShow = true;

        private BiomeSituation biome = new();
        private BiomeSituation prevBiome = new();
        //private BiomeSituation biome = new BiomeSituation();
        //private BiomeSituation prevBiome = new BiomeSituation();

        private BiomeSituation[] historyArray = null;

        private int numParts = -1;
        private int stage = -1;

        private Vector3 clickedPosition;
        private Rect originalWindow;
        private bool handleClicked = false;

        private static bool instantDewarp = false;

        // sound generation
        private static AudioSource audioSource = null;

        private static float[] cachedBeep = null;
        private readonly static bool beepCreated = false;
        private static int soundType = 0;


        private static bool systemOn = true;
        /// <summary>System On</summary>
        public static bool SystemOn
        {
            get { return systemOn; }
            set { systemOn = value; }
        }

        #region Private methods

        //private static bool UseToolbar = false;
        private static bool toolbarShowSettings = false;
        /// <summary>Toobar Show Settings</summary>
        public static bool ToolbarShowSettings
        {
            get { return toolbarShowSettings; }
            set
            {
                toolbarShowSettings = value;
                sizechange = true;
            }
        }

        //private static StockToolbar stockToolbar = null;
        private static bool useStockToolBar = true;
        public static bool UseStockToolBar
        {
            get { return useStockToolBar; }
            set { useStockToolBar = value; }
        }

        //private static ToolbarButtonWrapper toolbarButton = null;
        ToolbarControl toolbarControl;
        private bool newInstance = true;

        // resize window? - prevents blinking when buttons clicked
        private static bool sizechange = true;

        /// <summary><para>Hides all user interface elements.</para></summary>
        bool _hideUIwhenPaused = false;

        private static bool _hideUI = false;
        public static bool HideUI
        {
            get { return _hideUI; }
            set { _hideUI = value; }
        }

        public void HideUIAction()
        {
            _hideUI = true;
        }
        void ShowUIAction()
        {
            _hideUI = false;
        }

#endregion Private methods
#region Mono
        public void Awake() // Runs first
        {
           Log.Info(msg: "AWAKE");

            if (Instance != null)
            {
                Debug.Log("kill me!");
                Destroy(gameObject);
                return;
            }
            //Instance = this;
            ActiveVessel = FlightGlobals.ActiveVessel;
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            Instance = this;
            Log.Info("BiomaticUI.OnStart");
            if (Instance != null)
            {
                // Reloading of GameDatabase causes another copy of addon to spawn at next opportunity. Suppress it.
                // see: https://forum.kerbalspaceprogram.com/index.php?/topic/7542-x/&do=findComment&comment=3574980
                Destroy(gameObject);
                return;
            }

            audioSource = GetComponent<AudioSource>();

            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(
                OnClick, OnClick,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                MODID,
                "biomaticButton",
                IconPath + "BiomaticUnavailable",
                MODNAME
            );

            GameEvents.onHideUI.Add(this.HideUIAction);
            GameEvents.onShowUI.Add(this.ShowUIAction);
            GameEvents.onGamePause.Add(this.HideUIWhenPaused);
            GameEvents.onGameUnpause.Add(this.ShowUIwhenPaused);

        }


        // Following get run multiple times


        private void OnGUI()
        {
            if (!HideUI && !_hideUIwhenPaused) // && AppLauncherKerbalGPS.Instance.displayGUI)
                try
                {
                    if (!TechChecker.TechAvailable || !Biomatic.BiomaticIsEnabled) return;

                    historyArray ??= new BiomeSituation[5];
                    //if (historyArray == null) historyArray = new BiomeSituation[5];

                    if (!HighLogic.CurrentGame.Parameters.CustomParams<Options>().useAlternateSkin) GUI.skin = HighLogic.Skin;
                    Draw();
                }
                catch (Exception ex) { Log.Info(msg: String.Format("Biomatic - OnGUI(): {0}", ex.Message)); }
        }

        private void OnClick()
        {
            toolbarShowSettings = !toolbarShowSettings;
            UpdateToolbarButton();
            sizechange = true;
        }

        public void OnFixedUpdate()
        { }

        public void Update()
        { }

        public void LateUpdate()
        { }

        // Runs when being destroyed
        public void OnDestroy()
        {
            GameEvents.onHideUI.Remove(this.HideUIAction);
            GameEvents.onShowUI.Remove(this.ShowUIAction);
            GameEvents.onGamePause.Remove(this.HideUIWhenPaused);
            GameEvents.onGameUnpause.Remove(this.ShowUIwhenPaused);
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
            Instance = null;
            Log.Info("OnDestroy");
        }

#endregion Mono
#region IO

        public void OnSave(ConfigNode node) // override
        {
            try
            {
                PluginConfiguration config = PluginConfiguration.CreateForType<Biomatic>();

                config.SetValue("Window Position", windowPos);

                config.SetValue("List items", listIgnore.Count);
                List<string>.Enumerator en = listIgnore.GetEnumerator();
                int count = 0;
                while (en.MoveNext())
                {
                    config.SetValue("Item" + count.ToString(), en.Current);
                    count++;
                }
                en.Dispose();

                config.SetValue("DeWarp", deWarp);
                config.SetValue("Use altitude", includeAlt);
                config.SetValue("Show description", showDescription);
                config.SetValue("Width", ((int)100 * fixedwidth));
                config.SetValue("Toolbar", useStockToolBar ? "stock" : "blizzy");
                config.SetValue("Instant dewarp", instantDewarp);
                config.SetValue("OnOff", (systemOn ? "On" : "Off"));
                config.SetValue("Show recent", showHistory);
                config.SetValue("Sound", soundType.ToString());
                config.SetValue("Per vessel", perVessel);

                config.save();
            }
            catch (Exception ex) { Log.Info(msg: String.Format("Biomatic - OnSave(): {0}", ex.Message)); }
        }

        public  void OnLoad(ConfigNode node) // override
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<Biomatic>();

            config.load();

            try
            {
                windowPos = config.GetValue<Rect>("Window Position");

                if (listIgnore == null) listIgnore = new List<string>();
                else listIgnore.Clear();

                int count = config.GetValue<int>("List items");
                for (int i = 0; i < count; i++) listIgnore.Add(config.GetValue<string>("Item" + i.ToString()));

                deWarp = config.GetValue<bool>("DeWarp");
                includeAlt = config.GetValue<bool>("Use altitude");
                showDescription = config.GetValue<bool>("Show description");
                fixedwidth = ((float)config.GetValue<int>("Width")) / 100;

                if (fixedwidth < defaultwidth) fixedwidth = defaultwidth;

                string s = config.GetValue<string>("Toolbar");
                s = s.ToLower();
                useStockToolBar = !(s.Contains("blizzy"));
                instantDewarp = config.GetValue<bool>("Instant dewarp");

                systemOn = (config.GetValue<string>("OnOff") == "On");

                showHistory = config.GetValue<bool>("Show recent");

                soundType = Convert.ToInt32(config.GetValue<string>("Sound"));

                perVessel = config.GetValue<bool>("Per vessel");
            }
            catch (Exception ex) { Log.Info(msg: String.Format("Biomatic - OnLoad(): {0}", ex.Message)); }

            warpButtonText = "None";
            if (deWarp)
            {
                warpButtonText = "Gradual";
                if (instantDewarp) warpButtonText = "Instant";
            }

            windowPos.width = fixedwidth;

           // base.OnLoad(node);
        }

#endregion
#region GUIButtons
        
        public void HideUIWhenPaused()
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<Options>().hideWhenPaused)
                _hideUIwhenPaused = true;
        }
        void ShowUIwhenPaused()
        {
            _hideUIwhenPaused = false;
        }


        void UpdateToolbarButton()
        {
            string blizzyButtonPath = IconPath + "Biom";
            bool relevant = IsRelevant;
            if (relevant)
            {
                blizzyButtonPath += toolbarShowSettings ? "Grey" : "Colour";
                if (!systemOn) blizzyButtonPath += "X";
            }
            else blizzyButtonPath += "Unavailable";

            string stockButtonPath;
            if (TechChecker.TechAvailable)
            {
                if (SystemOn) stockButtonPath = (ToolbarShowSettings ? shownOnButton : hiddenOnButton);
                else stockButtonPath = (ToolbarShowSettings ? shownOffButton : hiddenOffButton);
            }
            else stockButtonPath = unavailableButton;
            toolbarControl.SetTexture(IconPath + stockButtonPath, blizzyButtonPath);
        }

#endregion
#region GUI

        private void Draw()
        {
            DoResize();

            ActiveVessel = FlightGlobals.ActiveVessel;

            if (ActiveVessel != null)
            {
                // this takes account of vessels splitting (when undocking), Kerbals going on EVA, etc.
                if (newInstance || (useStockToolBar && (ActiveVessel.parts.Count != numParts || ActiveVessel.currentStage != stage)))
                {
                    numParts = ActiveVessel.parts.Count;
                    stage = ActiveVessel.currentStage;

                    newInstance = false;
                    lostToStaging = false;

                    UpdateToolbarButton();
                }

                if (RightConditionsToDraw)
                {
                    if (HideUI)
                    {
                        try { DoBiomaticContent(); }
                        catch (Exception ex)
                        {
                            Log.Info(String.Format("Draw() - DoBiomaticContent() threw {0}", ex.Message));
                        }
                    }
                    else
                    {
                        styleTextArea = new GUIStyle(GUI.skin.textArea);
                        styleTextArea.normal.textColor = Color.green;
                        styleTextArea.alignment = TextAnchor.MiddleCenter;
                        styleTextArea.fixedWidth = fixedwidth - 40;

                        styleButton = new GUIStyle(GUI.skin.button);
                        styleButton.normal.textColor = styleButton.hover.textColor = styleButton.focused.textColor = styleButton.active.textColor = Color.white;
                        styleButton.active.textColor = Color.green;
                        styleButton.padding = new RectOffset(0, 0, 0, 0);

                        styleToggle = new GUIStyle(GUI.skin.toggle);

                        styleValue = new GUIStyle(GUI.skin.label);
                        styleValue.normal.textColor = Color.white;
                        styleValue.alignment = TextAnchor.MiddleCenter;

                        if (ConditionalShow != prevConditionalShow) sizechange = true;

                        if (sizechange)
                        {
                            windowPos.yMax = windowPos.yMin + 20;
                            sizechange = false;
                        }

                        // windowPos = GUILayout.Window(this.ClassID, windowPos, OnWindow, ConditionalShow ? Localizer.Format("#BIO-wind-title-1") : Localizer.Format("#BIO-wind-title-2"), GUILayout.Width(fixedwidth));//"Biomatic""Biomatic settings"
                        windowPos = GUILayout.Window(BiomaticWinID, windowPos, OnWindow, ConditionalShow ? Localizer.Format("#BIO-wind-title-1") : Localizer.Format("#BIO-wind-title-2"), GUILayout.Width(fixedwidth));//"Biomatic""Biomatic settings"
                        windowPos.width = fixedwidth;

                        if (windowPos.x == 0 && windowPos.y == 0) windowPos = windowPos.CenterScreen();
                    }
                }
            }
        }

        private bool RightConditionsToDraw
        {
            get
            {
              // if (!Biomatic.IsPrimary(ActiveVessel.parts, BiomaticWinID)) return false;

                if (lostToStaging)
                {
                    prevConditionalShow = ConditionalShow = false;
                    return false;
                }

                bool retval = true;
                if (!systemOn || !Biomatic.BiomaticIsEnabled) retval = false;

                prevConditionalShow = ConditionalShow;
                ConditionalShow = retval;
                return ConditionalShow || toolbarShowSettings;
            }
        }

        private void OnWindow(int windowID)
        {
            try { DoBiomaticContent(); }
            catch (Exception ex)
            {

                Log.Info(String.Format("Biomatic - OnWindow() - DoBiomaticContent() threw " + ex.Message.ToString()));
            }

            if (!handleClicked) GUI.DragWindow();
        }

        private void DoResize()
        {
            if (windowPos.width > 25)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;    // Convert to GUI coords

                Rect windowHandle = new(windowPos.x + windowPos.width - 25, windowPos.y, 25, windowPos.height);
                //Rect windowHandle = new Rect(windowPos.x + windowPos.width - 25, windowPos.y, 25, windowPos.height);

                // If clicked on window resize widget
                if (!handleClicked && Input.GetMouseButtonDown(0) &&
                    windowHandle.x < mousePos.x && windowHandle.x + windowHandle.width > mousePos.x &&
                    windowHandle.y < mousePos.y && windowHandle.y + windowHandle.height > mousePos.y)
                {
                    handleClicked = true;
                    clickedPosition = mousePos;
                    originalWindow = windowPos;
                }

                if (handleClicked)
                {
                    // Resize window by dragging
                    if (Input.GetMouseButton(0))
                    {
                        windowPos.width = Mathf.Clamp(originalWindow.width + (mousePos.x - clickedPosition.x), defaultwidth, defaultwidth * 2);
                        fixedwidth = windowPos.width;
                    }
                    // Finish resizing window
                    if (Input.GetMouseButtonUp(0))
                    {
                        handleClicked = false;
                        sizechange = true;
                    }
                }
            }
        }

        private void DoBiomaticContent()
        {
            if (ConditionalShow)
            {
                isPowered = true;// Biomatic.IsPowered;
                if (isPowered != wasPowered)
                {
                    sizechange = true;
                    wasPowered = isPowered;
                }

                if (isPowered && Biomatic.BiomaticIsEnabled)
                {
                    biome = BiomeSituation;
                    if (!prevBiome.IsSameAs(biome, includeAlt, perVessel))
                    {
                        prevBiome = biome;
                        if (soundType == 2) DoSound();

                        if (!BiomeInList(biome.GetText(includeAlt, perVessel)))
                        {
                            if (soundType == 1) DoSound();
                            if (deWarp) TimeWarp.SetRate(0, instantDewarp);
                        }
                        AddToArray(biome);
                    }
                }
            }

            if (!HideUI)
            {
                ShowGraphicalIndicator();
                ShowSettings();
            }
        }

        private BiomeSituation BiomeSituation => new(BiomeString, SituationString, ActiveVessel.mainBody.name, ActiveVessel.id);
        //private BiomeSituation BiomeSituation => new BiomeSituation(BiomeString, SituationString, ActiveVessel.mainBody.name, ActiveVessel.id);

        private void AddToArray(BiomeSituation biome)
        {
            for (int i = 4; i > 0; i--)
            {
                if (historyArray[i] == null) sizechange = true;
                historyArray[i] = historyArray[i - 1];
            }
            historyArray[0] = biome;
        }

        private void ShowGraphicalIndicator()
        {
            if (systemOn && ConditionalShow && Biomatic.BiomaticIsEnabled)
            {
                if (showDescription)
                {
                    styleValue.normal.textColor = styleValue.focused.textColor = Color.green;
                    GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                    GUILayout.Label(ActiveVessel.RevealSituationString(), styleValue);
                    GUILayout.EndHorizontal();
                }
                
                DisplayLine(historyArray[0]);

                if (isPowered && showHistory) ShowHistoricBiomeSits();
            }
        }

        private void ShowHistoricBiomeSits()
        {
            styleButton.normal.textColor = Color.red;
            for (int i = 1; i < 5; i++)
                if (historyArray[i] != null) DisplayLine(historyArray[i]);
                else break;
        }

        private void DisplayLine(BiomeSituation bs)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
            if (isPowered)
            {
                styleTextArea.normal.textColor = Color.green;
                string text = GetOutputString(bs);
                GUILayout.Label(text, styleTextArea);

                styleButton.normal.textColor = (text.Contains("\u2713") ? Color.red : Color.green);
                if (GUILayout.Button(".", styleButton, GUILayout.ExpandWidth(true)))
                {
                    if (text.Contains("\u2713")) RemoveCurrentBiomeFromList(bs);
                    else AddCurrentBiomeToList(bs);
                }
                styleButton.normal.textColor = styleButton.hover.textColor = styleButton.focused.textColor = styleButton.active.textColor = Color.white;
                styleButton.active.textColor = Color.green;
            }
            else
            {
                styleTextArea.normal.textColor = Color.grey;
                GUILayout.Label("----" + Localizer.Format("#BIO-label-unpowered") + "----", styleTextArea);//unpowered
            }
            GUILayout.EndHorizontal();
        }

        private string GetFullBiomeName(string bio, bool useAlt)
        {
            string result = ActiveVessel.mainBody.name + "." + bio;

            if (useAlt) result = SituationString + "." + result;

            return result;
        }

        private string SituationString
        {
            get
            {
                string result = String.Empty;
                ExperimentSituations sit = ScienceUtil.GetExperimentSituation(ActiveVessel);
                switch (sit)
                {
                    case ExperimentSituations.FlyingHigh:
                        result = Localizer.Format("#BIO-situtation-FlyingHigh");//"High flight"
                        break;
                    case ExperimentSituations.FlyingLow:
                        result = Localizer.Format("#BIO-situtation-FlyingLow");//"Low flight"
                        break;
                    case ExperimentSituations.InSpaceHigh:
                        result = Localizer.Format("#BIO-situtation-InSpaceHigh");//"High above"
                        break;
                    case ExperimentSituations.InSpaceLow:
                        result = Localizer.Format("#BIO-situtation-InSpaceLow");//"Just above"
                        break;
                    case ExperimentSituations.SrfLanded:
                        result = Localizer.Format("#BIO-situtation-SrfLanded");//"Landed"
                        break;
                    case ExperimentSituations.SrfSplashed:
                        result = Localizer.Format("#BIO-situtation-SrfSplashed");//"Splashed"
                        break;
                }

                return result;
            }
        }

        // show settings buttons / fields
        private void ShowSettings()
        {
            if (toolbarShowSettings)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleButton.normal.textColor = Color.white;
                if (GUILayout.Button(Localizer.Format("#BIO-button-remove", ActiveVessel.mainBody.bodyDisplayName), styleButton, GUILayout.ExpandWidth(true)))
                    RemoveCurrentBody();
                //"Remove " +  + " biomes from list"
                GUILayout.EndHorizontal();

                // de-warp
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleValue.normal.textColor = Color.white;
                GUILayout.Label(Localizer.Format("#BIO-warp-label"), styleValue);//"De-warp "
                if (GUILayout.Button(warpButtonText, styleButton, GUILayout.ExpandWidth(true)))
                {
                    if (warpButtonText == Localizer.Format("#BIO-warp-none"))
                    {
                        warpButtonText = Localizer.Format("#BIO-warp-gradual");
                        deWarp = true;
                        instantDewarp = false;
                    }
                    else if (warpButtonText == Localizer.Format("#BIO-warp-gradual"))
                    {
                        warpButtonText = Localizer.Format("#BIO-warp-instant");
                        instantDewarp = true;
                    }
                    else if (warpButtonText == Localizer.Format("#BIO-warp-instant"))
                    {
                        warpButtonText = Localizer.Format("#BIO-warp-none");
                        deWarp = false;
                        instantDewarp = false;
                    }
                }
                GUILayout.EndHorizontal();

                // sound
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleValue.normal.textColor = Color.white;
                GUILayout.Label(Localizer.Format("#BIO-label-sound"), styleValue);//"Sound"
                string beepButtonText = Localizer.Format("#BIO-generic-none");//"None"
                switch (soundType)
                {
                    case 1:
                        {
                            beepButtonText = Localizer.Format("#BIO-button-biome-unticked");//"Unticked Biome"
                            break;
                        }
                    case 2:
                        {
                            beepButtonText = Localizer.Format("#BIO-button-biome");//"Any Biome"
                            break;
                        }
                }

                if (GUILayout.Button(beepButtonText, styleButton, GUILayout.ExpandWidth(true)))
                {
                    soundType++;
                    if (soundType > 2) soundType = 0;
                }
                GUILayout.EndHorizontal();

                // per vessel
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleValue.normal.textColor = Color.white;
                GUILayout.Label(Localizer.Format("#BIO-label-biome-list"), styleValue);//"Biome list"
                string perVesselText = (perVessel ? Localizer.Format("#BIO-button-per-vessel") : Localizer.Format("#BIO-button-global"));//"Per vessel""Global"

                if (GUILayout.Button(perVesselText, styleButton, GUILayout.ExpandWidth(true)))
                {
                    perVessel = !perVessel;
                }
                GUILayout.EndHorizontal();

                //show hist
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                bool oldShowHistory = showHistory;
                showHistory = GUILayout.Toggle(showHistory, " " + Localizer.Format("#BIO-button-recent"), styleToggle, null);//Recent
                GUILayout.FlexibleSpace();
                // use altitude
                bool oldIncludeAlt = includeAlt;
                includeAlt = GUILayout.Toggle(includeAlt, " " + Localizer.Format("#BIO-button-altitude"), styleToggle, null);//Altitude
                if (includeAlt != oldIncludeAlt)
                {
                    sizechange = true;
                }
                GUILayout.FlexibleSpace();

                // show description
                bool oldShowDescription = showDescription;
                showDescription = GUILayout.Toggle(showDescription, " " + Localizer.Format("#BIO-button-desc"), styleToggle, null);//Description
                GUILayout.EndHorizontal();
                if (showDescription != oldShowDescription || oldShowHistory != showHistory)
                {
                    sizechange = true;
                }
                GUILayout.FlexibleSpace();

                styleButton.normal.textColor = styleButton.focused.textColor = styleButton.hover.textColor = styleButton.active.textColor = systemOn ? Color.red : Color.green;
                styleValue.normal.textColor = Color.white;

                // On / Off switch
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                GUILayout.Label(Localizer.Format("#BIO-wind-title-1") + " " + Version.SText + " ", styleValue);//Biomatic
                styleValue.normal.textColor = systemOn ? Color.green : Color.red;
                GUILayout.Label(systemOn ? Localizer.Format("#BIO-generic-oN") : Localizer.Format("#BIO-generic-off"), styleValue);//"ON ""OFF "
                if (GUILayout.Button(systemOn ? Localizer.Format("#BIO-button-switch-off") : Localizer.Format("#BIO-button-switch-on"), styleButton, GUILayout.ExpandWidth(true)))//"Switch off""Switch on"
                {
                    systemOn = !systemOn;
                    UpdateToolbarButton();

                    sizechange = true;
                }
                GUILayout.EndHorizontal();

                styleValue.normal.textColor = styleValue.focused.textColor = styleValue.hover.textColor = styleValue.active.textColor = Color.white;
                styleButton.normal.textColor = styleButton.focused.textColor = styleButton.hover.textColor = styleButton.active.textColor = Color.white;
            }
        }

/*        public void OnDestroy()
        {
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
            Instance = null;
        }*/

#endregion
#region Internal

        public CBAttributeMapSO.MapAttribute Biome
        {
            get
            {
                CBAttributeMapSO.MapAttribute mapAttribute;

                try
                {
                    var lat = ActiveVessel.latitude * Math.PI / 180d; // double
                    var lon = ActiveVessel.longitude * Math.PI / 180d; // double

                    mapAttribute = ActiveVessel.mainBody.BiomeMap.GetAtt(lat, lon);
                }
                catch (NullReferenceException)
                {
                    mapAttribute = new CBAttributeMapSO.MapAttribute
                    {
                        name = "N/A"
                    };
                }

                return mapAttribute;
            }
        }

        public string BiomeString
        {
            get
            {
                string biome_desc = String.Empty;

                try
                {
                    var lat = ActiveVessel.latitude * Math.PI / 180d; // double
                    var lon = ActiveVessel.longitude * Math.PI / 180d; // double

                    biome_desc = FlightGlobals.ActiveVessel.mainBody.BiomeMap.GetAtt(lat, lon).name;
                }
                catch (NullReferenceException)
                {
                }

                // for worlds without biomes, just use body name
                if (biome_desc == "") biome_desc = ActiveVessel.mainBody.name;

                return biome_desc;
            }
        }

        private void AddCurrentBiomeToList(BiomeSituation bs)
        {
            string fullname = bs.GetText(true, true);
            if (!BiomeInList(fullname)) listIgnore.Add(fullname);
        }

        private bool BiomeInList(string fullname)
        {
            bool result = false;

            using (List<string>.Enumerator en = listIgnore.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    if (en.Current.Contains(fullname))
                    {
                        result = true;
                        break;
                    }
                }
                en.Dispose();
            }

            return result;
        }

        private void RemoveCurrentBody()
        {
            try
            {
                while (true)
                {
                    bool removed = false;
                    foreach (string s in from string s in listIgnore
                                         where s.Contains(ActiveVessel.mainBody.name) && (!perVessel || s.Contains(ActiveVessel.id.ToString()))
                                         select s)
                    {
                        listIgnore.Remove(s);
                        removed = true;
                        break;
                    }

                    if (!removed) break;
                }
            }
            catch (Exception ex)
            {
                Log.Info(ex.Message);
            }
        }

        private void RemoveCurrentBiomeFromList(BiomeSituation bs)
        {
            try
            {
                while (true)
                {
                    bool removed = false;
                    foreach (var s in from string s in listIgnore
                                      where s.Contains(bs.Biome) && s.Contains(bs.Body) && (!includeAlt || s.Contains(bs.Situation)) && (!perVessel || s.Contains(bs.VesselGUID.ToString()))
                                      select s)
                    {
                        listIgnore.Remove(s);
                        removed = true;
                        break;
                    }

                    if (!removed) break;
                }
            }
            catch (Exception ex)
            {
                //print("Biomatic - RemoveCurrentBiomeFromList(): " + ex.Message);
                Log.Info(String.Format("RemoveCurrentBiomeFromList(): {0} ", ex.Message));
            }
        }

        private string GetOutputString(BiomeSituation bs)
        {
            if (bs == null)
                return "----";

            string output = bs.GetDescription(includeAlt);

            if (BiomeInList(bs.GetText(includeAlt, perVessel)))
                output += " \u2713";

            return output;
        }

        public static bool IsRelevant
        {
            get
            {
                if ((HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.PSYSTEM) && FlightGlobals.ActiveVessel != null)
                {
                    List<Biomatic> bio = FlightGlobals.ActiveVessel.FindPartModulesImplementing<Biomatic>();

                    if (bio != null && bio.Count > 0)
                        return true;
                }
                return false;
            }
        }

#endregion
#region Sound

        private void DoSound()
        {
            if (audioSource.isPlaying) audioSource.Stop();

            audioSource.PlayOneShot(MakeBeep, 0.9f);
        }

        private AudioClip MakeBeep => AudioClip.Create("beep", 4096, 1, 44100, false, OnAudioRead, OnAudioSetPosition);

        private void OnAudioRead(float[] data)
        {
            try
            {
                if (beepCreated)
                {
                    data = cachedBeep;
                    return;
                }

                float gain = 0.6f;
                float increment;
                float phase = 0;
                float sampling_frequency = 44100;

                increment = 880 * Mathf.PI / sampling_frequency;

                for (var i = 0; i < data.Length; i++)
                {
                    phase += increment;
                    //phase = phase + increment;

                    data[i] = Mathf.PingPong(gain * Mathf.Sin(phase), 1);

                    if (phase > 2 * Math.PI) phase = 0;
                }
                cachedBeep = data;
            }
            catch (Exception ex)
            {
                Log.Info(msg: "Biomatic - OnAudioRead(): " + ex.Message);
            }
        }

        private void OnAudioSetPosition(int newPosition)
        {
            return;
        }
#endregion Sound

    }
}
