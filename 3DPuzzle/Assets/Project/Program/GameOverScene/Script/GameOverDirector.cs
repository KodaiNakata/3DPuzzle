using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
