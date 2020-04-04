using System;
using System.Collections;
using UnityEngine;

namespace PatchLoaderMod
{
    public static class CoroutineHelper
    {
        public static void WaitFor(Func<bool> predicate, Action success, Action failure, float stopPollingAfterInSec = 60f, float pollRateInSec = 1f)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (success is null)
            {
                throw new ArgumentNullException(nameof(success));
            }

            if (failure is null)
            {
                throw new ArgumentNullException(nameof(failure));
            }
            
            var go = new GameObject();
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<EmptyMonoBehaviour>()
                .StartCoroutine(
                    WaitForCoroutine(predicate, success, failure, pollRateInSec, stopPollingAfterInSec, go)
                );
        }

        private static IEnumerator WaitForCoroutine(Func<bool> predicate, Action success, Action failure, float pollInSec, float stopPollingAfterInSec, GameObject go)
        {
            var startTime = Time.time;
            var stopped = false;

            while (!predicate())
            {
                if (Time.time - startTime >= stopPollingAfterInSec)
                {
                    stopped = true;
                    break;
                }

                yield return new WaitForSeconds(pollInSec);
            }

            if (stopped)
            {
                failure();
            }
            else
            {
                success();
            }

            GameObject.Destroy(go);
        }

        private class EmptyMonoBehaviour : MonoBehaviour
        {
        }
    }
}
