using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/**
 * @file StartSceneDirector.cs
 * @brief スタート画面の監督スクリプトのファイル
 * @author Kodai Nakata
 */

/**
 * @class StartSceneDirector
 * @brief スタート画面の監督スクリプト用のクラス
 */
public class StartSceneDirector : MonoBehaviour
{
    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        // エンターキーが押されたとき
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("GameScene");// ゲーム画面へ遷移
        }
    }
}
