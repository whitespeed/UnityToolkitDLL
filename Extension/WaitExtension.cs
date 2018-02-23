using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extension
{
    public partial class WaitExtension
    {
        public static WaitForSeconds Wait1Second = new WaitForSeconds(1);
        public static WaitForSeconds Wait2Seconds = new WaitForSeconds(2);
        public static WaitForSeconds Wait3Seconds = new WaitForSeconds(3);
        public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        public static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    }

}

