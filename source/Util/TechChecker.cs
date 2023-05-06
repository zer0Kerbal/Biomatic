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
using UnityEngine;

namespace Biomatic
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TechChecker : MonoBehaviour
    {
        private static bool techAvailable = false;
        public static bool TechAvailable
        {
            get { return techAvailable; }
            set { techAvailable = value; }
        }

        private static bool initialized = false;

        private void Start()
        {
            techAvailable = false;

            try
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                {
                    AvailablePart availablePart = PartLoader.getPartInfoByName("Biomatic");

                    if (availablePart != null)
                    {
                        // This looks weird, because we get the part, find the node that it is in, loop through the parts in that node,
                        // and look at PartModelPurchased() when we get to the part we started with. 
                        // Why not just call PartModelPurchased() on the part that we first get? Because it doesn't work, it always returns true.
                        // So this code looks as if it was written by the Department of Redundancy Department, but it is necessary to do it this way.

                        ProtoTechNode techNode = ResearchAndDevelopment.Instance.GetTechState(availablePart.TechRequired);

                        if (techNode != null)
                        {
                            List<AvailablePart> lap = techNode.partsPurchased;

                            foreach (AvailablePart p in lap)
                            {
                                if (p.name == "Biomatic")
                                {
                                    if (ResearchAndDevelopment.PartModelPurchased(p))
                                    {
                                        Initialize();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Initialize();
                }
            }
            catch (Exception ex) { print("Biomatic startup exeption - " + ex.Message); }
        }

        private void Initialize()
        {
            techAvailable = true;

            if (!initialized)
            {
                GameEvents.onHideUI.Add(new EventVoid.OnEvent(OnHideUI));
                GameEvents.onShowUI.Add(new EventVoid.OnEvent(OnShowUI));
                initialized = true;
            }
        }

        private void OnShowUI()
        {
            BiomaticUI.HideUI = false;
        }

        private void OnHideUI()
        {
            BiomaticUI.HideUI = true;
        }
    }
}
