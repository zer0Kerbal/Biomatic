using System;
using UnityEngine;
using System.Collections.Generic;
using KSP.IO;
using KSP.Localization;
using UnityEngine.Networking;
using Biomatic.Extensions;
using ClickThroughFix;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace Biomatic
{

    class Biomatic : PartModule
    {
        internal static Biomatic Instance;

        internal const string MODID = "Biomatic_ID";
        internal const string MODNAME = "Biomatic";

        const string shownOnButton = "BiomaticGreyOn";
        const string shownOffButton = "BiomaticGreyOff";
        const string hiddenOnButton = "BiomaticColourOn";
        const string hiddenOffButton = "BiomaticColourOff";
        const string unavailableButton = "BiomaticUnavailable";
        const string IconPath = "Biomatic/Plugins/PluginData/ToolbarIcons/";

        private static Rect windowPos = new Rect();
#region Fields
        [KSPField(  isPersistant = true, guiActive = true, guiActiveEditor = true,
                    groupName = "Biomatic", groupStartCollapsed = true,
                    guiName = "Biomatic:"),
        UI_Toggle(disabledText = "Off", enabledText = "On")]
        public static bool _BiomaticIsEnabled = true;
        public static bool BiomaticIsEnabled
        {
            get { return _BiomaticIsEnabled; }
            set { _BiomaticIsEnabled = BiomaticIsEnabled; }
        }
        
        [KSPField(  guiActive = true, guiActiveEditor = true, groupName = "Biomatic",
                    guiName = "EC rate")]
        public float ECresourceConsumptionRate = 0.05f;
        #endregion
#region Events

        [KSPAction("Biomatic: Toggle")]
        public void toggleAction(KSPActionParam kap)
        { BiomaticIsEnabled = !BiomaticIsEnabled; }
#endregion
#region Actions
        [KSPAction("Biomatic: Enable")]
        public void enableAction(KSPActionParam kap)
        { BiomaticIsEnabled = true; }

        [KSPAction("Biomatic: Disable")]
        public void disableAction(KSPActionParam kap)
        { BiomaticIsEnabled = false; }
#endregion
#region Private functions
        // private static bool IsPrimary = true;
        private static bool _isPrimary = true;
        internal static bool IsPrimary
        {
            get { return _isPrimary; }
            set { _isPrimary = IsPrimary; }
        }

        //private static bool UseToolbar = false;
        private static bool toolbarShowSettings = false;
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

        private static bool systemOn = true;
        public static bool SystemOn
        {
            get { return systemOn; }
            set { systemOn = value; }
        }

        //private static ToolbarButtonWrapper toolbarButton = null;
        ToolbarControl toolbarControl;
        private bool newInstance = true;

        // resize window? - prevents blinking when buttons clicked
        private static bool sizechange = true;

        /// <summary><para>Hides all user interface elements.</summary>
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

        private static string warpButtonText = Localizer.Format("#Biomatic_None");
        private static bool deWarp = false;
        private static bool includeAlt = false;
        private static bool showHistory = false;
        private static bool showDescription = false;
        private static bool perVessel = false;

        private static List<string> listIgnore = null;

        private bool isPowered = true;
        private bool wasPowered = true;

        private bool lostToStaging = false;

        private GUIStyle styleTextArea = null;
        private GUIStyle styleButton = null;
        private GUIStyle styleValue = null;
        private GUIStyle styleToggle = null;

        private float fixedwidth = 255f;
        private float defaultwidth = 255f;
        private float margin = 20f;

        private static bool prevConditionalShow = false;
        private static bool ConditionalShow = true;

        private BiomeSituation biome = new BiomeSituation();
        private BiomeSituation prevBiome = new BiomeSituation();

        private BiomeSituation[] historyArray = null;

        private int numParts = -1;
        private int stage = -1;

        private Vessel ActiveVessel;

        private Vector3 clickedPosition;
        private Rect originalWindow;
        private bool handleClicked = false;

        private static bool instantDewarp = false;

        // sound generation
        private static AudioSource audioSource = null;

        private static float[] cachedBeep = null;
        private static bool beepCreated = false;
        private static int soundType = 0;

        /// <summary>Module information shown in editors</summary>
        private string info = string.Empty;

        /// <summary>ElectricCharge identification number</summary>
        public static int ElectricChargeID;

        // #region Mono
        #region Public Functions

        /// <summary>Called when part is added to the craft.</summary>
        public override void OnAwake()
        {
            // Log.Info(String.Format("OnAwake for {0}", name));
        }

        public void Start()
        {
            Log.Info("OnStart");
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

            if (ElectricChargeID == default(int))
                ElectricChargeID = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;
        }



        public override void OnFixedUpdate()
        {
            if (BiomaticIsEnabled && HighLogic.CurrentGame.Parameters.CustomParams<Options>().UseEC)
            {
                if (ConsumeEC(TimeWarp.fixedDeltaTime) == false)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#Biomatic_PM_EC01")); // "Electric Charge Depleted. Stopping Biomatic Scanning."
                    //Log.Info("OnFixedUpdate: Electric Charge Depleted. Stopping Biomatic Scanning.");
                }
            }
            base.OnFixedUpdate();
        }

        public override void OnUpdate()
        {
            if (!BiomaticIsEnabled) return;
/*            if (BiomaticIsEnabled && HighLogic.CurrentGame.Parameters.CustomParams<Options>().UseEC)
            {
                if (ConsumeEC(TimeWarp.fixedDeltaTime) == false)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#Biomatic_PM_EC01")); // "Electric Charge Depleted. Stopping Biomatic Scanning."
                   // Log.Info("OnUpdate: Electric Charge Depleted. Stopping Biomatic Scanning.");
                }
            }*/
            base.OnUpdate();
        }
       
        public bool ConsumeEC(double elapsed)
        {
            Log.Info(String.Format("ConsumeEC : elapsed: {0}", elapsed.ToString()));
            double ec = 0, amount = 0;
            if (CheatOptions.InfiniteElectricity == true) { Log.Info(String.Format("CheatOptions.InfiniteElectricity({0})", CheatOptions.InfiniteElectricity.ToString())); return true; }
            else foreach (Part part in ActiveVessel.parts)
                    foreach (PartResource res in part.Resources)
                        if (res.resourceName == "ElectricCharge" && res.amount > 0)
                        {
                            Log.Info(String.Format("part {0}.{1}:{2}]", part.name, res.resourceName, res.amount));
                            ec += res.amount;  // tally total EC available on ship
                            Log.Info(String.Format("total EC available {0} ]", ec.ToString()));
                        }

            amount = ECresourceConsumptionRate * TimeWarp.fixedDeltaTime;
            Log.Info(String.Format("EC available: {0} / Consumption Rate: {1} / fixedDeltaTime {2}", ec.ToString(), ECresourceConsumptionRate.ToString(), TimeWarp.fixedDeltaTime.ToString()));
            // if not enough EC to power, then SHut.It.Down
            if (ec < amount) return false;

            //? compute consumption
            //? don't forget to consume the EC needed to power this beast
            part.RequestResource(ElectricChargeID, amount);
            return true;
        }
#endregion
#region IO
        public override void OnSave(ConfigNode node)
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
            catch (Exception ex) { print("Biomatic - OnSave(): " + ex.Message); }
        }

        public override void OnLoad(ConfigNode node)
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
            catch (Exception ex) { print("Biomatic - OnLoad(): " + ex.Message); }

            warpButtonText = "None";
            if (deWarp)
            {
                warpButtonText = "Gradual";
                if (instantDewarp)  warpButtonText = "Instant";
            }

            windowPos.width = fixedwidth;

            base.OnLoad(node);
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
        private void OnClick()
        {
            toolbarShowSettings = !toolbarShowSettings;
            UpdateToolbarButton();
            sizechange = true;
        }

        private void OnGUI()
        {
            if (!HideUI && !_hideUIwhenPaused) // && AppLauncherKerbalGPS.Instance.displayGUI)
                try
                {
                    if (!TechChecker.TechAvailable || !BiomaticIsEnabled) return;

                    if (historyArray == null)  historyArray = new BiomeSituation[5];

                    if (!HighLogic.CurrentGame.Parameters.CustomParams<Options>().useAlternateSkin) GUI.skin = HighLogic.Skin;
                    Draw();
                }
                catch (Exception ex) { print("Biomatic - OnGUI(): " + ex.Message); }
        }

        void UpdateToolbarButton()
        {
            string blizzyButtonPath = IconPath + "Biom";
            bool relevant = IsRelevant();
            if (relevant)
            {
                blizzyButtonPath += toolbarShowSettings ? "Grey" : "Colour";
                if (!systemOn) blizzyButtonPath += "X";
            }
            else  blizzyButtonPath += "Unavailable";

            string stockButtonPath;
            if (TechChecker.TechAvailable)
            {
                if (Biomatic.SystemOn)  stockButtonPath = (Biomatic.ToolbarShowSettings ? shownOnButton : hiddenOnButton);
                else stockButtonPath = (Biomatic.ToolbarShowSettings ? shownOffButton : hiddenOffButton);
            }
            else  stockButtonPath = unavailableButton;
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

                if (RightConditionsToDraw())
                {
                    if (HideUI)
                    {
                        try { DoBiomaticContent(); }
                        catch (Exception ex)
                        {
                            Log.Info(String.Format("Draw() - DoBiomaticContent() threw " + ex.Message.ToString()));
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

                        windowPos = ClickThruBlocker.GUILayoutWindow(this.ClassID, windowPos, OnWindow, ConditionalShow ? Localizer.Format("#Biomatic_Window_title1") : Localizer.Format("#Biomatic_Window_title2"), GUILayout.Width(fixedwidth));//"Biomatic""Biomatic settings"
                        windowPos.width = fixedwidth;

                        if (windowPos.x == 0 && windowPos.y == 0)  windowPos = windowPos.CentreScreen();
                    }
                }
            }
        }


        private bool RightConditionsToDraw()
        {
            bool retval = true;

            if (!part.IsPrimary(ActiveVessel.parts, ClassID))  return false;

            if (lostToStaging)
            {
                prevConditionalShow = ConditionalShow = false;
                return false;
            }

            if (!systemOn || !BiomaticIsEnabled) retval = false;

            prevConditionalShow = ConditionalShow;
            ConditionalShow = retval;
            return ConditionalShow || toolbarShowSettings;
        }

        private void OnWindow(int windowID)
        {
            try { DoBiomaticContent(); }
            catch (Exception ex)
            {

                Log.Info(String.Format(("Biomatic - OnWindow() - DoBiomaticContent() threw " + ex.Message.ToString())));
            }

            if (!handleClicked) GUI.DragWindow();
        }

        private void DoBiomaticContent()
        {
            if (ConditionalShow)
            {
                isPowered = IsPowered();
                if (isPowered != wasPowered)
                {
                    sizechange = true;
                    wasPowered = isPowered;
                }

                if (isPowered && BiomaticIsEnabled)
                {
                    biome = GetBiomeSituation();
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

        private BiomeSituation GetBiomeSituation()
        {
            return new BiomeSituation(GetBiomeString(), GetSituationString(), ActiveVessel.mainBody.name, ActiveVessel.id);
        }

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
            if (systemOn && ConditionalShow && BiomaticIsEnabled)
            {
                if (showDescription)
                {
                    styleValue.normal.textColor = styleValue.focused.textColor = Color.green;
                    GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                    GUILayout.Label(ActiveVessel.RevealSituationString(), styleValue);
                    GUILayout.EndHorizontal();
                }

                DisplayLine(historyArray[0]);

                if (isPowered && showHistory)  ShowHistoricBiomeSits();
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
                GUILayout.Label("----" + Localizer.Format("#Biomatic_Label_unpowered") + "----", styleTextArea);//unpowered
            }
            GUILayout.EndHorizontal();
        }

        private string GetFullBiomeName(string bio, bool useAlt)
        {
            string result = ActiveVessel.mainBody.name + "." + bio;

            if (useAlt) result = GetSituationString() + "." + result;

            return result;
        }

        private string GetSituationString()
        {
            string result = "";
            ExperimentSituations sit = ScienceUtil.GetExperimentSituation(ActiveVessel);
            switch (sit)
            {
                case ExperimentSituations.FlyingHigh:
                    result = Localizer.Format("#Biomatic_Situation_FlyingHigh");//"High flight"
                    break;
                case ExperimentSituations.FlyingLow:
                    result = Localizer.Format("#Biomatic_Situation_FlyingLow");//"Low flight"
                    break;
                case ExperimentSituations.InSpaceHigh:
                    result = Localizer.Format("#Biomatic_Situation_InSpaceHigh");//"High above"
                    break;
                case ExperimentSituations.InSpaceLow:
                    result = Localizer.Format("#Biomatic_Situation_InSpaceLow");//"Just above"
                    break;
                case ExperimentSituations.SrfLanded:
                    result = Localizer.Format("#Biomatic_Situation_SrfLanded");//"Landed"
                    break;
                case ExperimentSituations.SrfSplashed:
                    result = Localizer.Format("#Biomatic_Situation_SrfSplashed");//"Splashed"
                    break;
            }

            return result;
        }

        // show settings buttons / fields
        private void ShowSettings()
        {
            if (toolbarShowSettings)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleButton.normal.textColor = Color.white;
                if (GUILayout.Button(Localizer.Format("#Biomatic_Button_Remove", ActiveVessel.mainBody.bodyDisplayName), styleButton, GUILayout.ExpandWidth(true))) 
                    RemoveCurrentBody();
                //"Remove " +  + " biomes from list"
                GUILayout.EndHorizontal();

                // de-warp
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleValue.normal.textColor = Color.white;
                GUILayout.Label(Localizer.Format("#Biomatic_Label_Warp"), styleValue);//"De-warp "
                if (GUILayout.Button(warpButtonText, styleButton, GUILayout.ExpandWidth(true)))
                {
                    if (warpButtonText == Localizer.Format("#Biomatic_None"))
                    {
                        warpButtonText = Localizer.Format("#Biomatic_Gradual");
                        deWarp = true;
                        instantDewarp = false;
                    }
                    else if (warpButtonText == Localizer.Format("#Biomatic_Gradual"))
                    {
                        warpButtonText = Localizer.Format("#Biomatic_Instant");
                        instantDewarp = true;
                    }
                    else if (warpButtonText == Localizer.Format("#Biomatic_Instant"))
                    {
                        warpButtonText = Localizer.Format("#Biomatic_None");
                        deWarp = false;
                        instantDewarp = false;
                    }
                }
                GUILayout.EndHorizontal();

                // sound
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleValue.normal.textColor = Color.white;
                GUILayout.Label(Localizer.Format("#Biomatic_Label_Sound"), styleValue);//"Sound"
                string beepButtonText = Localizer.Format("#Biomatic_Generic_None");//"None"
                switch (soundType)
                {
                    case 1:
                        {
                            beepButtonText = Localizer.Format("#Biomatic_Button_UntickedBiome");//"Unticked Biome"
                            break;
                        }
                    case 2:
                        {
                            beepButtonText = Localizer.Format("#Biomatic_Button_AnyBiome");//"Any Biome"
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
                GUILayout.Label(Localizer.Format("#Biomatic_Label_Biomelist"), styleValue);//"Biome list"
                string perVesselText = (perVessel ? Localizer.Format("#Biomatic_Button_Pervessel") : Localizer.Format("#Biomatic_Button_Global"));//"Per vessel""Global"

                if (GUILayout.Button(perVesselText, styleButton, GUILayout.ExpandWidth(true)))
                {
                    perVessel = !perVessel;
                }
                GUILayout.EndHorizontal();

                //show hist
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                bool oldShowHistory = showHistory;
                showHistory = GUILayout.Toggle(showHistory, " " + Localizer.Format("#Biomatic_Button_Recent"), styleToggle, null);//Recent
                GUILayout.FlexibleSpace();
                // use altitude
                bool oldIncludeAlt = includeAlt;
                includeAlt = GUILayout.Toggle(includeAlt, " " + Localizer.Format("#Biomatic_Button_Altitude"), styleToggle, null);//Altitude
                if (includeAlt != oldIncludeAlt)
                {
                    sizechange = true;
                }
                GUILayout.FlexibleSpace();

                // show description
                bool oldShowDescription = showDescription;
                showDescription = GUILayout.Toggle(showDescription, " " + Localizer.Format("#Biomatic_Button_Description"), styleToggle, null);//Description
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
                GUILayout.Label(Localizer.Format("#Biomatic_Window_title1") + " " + Version.Text + " ", styleValue);//Biomatic
                styleValue.normal.textColor = systemOn ? Color.green : Color.red;
                GUILayout.Label(systemOn ? Localizer.Format("#Biomatic_Generic_ON") : Localizer.Format("#Biomatic_Generic_OFF"), styleValue);//"ON ""OFF "
                if (GUILayout.Button(systemOn ? Localizer.Format("#Biomatic_Button_Switchoff") : Localizer.Format("#Biomatic_Button_Switchon"), styleButton, GUILayout.ExpandWidth(true)))//"Switch off""Switch on"
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

        public void OnDestroy()
        {
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                Destroy(toolbarControl);
            }
            Instance = null;
        }
        #endregion
 #region Internal
        private bool IsPowered()
        {
            foreach (Part p in ActiveVessel.parts)
                foreach (PartResource pr in p.Resources)
                    if (pr.resourceName.Equals("ElectricCharge") && pr.flowState)
                        if (pr.amount >= ECresourceConsumptionRate)  return true;

            return false;
        }

        public CBAttributeMapSO.MapAttribute GetBiome()
        {
            CBAttributeMapSO.MapAttribute mapAttribute;

            try
            {
                double lat = ActiveVessel.latitude * Math.PI / 180d;
                double lon = ActiveVessel.longitude * Math.PI / 180d;

                mapAttribute = ActiveVessel.mainBody.BiomeMap.GetAtt(lat, lon);
            }
            catch (NullReferenceException)
            {
                mapAttribute = new CBAttributeMapSO.MapAttribute();
                mapAttribute.name = "N/A";
            }

            return mapAttribute;
        }

        public string GetBiomeString()
        {
            string biome_desc = "";

            try
            {
                double lat = ActiveVessel.latitude * Math.PI / 180d;
                double lon = ActiveVessel.longitude * Math.PI / 180d;

                biome_desc = FlightGlobals.ActiveVessel.mainBody.BiomeMap.GetAtt(lat, lon).name;
            }
            catch (NullReferenceException)
            {
            }

            // for worlds without biomes, just use body name
            if (biome_desc == "")  biome_desc = ActiveVessel.mainBody.name;

            return biome_desc;
        }

        private void AddCurrentBiomeToList(BiomeSituation bs)
        {
            string fullname = bs.GetText(true, true);
            if (!BiomeInList(fullname))
            {
                listIgnore.Add(fullname);
            }
        }

        private bool BiomeInList(string fullname)
        {
            bool result = false;

            List<string>.Enumerator en = listIgnore.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Current.Contains(fullname))
                {
                    result = true;
                    break;
                }
            }
            en.Dispose();

            return result;
        }

        private void RemoveCurrentBody()
        {
            try
            {
                while (true)
                {
                    bool removed = false;
                    foreach (string s in listIgnore)
                    {
                        if (s.Contains(ActiveVessel.mainBody.name) && (!perVessel || s.Contains(ActiveVessel.id.ToString())))
                        {
                            listIgnore.Remove(s);
                            removed = true;
                            break;
                        }
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
                    foreach (string s in listIgnore)
                    {
                        if (s.Contains(bs.Biome) && s.Contains(bs.Body) && (!includeAlt || s.Contains(bs.Situation)) && (!perVessel || s.Contains(bs.VesselGUID.ToString())))
                        {
                            listIgnore.Remove(s);
                            removed = true;
                            break;
                        }
                    }
                    if (!removed)  break;
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
            {
                return "----";
            }

            string output = bs.GetDescription(includeAlt);

            if (BiomeInList(bs.GetText(includeAlt, perVessel)))
            {
                output += " \u2713";
            }

            return output;
        }

        public static bool IsRelevant()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.PSYSTEM)
            {
                if (FlightGlobals.ActiveVessel != null)
                {
                    List<Biomatic> bio = FlightGlobals.ActiveVessel.FindPartModulesImplementing<Biomatic>();

                    if (bio != null && bio.Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void DoResize()
        {
            if (windowPos.width > 25)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;    // Convert to GUI coords

                Rect windowHandle = new Rect(windowPos.x + windowPos.width - 25, windowPos.y, 25, windowPos.height);

                // If clicked on window resize widget
                if (!handleClicked && (Input.GetMouseButtonDown(0) &&
                    windowHandle.x < mousePos.x && windowHandle.x + windowHandle.width > mousePos.x &&
                    windowHandle.y < mousePos.y && windowHandle.y + windowHandle.height > mousePos.y))
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
#endregion
#region Sound
        private void DoSound()
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.PlayOneShot(MakeBeep(), 0.9f);
        }

        private AudioClip MakeBeep()
        {
            return AudioClip.Create("beep", 4096, 1, 44100, false, OnAudioRead, OnAudioSetPosition);
        }

        void OnAudioRead(float[] data)
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
                    phase = phase + increment;

                    data[i] = Mathf.PingPong(gain * Mathf.Sin(phase), 1);

                    if (phase > 2 * Math.PI) phase = 0;
                }

                cachedBeep = data;
            }
            catch (Exception ex)
            {
                print("Biomatic - OnAudioRead(): " + ex.Message);
            }
        }

        void OnAudioSetPosition(int newPosition)
        {
            return;
        }
        #endregion
        private static string RateString(double Rate)
        {
            //  double rate = double.Parse(value.value);
            string sfx = "/sec";
            if (Rate <= 0.004444444f)
            {
                Rate *= 3600;
                sfx = "/hr";
            }
            else if (Rate < 0.2666667f)
            {
                Rate *= 60;
                sfx = "/min";
            }
            // limit decimal places to 10 and add sfx
            //return String.Format(FuelRateFormat, Rate, sfx);
            return Rate.ToString("###.#####") + " EC" + sfx;
        }

        /// <summary>Formats the information for the part information in the editors.</summary>
        /// <returns>info</returns>
        public override string GetInfo()
        {
            //? this is what is show in the editor
            //? As annoying as it is, pre-parsing the config MUST be done here, because this is called during part loading.
            //? The config is only fully parsed after everything is fully loaded (which is why it's in OnStart())
            if (info == string.Empty)
            {
                info += Localizer.Format("#Biomatic_manu"); // #Biomatic_manu = Biff Industries, Inc.
                info += "\n<color=#b4d455FF>" + Localizer.Format("#Biomatic_desc"); // #Biomatic_desc = In-flight biome identifier
                info += "\n\n<color=orange>Requires:</color><color=#FFFFFFFF> \n- <b>" + Localizer.Format("#autoLOC_252004"); // #autoLOC_252004 = ElectricCharge
                info += "</b>: </color><color=#99FF00FF>" + RateString(ECresourceConsumptionRate) + "</color>"; 
            }
            return info;
        }
    }
}
#endregion
