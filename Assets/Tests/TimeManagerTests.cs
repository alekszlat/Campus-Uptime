/* UnitTests for the TimeManager
 * Tested Features:
 *     • Singleton creation and lifecycle management
 *     • Reset behavior (restores starting time)
 *     • AddTime / SubtractTime logic
 *     • Time countdown per frame (Update loop)
 *     • Pause and Resume functionality
 *
 * Author: H. Hristov (milkeles)
 * Created: 09/11/2025 (dd/mm/yyyy)
 * Updated: 09/11/2025 (dd/mm/yyyy)
 * Changelog:
*/
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Game.Core.TimeSystem;
using System.Collections;

namespace Tests.Core.TimeSystem
{
    public class TimeManagerTests
    {
        [UnityTest]
        public IEnumerator TimeManager_Creates_Instance_Automatically()
        {
            var existing = Object.FindFirstObjectByType<TimeManager>();
            if (existing)
                Object.DestroyImmediate(existing.gameObject);

            var instance = TimeManager.Instance;

            yield return null;

            Assert.NotNull(instance);
            Assert.AreEqual("TimeManager", instance.name);
        }

        [UnityTest]
        public IEnumerator TimeManager_ResetTimesCorrectly()
        {
            var instance = TimeManager.Instance;
            instance.ResetTime();
            var time = instance.RemainingTime;

            Assert.AreEqual(15.0f, time, 0.001f, "Reset time should reset back to starting time.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TimeManager_AddsTime()
        {
            var instance = TimeManager.Instance;
            instance.ResetTime();
            instance.Pause(); // Pause or it decreases in the meantime and fails the test.

            var startTime = instance.RemainingTime;
            float amount = 5.43f;

            instance.AddTime(amount);
            yield return null;

            var newTime = instance.RemainingTime;
            Assert.AreEqual(startTime + amount, newTime, 0.001f, "Time should be added when AddTime is called.");

            instance.Resume();
            instance.ResetTime();
        }


        [UnityTest]
        public IEnumerator TimeManager_SubstractsTime()
        {
            var instance = TimeManager.Instance;
            instance.ResetTime(); // Ensure enough time available for test.
            instance.Pause();

            var startTime = instance.RemainingTime;

            float amount = 5.43f;
            instance.SubtractTime(amount);

            yield return null;

            var newTime = instance.RemainingTime;
            Assert.AreEqual(startTime - amount, newTime, 0.001f, "Time should be removed when SubstractTime is called.");

            instance.ResetTime(); // Set back to 15 to ensure other tests don't break because of singleton if it runs out of time.
        }

        [UnityTest]
        public IEnumerator TimeManager_Decreases_RemainingTime_Each_Frame()
        {
            var instance = TimeManager.Instance;
            instance.ResetTime();
            instance.Resume();

            var startTime = instance.RemainingTime;

            yield return new WaitForSeconds(2.0f);

            var newTime = instance.RemainingTime;

            Assert.Less(newTime, startTime, "RemainingTime should decrease every frame while running.");
        }

        [UnityTest]
        public IEnumerator TimeManager_Pause_Stops_Countdown()
        {
            var instance = TimeManager.Instance;
            instance.ResetTime();
            instance.Resume();

            yield return new WaitForSeconds(0.3f);
            instance.Pause();
            var timeBeforePause = instance.RemainingTime;
            yield return new WaitForSeconds(0.3f);
            var timeAfterPause = instance.RemainingTime;

            Assert.AreEqual(timeBeforePause, timeAfterPause, 0.001f, "Time should not decrease while paused.");
        }

        [UnityTest]
        public IEnumerator TimeManager_Resume_Continues_Countdown()
        {
            var instance = TimeManager.Instance;
            instance.ResetTime();
            instance.Pause();
            var timeBeforeResume = instance.RemainingTime;

            instance.Resume();
            yield return new WaitForSeconds(0.3f);
            var timeAfterResume = instance.RemainingTime;
            Assert.Less(timeAfterResume, timeBeforeResume, "Time should decrease after resuming.");
        }
    }
}