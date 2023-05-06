﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Biomatic.Extensions
{
	/// <summary>class Biomatic Extensions</summary>
	public static class BiomaticExtensions
    {
        /// <summary>IsPrimary</summary>
        public static bool IsPrimary(this Part thisPart, List<Part> partsList, int moduleClassID)
		{
			foreach (Part part in partsList)
			{
				if (part.Modules.Contains(moduleClassID))
				{
					if (part == thisPart)
					{
						return true;
					}
					else
					{
						break;
					}
				}
			}

			return false;
		}
	}

    /// <summary>RectExtensions</summary>
    // [KSPAddon(KSPAddon.Startup.Flight, false)]
    public static class RectExtensions
	{
        /// <summary>CenterScreen</summary>
        public static Rect CenterScreen(this Rect thisRect)
		{
			if (Screen.width > 0 && Screen.height > 0
				&& thisRect.width > 0f && thisRect.height > 0f)
			{
				thisRect.x = Screen.width / 2 - thisRect.width / 2;
				thisRect.y = Screen.height / 2 - thisRect.height / 2;
			}

			return thisRect;
		}
	}
}