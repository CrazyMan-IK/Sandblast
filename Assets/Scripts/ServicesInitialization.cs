using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public class ServicesInitialization : MonoBehaviour
    {
        private const string _RegistrationDayKey = "_registrationDayKey";

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Amplitude amplitude = Amplitude.Instance;
            //amplitude.setServerUrl("https://api2.amplitude.com");
            amplitude.logging = true;
            amplitude.trackSessionEvents(true);
            amplitude.init("b1ecee4894cff833c0efa9651691c660");

            var now = DateTime.Now;
            if (!PlayerPrefs.HasKey(_RegistrationDayKey))
            {
                PlayerPrefs.SetString(_RegistrationDayKey, now.ToString("dd.MM.yy"));
            }
            var cached = DateTime.ParseExact(PlayerPrefs.GetString(_RegistrationDayKey, now.ToString("dd.MM.yy")), "dd.MM.yy", null);

            amplitude.setOnceUserProperty("reg_day", now.ToString("dd.MM.yy"));
            amplitude.setUserProperty("days_in_game", (now.Date - cached).TotalDays);
        }
    }
}
