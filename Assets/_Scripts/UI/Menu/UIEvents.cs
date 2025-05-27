using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class UIEvents
{
    // Static event for opening death screen
    public static event Action OnOpenDeathScreen;

    // Static method to trigger the death screen event
    public static void OpenDeathScreen()
    {
        OnOpenDeathScreen?.Invoke();
    }
}
