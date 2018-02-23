using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    public class MsgDispatcher<TKey, TValue>
    {
        protected Dictionary<TKey, List<Action<TValue>>> listeners = new Dictionary<TKey, List<Action<TValue>>>();
        public void AddListener(TKey msg, Action<TValue> listener)
        {
            if(null == listener)
                return;
            if (!listeners.ContainsKey(msg))
            {
                var list = new List<Action<TValue>> {listener};
                listeners[msg] = list;
            }
            else
            {
                listeners[msg].Add(listener);
            }
        }

        public bool ContainsKey(TKey msg)
        {
            return listeners.ContainsKey(msg);
        }

        public void RemoveListner(TKey msg, Action<TValue> listener)
        {
            if (listener != null && listeners.ContainsKey(msg) )
            {
                var list = listeners[msg];
                listeners.Remove(msg);
            }
        }

        public void ClearListeners(TKey msg)
        {
            if (listeners.ContainsKey(msg))
            {
                listeners[msg].Clear();
            }
        }

        public virtual void DispathMsg(TKey msg, TValue context)
        {
            if (listeners.ContainsKey(msg))
            {
                var list = listeners[msg];
                for(int i = 0; i < list.Count; i++)
                {
                    list[i](context);
                }
            }
        }

        public void ClearAll()
        {
            listeners.Clear();
        }
    }
    public class BlockDispatcher<TKey, TValue>
    {
        protected Dictionary<TKey, List<Func<TValue, bool>>> listeners = new Dictionary<TKey, List<Func<TValue, bool>>>();
        public void AddListener(TKey msg, Func<TValue, bool> listener)
        {
            if (null == listener)
                return;
            if (!listeners.ContainsKey(msg))
            {
                var list = new List<Func<TValue, bool>> { listener };
                listeners[msg] = list;
            }
            else
            {
                listeners[msg].Add(listener);
            }
        }

        public bool ContainsKey(TKey msg)
        {
            return listeners.ContainsKey(msg);
        }

        public void RemoveListner(TKey msg, Action<TValue> listener)
        {
            if (listener != null && listeners.ContainsKey(msg))
            {
                var list = listeners[msg];
                listeners.Remove(msg);
            }
        }

        public void ClearListeners(TKey msg)
        {
            if (listeners.ContainsKey(msg))
            {
                listeners[msg].Clear();
            }
        }

        public virtual void DispathMsg(TKey msg, TValue context)
        {
            if (listeners.ContainsKey(msg))
            {
                var list = listeners[msg];
                for (int i = 0; i < list.Count; i++)
                {
                    //jump out if result is no OK.
                    if(!list[i](context))
                        break;
                }
            }
        }

        public void ClearAll()
        {
            listeners.Clear();
        }
    }
}

