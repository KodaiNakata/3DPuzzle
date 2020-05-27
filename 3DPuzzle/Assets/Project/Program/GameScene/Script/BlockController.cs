using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * @file BlockController.cs
 * @brief ブロックの移動や衝突判定を扱うファイル
 * @author Kodai Nakata
 */

/**
 * @class BlockController
 * @brief ブロックの移動や衝突判定を扱うクラス
 */
public class BlockController : MonoBehaviour
{
    /**
     * @enum InputDirection
     * 入力した方向の列挙体
     */
    enum InputDirection{
        no,// 入力なし
        left,// 左方向入力あり
        right,// 右方向入力あり
        up,// 上方向入力あり
        down,// 下方向入力あり
    };

    Vector3Int blockPosition;// ブロックの位置
    Quaternion beforeQuaternion;// 回転前の角度

    int blockScale;// ブロックの大きさ
    int blockNum;// ブロックの数

    float timerSpan = 1.0f;// タイマーの期間
    float timerCount = 0;// タイマーのカウント

    float triggerSpan = 0.05f;// 衝突判定の期間
    float triggerCount = 0;// 衝突判定のカウント

    bool canSet;// 設置できたか
    bool isCollide;// 衝突したか

    InputDirection inputDirect;// どこの方向を入力したか

    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        Debug.Log("BlockController::Start");
        // @todo 下記のthis.blockPositionはテスト用で適当に配置
        //       本番用に無難に設置できるような座標を決める必要あり
        this.blockPosition = new Vector3Int(10, 100, 0);
        this.beforeQuaternion = transform.rotation;
        this.blockScale = (int)transform.GetChild(0).lossyScale.x;
        this.blockNum = transform.childCount;
        this.canSet = false;
        this.isCollide = false;
        this.inputDirect = InputDirection.no;
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        CountTrigger();
        // 設置未完了の時
        if (!this.canSet)
        {
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
            if (other.gameObject.CompareTag("Wall_Left"))
            {
                this.blockPosition.x += this.blockScale;
            }
            // 手前側の壁に衝突したとき
            else if (other.gameObject.CompareTag("Wall_Foreground"))
            {
                this.blockPosition.z += this.blockScale;
            }
            // 右側の壁に衝突したとき
            else if (other.gameObject.CompareTag("Wall_Right"))
            {
                this.blockPosition.x -= this.blockScale;
            }
            // 奥側の壁に衝突したとき
            else if (other.gameObject.CompareTag("Wall_Back"))
            {
                this.blockPosition.z -= this.blockScale;
            }
            // ステージ（床）に衝突したとき
            else if (other.gameObject.CompareTag("Stage"))
            {
                canSet = true;// 設置完了状態にする
                this.blockPosition.y += this.blockScale;
                BlockGenerator.GetInstatnce().SetStartCreating(true);// ブロックの生成を開始する
                GetComponent<BlockController>().enabled = false;// スクリプトを非アクティブにする
            }
            // ブロックと衝突したときの判定式をここに記載する
            else if (other.gameObject.CompareTag("Block"))
            {
                Debug.Log(other.gameObject.name + "と衝突");
                // 左方向キーが入力されてブロックに衝突したとき
                if (inputDirect == InputDirection.left)
                {
                    this.blockPosition.x += this.blockScale;
                }
                // 右方向キーが入力されてブロックに衝突したとき
                else if (inputDirect == InputDirection.right)
                {
                    this.blockPosition.x -= this.blockScale;
                }
                // 上方向キーが入力されてブロックに衝突したとき
                else if (inputDirect == InputDirection.up)
                {
                    this.blockPosition.z -= this.blockScale;
                }
                // 下方向キーが入力されてブロックに衝突したとき
                else if (inputDirect == InputDirection.down)
                {
                    this.blockPosition.z += this.blockScale;
                }
                // 回転させてブロックに衝突したとき
                else if (this.beforeQuaternion != transform.rotation)
                {
                    transform.rotation = this.beforeQuaternion;
                }
                // 方向キー何も入力せずにブロックに衝突したとき
                else
                {
                    canSet = true;// 設置完了状態にする
                    this.blockPosition.y += this.blockScale;
                    GetComponent<BlockController>().enabled = false;// スクリプトを非アクティブにする
                    BlockGenerator.GetInstatnce().SetStartCreating(true);// ブロックの生成を開始する
                }
            }
            isCollide = true;// 衝突した状態にする
        }
        transform.position = this.blockPosition;
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
            this.blockPosition.y -= this.blockScale;// ブロックを下へ移動
        }
    }

    /**
     * @brief キー入力
     */
    void InputKey()
    {
        inputDirect = InputDirection.no;

        // 回転前の角度を格納する
        this.beforeQuaternion = transform.rotation;

        // ブロックの移動
        // 上方向キーが押された瞬間
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.blockPosition.z += this.blockScale;
            inputDirect = InputDirection.up;
        }
        // 右方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.blockPosition.x += this.blockScale;
            inputDirect = InputDirection.right;
        }
        // 下方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.blockPosition.z -= this.blockScale;
            inputDirect = InputDirection.down;
        }
        // 左方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.blockPosition.x -= this.blockScale;
            inputDirect = InputDirection.left;
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
        transform.position = this.blockPosition;
    }
}
