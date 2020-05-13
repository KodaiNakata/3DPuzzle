using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_ZController : MonoBehaviour
{
    Vector3Int blockPosition;// ブロックの位置

    int blockScale;// ブロックの大きさ
    int blockNum;// ブロックの数

    float timerSpan = 1.0f;// タイマーの期間
    float timerCount = 0;// タイマーのカウント

    float triggerSpan = 0.1f;// 衝突判定の期間
    float triggerCount = 0;// 衝突判定のカウント

    bool canSet;// 設置できたか
    bool isCollide;// 衝突したか

    // Start is called before the first frame update
    void Start()
    {
        // @todo 下記のblockPositionはテスト用で適当に配置
        //       本番用に座標を後で確定させる
        blockPosition = new Vector3Int(10, 20, 0);
        blockScale = (int)transform.GetChild(0).lossyScale.x;
        blockNum = transform.childCount;
        canSet = false;
        isCollide = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canSet)
        {
            CountTrigger();
            FallBlock();
            InputKey();
        }
    }

    /**
     * @brief 衝突（すり抜けあり）を検出する
     * @param other 衝突したオブジェクト
     */
    void OnTriggerStay(Collider other)
    {
        // 衝突していない状態のとき
        if (!isCollide)
        {
            // 左側の壁に衝突したとき
            if (other.gameObject.name.Equals("Wall_Left"))
            {
                blockPosition.x += blockScale;
            }
            // 手前側の壁に衝突したとき
            else if (other.gameObject.name.Equals("Wall_Foreground"))
            {
                blockPosition.z += blockScale;
            }
            // 右側の壁に衝突したとき
            else if (other.gameObject.name.Equals("Wall_Right"))
            {
                blockPosition.x -= blockScale;
            }
            // 奥側の壁に衝突したとき
            else if (other.gameObject.name.Equals("Wall_Back"))
            {
                blockPosition.z -= blockScale;
            }
            // ステージ（床）に衝突したとき
            else if (other.gameObject.name.Equals("Stage"))
            {
                canSet = true;// 設置完了状態にする
                blockPosition.y += blockScale;
            }
            isCollide = true;// 衝突した状態にする
        }
        transform.position = blockPosition;
    }

    // @todo 衝突判定を一つにするために下記の関数は削除予定
    /**
     * @brief 衝突（すり抜けあり）を検出する
     * @param other 衝突したオブジェクト
     */
    void OnTriggerExit(Collider other)
    {
        //Debug.Log("衝突3");
    }

    /**
     * @brief 衝突判定をさせるタイマーをカウントさせる
     */
    void CountTrigger()
    {
        this.triggerCount += Time.deltaTime;

        // 一定時間を超えたとき
        if (this.triggerCount > this.triggerSpan)
        {
            isCollide = false;// 衝突していない状態にする
        }
    }

    /**
     * @brief ブロックを落下させる
     */
    void FallBlock()
    {
        this.timerCount += Time.deltaTime;

        // 一定時間を超えたとき
        if (this.timerCount > this.timerSpan)
        {
            this.timerCount = 0;// タイマーをリセットする
            blockPosition.y -= blockScale;// ブロックを下へ移動
        }
    }

    /**
     * @brief キー入力
     */
    void InputKey()
    {
        // ブロックの移動
        // 上方向キーが押された瞬間
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            blockPosition.z += blockScale;
        }
        // 右方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            blockPosition.x += blockScale;
        }
        // 下方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            blockPosition.z -= blockScale;
        }
        // 左方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            blockPosition.x -= blockScale;
        }

        // ブロックの回転
        // Aキーが押された瞬間
        if (Input.GetKeyDown(KeyCode.A))
        {
            // ブロックを左に回転させる(z軸中心)
            transform.Rotate(0, 0, 90.0f, Space.World);
        }
        // Dキーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // ブロックを右に回転させる(y軸中心)
            transform.Rotate(0, -90.0f, 0, Space.World);
        }
        // Wキーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.W))
        {
            // ブロックを奥に回転させる(x軸中心)
            transform.Rotate(90.0f, 0, 0, Space.World);
        }
        // ブロックの位置を反映する
        transform.position = blockPosition;
    }
}
