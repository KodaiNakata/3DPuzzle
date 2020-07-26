using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/**
* @file PauseController.cs
* @brief ポーズ操作のファイル
* @author Kodai Nakata
*/

/**
 * @class PauseController
 * @brief ポーズ操作のクラス
 */
public class PauseController : MonoBehaviour
{
    // ポーズ状態か
    private static bool isPaused;
    private GameObject pausePanel;

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Start()
    {
        pausePanel = GameObject.FindGameObjectWithTag("Pause");
        isPaused = false;
        pausePanel.SetActive(false);// ポーズを非表示にする
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        // Escキーが押されたとき
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ポーズの状態のとき
            if (isPaused)
            {
                isPaused = false;// ポーズ解除する
                Time.timeScale = 1f;
                pausePanel.SetActive(false);// ポーズを非表示にする
            }
            else
            {
                isPaused = true;// ポーズ状態にする
                Time.timeScale = 0f;
                pausePanel.SetActive(true);// ポーズを表示する
            }
        }
        // ポーズ状態でバックスペースキーが押されたとき
        else if (Input.GetKeyDown(KeyCode.Backspace) && isPaused)
        {
            isPaused = false;// ポーズ解除する
            Time.timeScale = 1f;
            pausePanel.SetActive(false);// ポーズを非表示にする
            SceneManager.LoadScene("StartScene");// スタート画面へ遷移する
        }
    }

    /**
     * @brief ポーズ状態を取得する
     * @return true:ポーズ状態
     *         false:ポーズ解除
     */
    public static bool IsPaused()
    {
        return isPaused;
    }
}
