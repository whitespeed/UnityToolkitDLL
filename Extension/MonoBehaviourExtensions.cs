using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extension
{
    public static class MonoBehaviourExtensions
    {
        /**
                Invokes the given action after the given amount of time.

                The action must be a method of the calling class.
            */
        public static void Invoke(this MonoBehaviour component, System.Action action, float time)
        {
            component.Invoke(action.Method.Name, time);
        }

        public static IEnumerator NextFrameAction(System.Action action)
        {
            yield return 1;
            action();
        }

        public static void InvokeNextFrame(this MonoBehaviour component, System.Action action)
        {
            component.StartCoroutine(NextFrameAction(action));
        }
        /**
                Invokes the given action after the given amount of time, and repeats the 
                action after every repeatTime seconds.

                The action must be a method of the calling class.
            */
        public static void InvokeRepeating(this MonoBehaviour component, System.Action action, float time, float repeatTime)
        {
            component.InvokeRepeating(action.Method.Name, time, repeatTime);
        }

        /**
                Invokes an action after a random time between the minimum and 
                maximum times given.
            */
        public static void InvokeRandom(this MonoBehaviour component, System.Action action, float minTime, float maxTime)
        {
            var time = UnityEngine.Random.value * (maxTime - minTime) + minTime;

            component.Invoke(action, time);
        }

        /**
                Cancels the action if it was scheduled.
            */
        public static void CancelInvoke(this MonoBehaviour component, System.Action action)
        {
            component.CancelInvoke(action.Method.Name);
        }

        /**
                Returns whether an invoke is pending on an action.
            */
        public static bool IsInvoking(this MonoBehaviour component, System.Action action)
        {
            return component.IsInvoking(action.Method.Name);
        }


        public static GameObject FindChild(this MonoBehaviour component, string childName)
        {
            return component.transform.FindChild(childName).gameObject;
        }

        public static GameObject FindChild(this MonoBehaviour component, string childName, bool recursive)
        {
            if (recursive) return component.FindChild(childName);

            return FindChildRecursively(component.transform, childName);
        }

        private static GameObject FindChildRecursively(Transform target, string childName)
        {
            if (target.name == childName) return target.gameObject;

            for (var i = 0; i < target.childCount; ++i)
            {
                var result = FindChildRecursively(target.GetChild(i), childName);

                if (result != null) return result;
            }

            return null;
        }

        /**
                Finds a component of the type s2 in on the same object, or on a child down the hierarchy. This method also works
                in the editor and when the game object is inactive.

                @version_e_1_1

            */
        public static T GetComponentInChildrenAlways<T>(this MonoBehaviour component) where T : MonoBehaviour
        {
            foreach (var child in component.transform.SelfAndAllChildren())
            {
                var componentInChild = child.GetComponent<T>();

                if (componentInChild != null)
                {
                    return componentInChild;
                }
            }

            return null;
        }

        /**
                Finds all components of the type s2 on the same object and on a children down the hierarchy. This method also works
                in the editor and when the game object is inactive.

                @version_e_1_1
            */
        public static T[] GetComponentsInChildrenAlways<T>(this MonoBehaviour component) where T : MonoBehaviour
        {
            var components = new List<T>();

            foreach (var child in component.transform.SelfAndAllChildren())
            {
                var componentsInChild = child.GetComponents<T>();

                if (componentsInChild != null)
                {
                    components.AddRange(componentsInChild);
                }
            }

            return components.ToArray();
        }

        /**
                The same as GetComponent, but logs an error if the component is not found.
            */
        public static T GetRequiredComponent<T>(this MonoBehaviour thisComponent) where T : Component
        {
            var component = thisComponent.GetComponent<T>();

            if (component == null)
            {
                Debug.LogError("Expected to find component of type " + typeof(T) + " but found none", thisComponent.gameObject);
            }

            return component;
        }

        /**
                Gets an attached component that implements the interface of the type parameter.
            */
        public static I GetInterfaceComponent<I>(this MonoBehaviour thisComponent) where I : class
        {
            return thisComponent.GetComponent(typeof(I)) as I;
        }

    }

}

