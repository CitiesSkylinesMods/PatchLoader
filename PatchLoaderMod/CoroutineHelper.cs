using System;
using System.Collections;
using UnityEngine;

namespace PatchLoaderMod
{
    public static class CoroutineHelper
    {
        public static void WaitFor(Func<bool> predicate, float pollInSec, Action action)
        {
            var mb = GameObject.FindObjectOfType<MonoBehaviour>();
            mb.StartCoroutine(WaitForCoroutine(predicate, pollInSec, action));
        }

        private static IEnumerator WaitForCoroutine(Func<bool> predicate, float pollInSec, Action callback)
        {
            while (!predicate())
            {
                yield return new WaitForSeconds(pollInSec);
            }

            callback();
        }
    }
}
