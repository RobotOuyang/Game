using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IPHONE
using UnityEngine.iOS;
using LocalNotification = UnityEngine.iOS.LocalNotification;
using NotificationServices = UnityEngine.iOS.NotificationServices;
using CalendarIdentifier = UnityEngine.iOS.CalendarIdentifier;
using CalendarUnit = UnityEngine.iOS.CalendarUnit;
using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public class PushManager : MonoBehaviour
{
#if UNITY_IPHONE
    List<NotiFicationData> push_quque = new List<NotiFicationData>();
#endif

    //本地推送
    public void NotificationMessage(string message, int hour, int minute, bool isRepeatDay)
    {
#if UNITY_IPHONE
        int year = System.DateTime.Now.Year;
        int month = System.DateTime.Now.Month;
        int day = System.DateTime.Now.Day;
        System.DateTime newDate = new System.DateTime(year, month, day, hour, minute, 0);
        IOSNotificationMessage(message, newDate, isRepeatDay);
#endif
    }
    //本地推送 你可以传入一个固定的推送时间
    public void IOSNotificationMessage(string message, System.DateTime newDate, bool isRepeatDay)
    {
        //推送时间需要大于当前时间, 重复推送的 就把时间定到明天
        if (newDate <= System.DateTime.Now)
        {
            if (isRepeatDay)
            {
                newDate.AddDays(1);
            }
            else
            {
                return;
            }
        }
#if UNITY_IPHONE
        LocalNotification localNotification = new LocalNotification();
        localNotification.fireDate = newDate;
        localNotification.alertBody = message;
        localNotification.hasAction = true;
        localNotification.applicationIconBadgeNumber = 1;
        if (isRepeatDay)
        {
            //是否每天定期循环
            localNotification.repeatCalendar = CalendarIdentifier.ChineseCalendar;
            localNotification.repeatInterval = CalendarUnit.Day;
        }
        localNotification.soundName = LocalNotification.defaultSoundName;
        NotificationServices.ScheduleLocalNotification(localNotification);
#endif
    }

    bool tokenSent = false;
    string hexToken;
    void Awake()
    {
        //第一次进入游戏的时候清空，有可能用户自己把游戏冲后台杀死，这里强制清空
        CleanNotification();
#if UNITY_IPHONE
        NotificationServices.RegisterForNotifications(
                NotificationType.Alert |
                NotificationType.Badge |
                NotificationType.Sound);
#endif
    }

    void Update()
    {
#if UNITY_IPHONE
        if (!tokenSent)
        {
            byte[] token = NotificationServices.deviceToken;
            if (token != null)
            {
                // send token to a provider
                hexToken = System.BitConverter.ToString(token);
                tokenSent = true;
            }
        }
#endif
    }

    public string GetDeviceToken()
    {
        return hexToken;
    }

    struct NotiFicationData
    {
        public string message;
        public int hour;
        public int minute;
        public bool isrepeat;
    }

    // 在这里添加的，在进入后台才会推送
    public void AddNotificationMessage(string message, int hour, int minute, bool isRepeatDay)
    {
#if UNITY_IPHONE
        NotiFicationData temp = new NotiFicationData();
        temp.message = message;
        temp.hour = hour;
        temp.minute = minute;
        temp.isrepeat = isRepeatDay;
        push_quque.Add(temp);
#endif
    }

    void OnApplicationPause(bool paused)
    {
        //程序进入后台时
        if (paused)
        {
#if UNITY_IPHONE
            foreach (NotiFicationData data in push_quque)
            {
                NotificationMessage(data.message, data.hour, data.minute, data.isrepeat);
            }
#endif
        }
        else
        {
            //程序从后台进入前台时
            ResetAppBadgeIcon();
            CleanNotification();
        }
    }

    IEnumerator ClearAllNoti()
    {
        //yield return null;
        yield return new WaitForSeconds(3f);
#if UNITY_IPHONE
        NotificationServices.CancelAllLocalNotifications();
        NotificationServices.ClearLocalNotifications();
#endif
    }

    //清空所有本地消息
    void CleanNotification()
    {
#if UNITY_IPHONE
        StartCoroutine(ClearAllNoti());
#endif
    }

    void ResetAppBadgeIcon()
    {
#if UNITY_IPHONE
        NotificationServices.RegisterForNotifications(
                NotificationType.Alert |
                NotificationType.Badge |
                NotificationType.Sound);
        LocalNotification notif2 = new LocalNotification();
        notif2.applicationIconBadgeNumber = -1;
        notif2.hasAction = false;
        notif2.fireDate = System.DateTime.Now.AddSeconds(1);
        NotificationServices.ScheduleLocalNotification(notif2);
#endif
    }
}
