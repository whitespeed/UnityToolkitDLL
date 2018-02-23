using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extension
{
    public abstract class UIHandler<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Get(GameObject o)
        {
            var handler = o.GetComponent<T>();
            if(null == handler)
            {
                handler = o.AddComponent<T>();
            }
            return handler;
        }
    }

    public class UIClickHandler : UIHandler<UIClickHandler>, IPointerClickHandler
    {
        public Action<PointerEventData> OnClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            if(OnClick != null)
            {
                OnClick(eventData);
            }
        }
    }

    public class UIEnterExitHandler : UIHandler<UIEnterExitHandler>, IPointerEnterHandler, IPointerExitHandler
    {
        public Action<PointerEventData> OnExit;
        public Action<PointerEventData> OnEnter;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(OnEnter != null)
            {
                OnEnter(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(OnExit != null)
            {
                OnExit(eventData);
            }
        }
    }

    public class UIDownUpHandler : UIHandler<UIDownUpHandler>, IPointerDownHandler, IPointerUpHandler
    {
        public Action<PointerEventData> OnDown;
        public Action<PointerEventData> OnUp;

        public void OnPointerDown(PointerEventData eventData)
        {
            if(null != OnDown)
            {
                OnDown(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if(null != OnUp)
            {
                OnUp(eventData);
            }
        }
    }
}