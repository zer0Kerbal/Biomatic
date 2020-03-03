using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.IO;
using Biomatic.Extensions;
using KSP.Localization;
using ClickThroughFix;
using ToolbarControl_NS;

namespace Biomatic
{
    class Biomatic : PartModule
    {
        private static Rect windowPos = new Rect();

        private static bool UseToolbar = false;
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

        private static StockToolbar stockToolbar = null;

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

        private static ToolbarButtonWrapper toolbarButton = null;
        private bool newInstance = true;

        // resize window? - prevents blinking when buttons clicked
        private static bool sizechange = true;

        private static bool hideUI = false;
        public static bool HideUI
        {
            get { return hideUI; }
            set { hideUI = value; }
        }

        private static string warpButtonText = "None";
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

        private void OnGUI()
        {
            try
            {
                if (!TechChecker.TechAvailable)
                {
                    return;
                }

                if (historyArray == null)
                {
                    historyArray = new BiomeSituation[5];
                }

                if (Event.current.type == EventType.Repaint || Event.current.isMouse)
                {
                    if (!useStockToolBar) // blizzy
                    {
                        try
                        {
                            toolbarButton = new ToolbarButtonWrapper("Biomatic", "toolbarButton");
                            RefreshBlizzyButton();
                            toolbarButton.ToolTip = Localizer.Format("#Biomatic_Window_title2");//"Biomatic settings"
                            toolbarButton.Visible = true;
                            toolbarButton.AddButtonClickHandler((e) =>
                            {
                                toolbarShowSettings = !toolbarShowSettings;
                                RefreshBlizzyButton();
                                sizechange = true;
                            });
                        }
                        catch (Exception) { }

                        UseToolbar = true;
                    }
                    else // stock
                    {
                        UseToolbar = true;
                    }

                    if (audioSource == null)
                    {
                        audioSource = GetComponent<AudioSource>();
                    }
                }

                Draw();
            }
            catch (Exception ex)
            {
                print("Biomatic - OnGUI(): " + ex.Message);
            }
        }

        private bool RefreshStockButton()
        {
            bool result = false;

            try
            {
                if (stockToolbar == null)
                {
                    stockToolbar = (StockToolbar)StockToolbar.FindObjectOfType(typeof(StockToolbar));

                    if (stockToolbar != null)
                    {
                        result = true;
                        stockToolbar.ButtonNeeded = true;
                        stockToolbar.CreateButton();
                        if (!stockToolbar.ButtonNeeded)
                        {
                            result = false;
                            windowPos.height = 20;
                            lostToStaging = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                print("Biomatic - RefreshStockButton(): " + ex.Message);
            }

            return result;
        }

        private bool RefreshBlizzyButton()
        {
            bool relevant = IsRelevant();
            toolbarButton.Visible = relevant;

            if (relevant)
            {
                string path = "Biomatic/ToolbarIcons/Biom";
                path += toolbarShowSettings ? "Grey" : "Colour";

                if (!systemOn)
                {
                    path += "X";
                }

                toolbarButton.TexturePath = path;
            }
            else
            {
                lostToStaging = true;
            }

            return relevant;
        }

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
            catch (Exception ex)
            {
                print("Biomatic - OnSave(): " + ex.Message);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<Biomatic>();

            config.load();

            try 
            { 
                windowPos = config.GetValue<Rect>("Window Position");

                if (listIgnore == null)
                {
                    listIgnore = new List<string>();
                }
                else
                {
                    listIgnore.Clear();
                }

                int count = config.GetValue<int>("List items");
                for (int i = 0; i < count; i++)
                {
                    listIgnore.Add(config.GetValue<string>("Item" + i.ToString()));
                }

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
            catch (Exception ex)
            {
                print("Biomatic - OnLoad(): " + ex.Message);
            }

            warpButtonText = "None";
            if (deWarp)
            {
                warpButtonText = "Gradual";
                if (instantDewarp)
                {
                    warpButtonText = "Instant";
                }
            }

            windowPos.width = fixedwidth;
        }

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

                    if (useStockToolBar)
                    {
                        if (!RefreshStockButton())
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!RefreshBlizzyButton())
                        {
                            return;
                        }
                    }
                }

                if (RightConditionsToDraw())
                {
                    if (HideUI)
                    {
                        try
                        {
                            DoBiomaticContent();
                        }
                        catch (Exception ex)
                        {
                            print("Biomatic - Draw() - DoBiomaticContent() threw " + ex.Message);
                            Log.dbg("Biomatic - Draw() - DoBiomaticContent() threw " + ex.Message);
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

                        if (ConditionalShow != prevConditionalShow)
                        {
                            sizechange = true;
                        }

                        if (sizechange)
                        {
                            windowPos.yMax = windowPos.yMin + 20;
                            sizechange = false;
                        }

                        windowPos = ClickThruBlocker.GUILayoutWindow(this.ClassID, windowPos, OnWindow, ConditionalShow ? Localizer.Format("#Biomatic_Window_title1") : Localizer.Format("#Biomatic_Window_title2"), GUILayout.Width(fixedwidth));//"Biomatic""Biomatic settings"
                        windowPos.width = fixedwidth;

                        if (windowPos.x == 0 && windowPos.y == 0)
                        {
                            windowPos = windowPos.CentreScreen();
                        }
                    }
                }
            }
        }

        private bool RightConditionsToDraw()
        {
            bool retval = true;

            if(!part.IsPrimary(ActiveVessel.parts, ClassID))
            {
                return false;
            }

            if (lostToStaging)
            {
                prevConditionalShow = ConditionalShow = false;
                return false;
            }

            if (!systemOn)
            { 
                retval = false;
            }

            prevConditionalShow = ConditionalShow;
            ConditionalShow = retval;
            return ConditionalShow || (UseToolbar && toolbarShowSettings);
        }

        private void OnWindow(int windowID)
        {
            try
            {
                DoBiomaticContent();
            }
            catch (Exception ex)
            {
                print("Biomatic - OnWindow() - DoBiomaticContent() threw " + ex.Message);
                Log.dbg("Biomatic - OnWindow() - DoBiomaticContent() threw " + ex.Message);
            }

            if (!handleClicked)
            {
                GUI.DragWindow();
            }
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

                if (isPowered)
                {
                    biome = GetBiomeSituation();

                    if (!prevBiome.IsSameAs(biome, includeAlt, perVessel))
                    {
                        prevBiome = biome;

                        if (soundType == 2)
                        {
                            DoSound();
                        }

                        if (!BiomeInList(biome.GetText(includeAlt, perVessel)))
                        {
                            if (soundType == 1)
                            {
                                DoSound();
                            }

                            if (deWarp)
                            {
                                TimeWarp.SetRate(0, instantDewarp);
                            }
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
                if (historyArray[i] == null)
                {
                    sizechange = true;
                }
                historyArray[i] = historyArray[i - 1];
            }
            historyArray[0] = biome;
        }

        private void ShowGraphicalIndicator()
        {
            if (systemOn && ConditionalShow)
            {
                if (showDescription)
                {
                    styleValue.normal.textColor = styleValue.focused.textColor = Color.green;
                    GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                    GUILayout.Label(ActiveVessel.RevealSituationString(), styleValue);
                    GUILayout.EndHorizontal();
                }

                DisplayLine(historyArray[0]);

                if (isPowered && showHistory)
                {
                    ShowHistoricBiomeSits();
                }
            }
        }

        private void ShowHistoricBiomeSits()
        {
            styleButton.normal.textColor = Color.red;
            for (int i = 1; i < 5; i++)
            {
                if (historyArray[i] != null)
                {
                    DisplayLine(historyArray[i]);
                }
                else
                {
                    break;
                }
            }
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
                    if (text.Contains("\u2713"))
                    {
                        RemoveCurrentBiomeFromList(bs);
                    }
                    else
                    {
                        AddCurrentBiomeToList(bs);
                    }
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

            if (useAlt)
            {
                result = GetSituationString() + "." + result;
            }

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
            if (UseToolbar && toolbarShowSettings)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleButton.normal.textColor = Color.white;
                if (GUILayout.Button(Localizer.Format("#Biomatic_Button_Remove", ActiveVessel.mainBody.bodyDisplayName), styleButton, GUILayout.ExpandWidth(true)))//"Remove " +  + " biomes from list"
                {
                    RemoveCurrentBody();
                }
                GUILayout.EndHorizontal();

                // de-warp
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                styleValue.normal.textColor = Color.white;
                GUILayout.Label(Localizer.Format("#Biomatic_Label_De_warp"), styleValue);//"De-warp "
                if (GUILayout.Button(warpButtonText, styleButton, GUILayout.ExpandWidth(true)))
                {
                    if (warpButtonText == "None")
                    {
                        warpButtonText = "Gradual";
                        deWarp = true;
                        instantDewarp = false;
                    }
                    else if (warpButtonText == "Gradual")
                    {
                        warpButtonText = "Instant";
                        instantDewarp = true;
                    }
                    else if (warpButtonText == "Instant")
                    {
                        warpButtonText = "None";
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
                    if (soundType > 2)
                    {
                        soundType = 0;
                    }
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

                // use altitude
                bool oldIncludeAlt = includeAlt;
                includeAlt = GUILayout.Toggle(includeAlt, " " + Localizer.Format("#Biomatic_Button_Altitude"), styleToggle, null);//Altitude
                if (includeAlt != oldIncludeAlt)
                {
                    sizechange = true;
                }

                // show description
                bool oldShowDescription = showDescription;
                showDescription = GUILayout.Toggle(showDescription, " " + Localizer.Format("#Biomatic_Button_Description"), styleToggle, null);//Description
                GUILayout.EndHorizontal();
                if (showDescription != oldShowDescription || oldShowHistory != showHistory)
                {
                    sizechange = true;
                }

                styleButton.normal.textColor = styleButton.focused.textColor = styleButton.hover.textColor = styleButton.active.textColor = systemOn ? Color.red: Color.green;
                styleValue.normal.textColor = Color.white;

                // On / Off switch
                GUILayout.BeginHorizontal(GUILayout.Width(fixedwidth - margin));
                GUILayout.Label(Localizer.Format("#Biomatic_Window_title1") + " ", styleValue);//Biomatic
                styleValue.normal.textColor = systemOn ? Color.green: Color.red;
                GUILayout.Label(systemOn ? Localizer.Format("#Biomatic_Generic_ON") : Localizer.Format("#Biomatic_Generic_OFF"), styleValue);//"ON ""OFF "
                if (GUILayout.Button(systemOn ? Localizer.Format("#Biomatic_Button_Switchoff") : Localizer.Format("#Biomatic_Button_Switchon"), styleButton, GUILayout.ExpandWidth(true)))//"Switch off""Switch on"
                {
                    systemOn = !systemOn;
                    if (!useStockToolBar)
                    {
                        RefreshBlizzyButton();
                    }
                    else 
                    {
                        StockToolbar stb = (StockToolbar)StockToolbar.FindObjectOfType(typeof(StockToolbar));
                        if (stb != null)
                        {
                            stb.RefreshButtonTexture();
                        }
                    }

                    sizechange = true;
                }
                GUILayout.EndHorizontal();

                styleValue.normal.textColor = styleValue.focused.textColor = styleValue.hover.textColor = styleValue.active.textColor = Color.white;
                styleButton.normal.textColor = styleButton.focused.textColor = styleButton.hover.textColor = styleButton.active.textColor = Color.white;
            }
        }

        public void OnDestroy()
        {
            if (toolbarButton != null)
            {
                toolbarButton.Destroy();
                // added
                toolbarButton = null;
            }
        }

        private bool IsPowered()
        {
            foreach (Part p in ActiveVessel.parts)
            {
                foreach (PartResource pr in p.Resources)
                {
                    if (pr.resourceName.Equals("ElectricCharge") && pr.flowState)
                    {
                        if (pr.amount > 0.04)
                        {
                            return true;
                        } 
                    }
                }
            }

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
            if (biome_desc == "")
            {
                biome_desc = ActiveVessel.mainBody.name;
            }
           
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
                    if (!removed)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                print("Biomatic - RemoveCurrentBody(): " + ex.Message);
                Log.dbg("Biomatic - RemoveCurrentBody(): " + ex.Message);
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
                    if (!removed)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                print("Biomatic - RemoveCurrentBiomeFromList(): " + ex.Message);
                Log.dbg("Biomatic - RemoveCurrentBiomeFromList(): " + ex.Message);
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
                output +=" \u2713";
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
            return AudioClip.Create("beep", 4096, 1, 44100, false, false, OnAudioRead, OnAudioSetPosition);
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
    }
}
