using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AcceptSelectionContractButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Animator animator;

    private void Start() {
        animator = transform.parent.GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        animator.SetBool("IsHovering", true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        animator.SetBool("IsHovering", false);
    }
}
