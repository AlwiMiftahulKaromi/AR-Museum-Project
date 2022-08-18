using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public TabButton selectedTab;
    public List<GameObject> objectToSwap;
    public GameObject closeButton;


    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);

        button.background.sprite = tabIdle;
    }

    public void OnTabEnter(TabButton button)
    {
        ResetTab();
        if (selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {
        ResetTab();
    }

    public void OnTabSelected(TabButton button)
    {
        closeButton.SetActive(true);
        selectedTab = button;
        ResetTab();
        button.background.sprite = tabActive;
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < objectToSwap.Count; i++)
        {
            if (i == index)
            {
                objectToSwap[i].SetActive(true);
            }
            else
            {
                objectToSwap[i].SetActive(false);
            }
        }
    }

    public void ResetTab()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }
            button.background.sprite = tabIdle;
        }
    }

    public void ClosePanel()
    {
        closeButton.SetActive(false);
        foreach (TabButton button in tabButtons)
        {
            button.background.sprite = tabIdle;
        }
        foreach (GameObject gameObject in objectToSwap)
        {
            gameObject.SetActive(false);
        }
    }
}
