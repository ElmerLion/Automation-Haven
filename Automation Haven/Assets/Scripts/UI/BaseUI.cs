using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BaseUI : MonoBehaviour {
    
    public bool isOpen { get; protected set; }


    public virtual void Show() {
        gameObject.SetActive(true);
        isOpen = true;

        UIManager.Instance.CloseOtherUIs(this);
        if (AutomationGameManager.Instance != null) {
            AutomationGameManager.Instance.CheckPauseState();
        }
    }
    
    public virtual void Hide() {
        gameObject.SetActive(false);
        isOpen = false;

        if (AutomationGameManager.Instance != null) {
            AutomationGameManager.Instance.CheckPauseState();
        }
        if (InterfaceToolTipUI.Instance != null) {
            InterfaceToolTipUI.Instance.Hide();
        }


    }

}
