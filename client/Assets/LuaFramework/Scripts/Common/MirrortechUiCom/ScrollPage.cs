using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ScrollPage : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    ScrollRect rect;
    List<float> pages = new List<float>();
    int currentPageIndex = 0;
    float ratio = 0.2F;
    float page_width = 1;

    public float smooting = 4;

    float targethorizontal = 0;

    bool isDrag = false;
    bool isInit = false;

    public System.Action<int,int> OnPageChanged;

    float startime = 0f;
    float delay = 0.1f;

    void Start()
    {
        rect = transform.GetComponent<ScrollRect>();    
        startime = Time.time;
    }
    
    void Update()
    {
        if (Time.time < startime + delay) return;
        UpdatePages();
        if (!isDrag && pages.Count>0)
        {
            rect.horizontalNormalizedPosition = Mathf.Lerp(rect.horizontalNormalizedPosition, targethorizontal, Time.deltaTime * smooting);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;

        float posX = rect.horizontalNormalizedPosition;
        float offset = pages[currentPageIndex] - posX;
        int index = currentPageIndex;
        if (offset > 0 && currentPageIndex <= 0){
            return;
        }
        if (offset < 0 && currentPageIndex >= pages.Count - 1){
            return;
        }
        if (offset < 0 && offset < - page_width * ratio){
            index = currentPageIndex + 1;
        }
        if (offset > 0 && offset > page_width * ratio){
            index = currentPageIndex - 1;
        }
        // int index = 0;
        // float offset = Mathf.Abs(pages[index] - posX);
        // for (int i = 1; i < pages.Count; i++)
        // {
        //     float temp = Mathf.Abs(pages[i] - posX);
        //     if (temp < offset)
        //     {
        //         index = i;
        //         offset = temp;
        //     }
        // }

        if(index!=currentPageIndex)
        {
            currentPageIndex = index;
            Debug.Log(pages.Count);
            OnPageChanged(pages.Count, currentPageIndex);
        }


        targethorizontal = pages[index];
    }

    void UpdatePages()
    {
        int count = this.rect.content.childCount;
        int temp = 0;
        for(int i=0; i<count; i++)
        {
            if(this.rect.content.GetChild(i).gameObject.activeSelf)
            {
                temp++;
            }
        }
        count = temp;
        if (count != 1)
        {
            page_width = 1 / ((float)(count - 1));
        }
        
        if (pages.Count!=count)
        {
            if (count != 0)
            {
                pages.Clear();
                for (int i = 0; i < count; i++)
                {
                    float page = 0;
                    if(count!=1)
                        page = i / ((float)(count - 1));
                    pages.Add(page);
                }
            }
            OnEndDrag(null);
        }
        if (!isInit)
        {
            OnPageChanged(pages.Count, 0);
            isInit = true;
        }
    }
}
