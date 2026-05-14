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

        #region Control Lock

        private static string CONTROL_LOCK_ID = "DPAI_SettingsWindow";

        public static bool PreventClickthrough(bool isVisible, Rect position, bool isLocked)
        {
            bool mouseOverWindow = isVisible && IsMouseOverRect(position);
            if (!isLocked && mouseOverWindow)
            {
                InputLockManager.SetControlLock( ControlTypes.ALLBUTCAMERAS, CONTROL_LOCK_ID);
                isLocked = true;
            }

            if (!isLocked || mouseOverWindow)
            {
                return isLocked;
            }

            InputLockManager.RemoveControlLock(CONTROL_LOCK_ID);
            isLocked = false;
            return false;
        }

        private static bool IsMouseOverRect(Rect position)
        {
            return position.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        }

        #endregion
    }
}
