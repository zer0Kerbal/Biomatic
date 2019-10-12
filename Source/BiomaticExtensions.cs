using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biomatic.Extensions
{
    public static class BiomaticExtensions
    {
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
}
