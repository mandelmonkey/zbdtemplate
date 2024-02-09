using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationSetting : MonoBehaviour
{
    public GameObject NotificationInfo;
    private void OnEnable()
    {
        StartCoroutine(ClosePanel());
    }
    public void Awake()
    {
        if(PlayerPrefs.HasKey("EarnMode"))
        {
            if( NotificationInfo != null)
            {
                NotificationInfo.SetActive(false);
            }
          
        }
        else
        {
          
            if (NotificationInfo != null)
            {
               
                NotificationInfo.SetActive(true);
            }
        }
    }
    IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(.5f);
        gameObject.SetActive(false);
    }


}
