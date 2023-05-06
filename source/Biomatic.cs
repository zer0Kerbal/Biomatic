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
using KSP.Localization;
using UnityEngine.Networking;
using Biomatic.Extensions;
using System.Linq;

namespace Biomatic
{
    class Biomatic : PartModule
    {
#region variables/constants
        /// <summary>Module information shown in editors</summary>
        private string info = string.Empty;

        /// <summary>ElectricCharge identification number</summary>
        public static int ElectricChargeID;

        public static bool _BiomaticIsEnabled = true;
        public static bool BiomaticIsEnabled
        {
            get { return _BiomaticIsEnabled; }
            set { _BiomaticIsEnabled = BiomaticIsEnabled; }
        }

        public float ECresourceConsumptionRate = 0.05f;

#endregion
#region Events Actions

        [KSPAction("Biomatic: Toggle")]
        public void toggleAction(KSPActionParam kap)
        { BiomaticIsEnabled = !BiomaticIsEnabled; }

        [KSPAction("Biomatic: Enable")]
        public void enableAction(KSPActionParam kap)
        { BiomaticIsEnabled = true; }

        [KSPAction("Biomatic: Disable")]
        public void disableAction(KSPActionParam kap)
        { BiomaticIsEnabled = false; }

#endregion Events Actions
#region Private functions

        // private static bool IsPrimary = true;
        private static bool _isPrimary = true;
        internal static bool IsPrimary
        {
            get { return _isPrimary; }
            set { _isPrimary = IsPrimary; }
        }

#endregion
#region Mono

        /// <summary>Called when part is added to the craft.</summary>
        public override void OnAwake()
        {
            // Log.Info(String.Format("OnAwake for {0}", name));
        }

        public void Start()
        {   
            //Log.Info("ModuleBiomatic.OnStart");
            if (ElectricChargeID == default(int))
                ElectricChargeID = PartResourceLibrary.Instance.GetDefinition("ElectricCharge").id;
        }

        public override void OnFixedUpdate()
        {
            if (BiomaticIsEnabled && HighLogic.CurrentGame.Parameters.CustomParams<Options>().UseEC)
            {
                if (ConsumeEC(TimeWarp.fixedDeltaTime) == false)
                {
                    ScreenMessages.PostScreenMessage(Localizer.Format("#BIO-part-EC01")); // "Electric Charge Depleted. Stopping Biomatic Scanning."
                    //Log.Info("OnFixedUpdate: Electric Charge Depleted. Stopping Biomatic Scanning.");
                }
            }
            base.OnFixedUpdate();
        }

        public override void OnUpdate()
        { }

        public bool ConsumeEC(double elapsed)
        {
            //Log.Info(String.Format("ConsumeEC : elapsed: {0}", elapsed.ToString()));
            double ec = 0, amount = 0;
            if (CheatOptions.InfiniteElectricity == true) { 
                //Log.Info(String.Format("CheatOptions.InfiniteElectricity({0})", CheatOptions.InfiniteElectricity.ToString()));
                return true;
            }
            else foreach (Part part in FlightGlobals.ActiveVessel.parts)
                    foreach (PartResource res in part.Resources)
                        if (res.resourceName == "ElectricCharge" && res.amount > 0)
                        {
                            //Log.Info(String.Format("part {0}.{1}:{2}]", part.name, res.resourceName, res.amount));
                            ec += res.amount;  // tally total EC available on ship
                            //Log.Info(String.Format("total EC available {0} ]", ec.ToString()));
                        }

            amount = ECresourceConsumptionRate * TimeWarp.fixedDeltaTime;
            //Log.Info(String.Format("EC available: {0} / Consumption Rate: {1} / fixedDeltaTime {2}", ec.ToString(), ECresourceConsumptionRate.ToString(), TimeWarp.fixedDeltaTime.ToString()));
            // if not enough EC to power, then SHut.It.Down
            if (ec < amount) return false;

            //? compute consumption
            //? don't forget to consume the EC needed to power this beast
            part.RequestResource(ElectricChargeID, amount);
            return true;
        }

        public bool IsPowered
        {
            get
            {
                foreach (Part p in FlightGlobals.ActiveVessel.parts)
                    foreach (PartResource pr in p.Resources)
                        if (pr.resourceName.Equals("ElectricCharge") && pr.flowState)
                            if (pr.amount >= ECresourceConsumptionRate) return true;

                return false;
            }
        }
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
#endregion
#region GetInfo
        /// <summary>Formats the information for the part information in the editors.</summary>
        /// <returns>info</returns>
        public override string GetInfo()
        {
            //? this is what is show in the editor
            //? As annoying as it is, pre-parsing the config MUST be done here, because this is called during part loading.
            //? The config is only fully parsed after everything is fully loaded (which is why it's in OnStart())
            if (info == string.Empty)
            {
                info += Localizer.Format("#BIFF-Agency-titl") + " v" + Version.SText; // Biomatic Version Number text
                info += "\n<color=#b4d455FF>" + Localizer.Format("#BIO-desc"); // #BIO-desc = In-flight biome identifier
                info += "\n\n<color=orange>Requires:</color><color=#FFFFFFFF> \n- <b>" + Localizer.Format("#autoLOC_252004"); // #autoLOC_252004 = ElectricCharge
                info += "</b>: </color><color=#99FF00FF>" + RateString(ECresourceConsumptionRate) + "</color>";
            }
            return info;
        }
#endregion
    }
}