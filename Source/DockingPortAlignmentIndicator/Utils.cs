using KSP.Localization;
using UnityEngine;

namespace NavyFish.DPAI
{
    public class Utils
    {
        // Return the localised string from tag f_tag.
        // If there is no localisation for f_tag, return f_default. If f_default is invalid,
        // return f_tag.
        public static string GetStringByTag(string f_tag, string f_default = "")
        {
            string s = f_default;

            Debug.Assert(!string.IsNullOrWhiteSpace(f_tag));

            bool ok = Localizer.TryGetStringByTag(f_tag, out s);
            if (!ok) {
                LogWrapper.LogW("[DPAI.Utils] Warning: localisation string missing - " + f_tag);
                s = string.IsNullOrWhiteSpace(f_default) ? f_tag : f_default;
            }

            return s;
        }
    }
}
