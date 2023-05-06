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
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

// This will add a tab to the Stock Settings in the Difficulty settings called "On Demand Fuel Cells"
// To use, reference the setting using the following:
//
//  HighLogic.CurrentGame.Parameters.CustomParams<Options>().consumeEC
//
// As it is set up, the option is disabled, so in order to enable it, the player would have
// to deliberately go in and change it
//
namespace Biomatic
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class Options : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Default Settings"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Biomatic"; } }
        public override string DisplaySection { get { return "Biomatic"; } }
        public override int SectionOrder { get { return 1; } }


        [GameParameters.CustomParameterUI("Consumes ElectricCharge",
            toolTip = "if set to yes, Biomatic consumes Electric Charge when enabled and on.",
            newGameOnly = false,
            unlockedDuringMission = true
            )]
        public bool UseEC = true;

        [GameParameters.CustomParameterUI("hide UI when game is paused",
            toolTip = "hide Biomatic UI when game is paused.",
            newGameOnly = false,
            unlockedDuringMission = true)]
        public bool _hideWhenPaused = true;
        public bool hideWhenPaused { get { return _hideWhenPaused; } }

        [GameParameters.CustomParameterUI("Use alternate skin",
            toolTip = "Use a more minimiliast skin")]
        public bool _useAlternateSkin = false;
        public bool useAlternateSkin { get { return _useAlternateSkin; } }

        [GameParameters.CustomParameterUI("PAW Color",
            toolTip = "allow color coding in Biomatic PAW (part action window) / part RMB (right menu button).",
            newGameOnly = false,
            unlockedDuringMission = true)]
        public bool coloredPAW = true;

        // If you want to have some of the game settings default to enabled,  change 
        // the "if false" to "if true" and set the values as you like


#if true
        public override bool HasPresets { get { return true; } }
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    UseEC = false;
                    break;

                case GameParameters.Preset.Normal:
                    UseEC = true;
                    break;

                case GameParameters.Preset.Moderate:
                    UseEC = true;
                    break;

                case GameParameters.Preset.Hard:
                    UseEC = true;
                    break;
            }
        }

#else
        public override bool HasPresets { get { return false; } }
        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
#endif

        public override bool Enabled(MemberInfo member, GameParameters parameters) { return true; }
        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        public override IList ValidValues(MemberInfo member) { return null; }
    }
}