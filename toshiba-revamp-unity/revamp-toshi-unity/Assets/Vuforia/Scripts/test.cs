using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : DefaultTrackableEventHandler {

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        Debug.Log("gg");
    }
}
