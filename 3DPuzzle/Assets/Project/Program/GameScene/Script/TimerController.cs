using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**
* @file TimerController.cs
* @brief タイマーを扱うファイル
* @author Kodai Nakata
*/

/**
 * @class TimerController
 * @brief タイマーを扱うクラス
 */
public class TimerController : MonoBehaviour
{
    private int hour;// 時
    private int minute;// 分
    private float second;// 秒

    private float oldSecond;// 前の秒数
    private Text timerText;// タイマー表示用テキスト

    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        hour = 0;
        minute = 0;
        second = 0f;
        oldSecond = 0f;
        timerText = GetComponent<Text>();
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        second += Time.deltaTime;
        // 60秒以上のとき
        if (second >= 60f)
        {
            minute++;// 1分増やす
            second = second - 60;// 秒数をリセット
        }
        // 60分以上のとき
        if (minute >= 60)
        {
            hour++;// 1時増やす
            minute = 0;// 分をリセット
        }
        // 更新前の秒数と更新後の秒数が違うとき
        if ((int)second != (int)oldSecond)
        {
            timerText.text = hour.ToString("00") + ":" + minute.ToString("00") + ":" + ((int)second).ToString("00");// タイマーを更新
        }
        oldSecond = second;// 更新前の秒数に格納
    }
}
