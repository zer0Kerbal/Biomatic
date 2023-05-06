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

namespace Biomatic
{
    class BiomeSituation
    {
        private string biome = "";
        public string Biome
        {
            get { return biome; }
            set { biome = value; }
        }

        private string situation = "";
        public string Situation
        {
            get { return situation; }
            set { situation = value; }
        }

        private string body = "";
        public string Body
        {
            get { return body; }
            set { body = value; }
        }
/*
        private string vessel = "";
        public string Vessel
        {
            get { return vessel; }
            set { vessel = value; }
        }
*/
        private bool listed = false;
        public bool Listed
        {
            get { return listed; }
            set { listed = value; }
        }

        private Guid vesselGUID;
        public Guid VesselGUID
        {
            get { return vesselGUID; }
            set { vesselGUID = value; }
        }

        public BiomeSituation()
        { 
        
        }

        public BiomeSituation(string bio, string sit, string bod, Guid GUID)
        {
            biome = bio;
            situation = sit;
            body = bod;
            vesselGUID = GUID;
        }

        public BiomeSituation(string bio, string sit, string bod)
        {
            biome = bio;
            situation = sit;
            body = bod;
        }

        public BiomeSituation(string desc)
        {
            string[] parts = desc.Split(new char[] { '.' });
            if (parts != null && parts.Length > 2)
            {
                situation = parts[0];
                body = parts[1];
                biome = parts[2];

                if (parts.Length > 3)
                {
                    vesselGUID = new Guid(parts[3]);
                }
            }
        }

        public BiomeSituation(BiomeSituation bs)
        { 
            biome = bs.biome;
            situation = bs.situation;
            body = bs.body;
            listed = bs.listed;
        }

        public bool IsSameAs(BiomeSituation bs, bool useSituation)
        {
            if (this.body.CompareTo(bs.body) != 0)
            {
                return false;
            }

            if (this.biome.CompareTo(bs.biome) != 0)
            {
                return false;
            }

            if (useSituation && this.situation.CompareTo(bs.situation) != 0)
            {
                return false;
            }

            return true;
        }

        public bool IsSameAs(BiomeSituation bs, bool useSituation, bool useVessel)
        {
            if (useVessel)
            {
                if (this.vesselGUID != bs.vesselGUID)
                {
                    return false;
                }
            }

            if (this.body.CompareTo(bs.body) != 0)
            {
                return false;
            }

            if (this.biome.CompareTo(bs.biome) != 0)
            {
                return false;
            }

            if (useSituation && this.situation.CompareTo(bs.situation) != 0)
            {
                return false;
            }

            return true;
        }
        public string GetText(bool useSituation, bool useVessel)
        {
            string s = body + "." + biome;

            if (useSituation)
            {
                s = situation + "." + s;
            }

            if (useVessel)
            {
                s = s + "." + vesselGUID.ToString();
            }

            return s;
        }

        public string GetDescription(bool useSituation)
        { 
            string s = biome;

            if (useSituation)
            {
                s = situation + ", " + s;
            }

            return s;
        }
    }
}