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
    private const float ROTATE_SPEED = 3.0f;// 回転する速さ
    private Vector3 centerRotation = new Vector3(20.0f,0.0f,20.0f);// 回転の中心座標

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        // Shiftキーが押されている間
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            float angle = Input.GetAxis("Horizontal") * ROTATE_SPEED;// 回転させる角度(左右方向キーが押されている)

            transform.RotateAround(centerRotation, Vector3.down, angle);// y軸を中心に回転させる
        }
        
    }
}
