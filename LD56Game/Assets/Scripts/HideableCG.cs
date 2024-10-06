using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideableCG : MonoBehaviour
{
    [SerializeField] bool startVisible = false;
    public bool isVisible;
    CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (startVisible) Show();
        else Hide();
    }

    public void Show()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        isVisible = false;
    }

    public void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        isVisible= false;
    }

    public void Toggle()
    {
        if (cg.alpha > 0.8f){ Hide(); }
        else { Show(); }
    }
}
