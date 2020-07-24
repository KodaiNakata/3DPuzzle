using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * @file CameraController.cs
 * @brief カメラの移動を扱うファイル
 * @author Kodai Nakata
 */

/**
 * @class CameraController
 * @brief カメラの移動を扱うクラス
 */
public class CameraController : MonoBehaviour
{
    private const float MOVE_SPEED = 3f;// 移動するスピード
    private const float ROTATE_SPEED = 3f;// 回転する速さ
    private const float MIN_DISTANCE = 70f;// ステージに向かって近づける最短距離
    private const float MAX_DISTANCE = 325f;// ステージから遠ざける最長距離

    private Vector3 centerRotation;// 回転の中心
    private Vector3 beforeCameraPosition;// 更新前のカメラの位置

    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        centerRotation = new Vector3(20.0f, 0.0f, 20.0f);// 回転の中心をステージの真ん中
        beforeCameraPosition = transform.position;
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        InputKey();
        ModifiedCameraPos();
    }

    /**
     * @brief キー入力
     */
    private void InputKey()
    {
        // Shiftキーが押されている間
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            // 左右方向キーが押されている間
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
            {
                float angle = Input.GetAxis("Horizontal") * ROTATE_SPEED;// 回転させる角度
                transform.RotateAround(centerRotation, Vector3.down, angle);// ステージの真ん中を中心に回転させる
            }
            // 上下方向キーが押されている間
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
            {
                float movement = Input.GetAxis("Vertical") * MOVE_SPEED;// 近づけるまたは遠ざける移動量
                transform.Translate(Vector3.forward * movement);// ステージの真ん中に向かって移動する
            }
        }
    }

    /**
     * @brief カメラの位置を修正
     */
    private void ModifiedCameraPos()
    {
        float distance = Vector3.Distance(transform.position, centerRotation);// ステージの中心からカメラの位置までの距離を取得

        // 最短距離より近づいたまたは最長距離より遠ざけたとき
        if (distance < MIN_DISTANCE || MAX_DISTANCE < distance)
        {
            transform.position = beforeCameraPosition;// カメラの位置を戻す
        }
        else
        {
            beforeCameraPosition = transform.position;// カメラの位置を更新
        }
    }
}
