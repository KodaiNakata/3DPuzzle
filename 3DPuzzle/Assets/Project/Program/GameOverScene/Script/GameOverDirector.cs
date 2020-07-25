using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/**
* @file GameOverDirector.cs
* @brief ゲームオーバー画面の監督スクリプトのファイル
* @author Kodai Nakata
*/

/**
 * @class GameOverDirector
 * @brief ゲームオーバー画面の監督スクリプト用のクラス
 */
public class GameOverDirector : MonoBehaviour
{
    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        Text resultTimer = GameObject.FindGameObjectWithTag("Result").GetComponent<Text>();// 時間の結果のテキストを取得
        resultTimer.text = "Result " + TimerController.getResultOfTime();// 時間の結果を表示する
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        // エンターキーが押されたとき
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("StartScene");// スタート画面へ遷移
        }
        
    }
}
