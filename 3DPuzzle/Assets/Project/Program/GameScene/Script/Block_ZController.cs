using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_ZController : MonoBehaviour
{
    Vector3 blockPosition;// ブロックの位置

    float blockScale;// ブロックの大きさ
    int blockNum;// ブロックの数

    float timerSpan = 1.0f;// タイマーの期間
    float timerCount = 0;// タイマーのカウント

    float stageScale = 40.0f;// ステージの大きさ(縦と横の長さ)

    // ブロックの方向
    enum Direction
    {
        left,// 左
        right,// 右
        back,// 奥
        foreground// 手前
    }

    int backDirection;// 奥方向
    int sideDirection;// 横方向

    bool canSet;// 設置できたか

    // Start is called before the first frame update
    void Start()
    {
        blockPosition = new Vector3(10.0f, 20.0f, 0.0f);
        blockScale = transform.GetChild(0).lossyScale.x;
        blockNum = transform.childCount;
        canSet = false;
        backDirection = 1;
        sideDirection = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("奥方向：" + backDirection + ",横方向：" + sideDirection);
        FallBlock();
        InputKey();
    }

    /**
     * @brief ブロックを落下させる
     */
    void FallBlock()
    {
        this.timerCount += Time.deltaTime;

        // 一定時間を超えていないとき
        if(this.timerCount > this.timerSpan)
        {
            this.timerCount = 0;// タイマーをリセットする
            
            // 設置できていないとき
            if (canSet == false)
            {
                blockPosition.y -= blockScale;// ブロックを下へ移動
                transform.position = FinishedFalling();
            }
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
            //blockPosition.z = GetModifiedBlockPosition(blockPosition.z + blockScale);
        }
        // 右方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            blockPosition.x += blockScale;
            //blockPosition.x = GetModifiedBlockPosition(blockPosition.x + blockScale);
            blockPosition.x = GetModifiedBlockPosition(Direction.right);
        }
        // 下方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            blockPosition.z -= blockScale;
            //blockPosition.z = GetModifiedBlockPosition(blockPosition.z - blockScale);
        }
        // 左方向キーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            blockPosition.x -= blockScale;
            //blockPosition.x = GetModifiedBlockPosition(blockPosition.x - blockScale);
            blockPosition.x = GetModifiedBlockPosition(Direction.left);
        }

        // ブロックの回転
        // Aキーが押された瞬間
        if (Input.GetKeyDown(KeyCode.A))
        {
            // ブロックを左に回転させる(z軸中心)
            transform.Rotate(0, 0, 90.0f, Space.World);
            blockDirection(0, 1);
        }
        // Dキーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // ブロックを右に回転させる(y軸中心)
            transform.Rotate(0, -90.0f, 0, Space.World);
            blockDirection(0, -1);
        }
        // Wキーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.W))
        {
            // ブロックを奥に回転させる(x軸中心)
            transform.Rotate(90.0f, 0, 0, Space.World);
            blockDirection(1, 0);
        }
        /*
        // Sキーが押された瞬間
        else if (Input.GetKeyDown(KeyCode.S))
        {
            // ブロックを手前に回転させる
            transform.Rotate(-90.0f, 0, 0, Space.World);
            blockDirection(-1, 0);
        }*/
        transform.position = blockPosition;
        //transform.position = ModifyBlockPosition();
    }

    /**
     * @brief ブロックが落下し終えたか(縦方向の当たり判定)
     * @return 落下し終えた後のブロックの座標
     */
    Vector3 FinishedFalling()
    {
        /*
        // 縦方向の当たり判定
        // 横方向が～～～のとき
        if(sideDirection == 2 || sideDirection == 4)
        {
            if (blockPosition.y - blockScale < 0.0f)
            {
                blockPosition.y += blockScale;
                canSet = true;
            }
        }
        else
        {
            // 奥方向が～～～のとき
            if(backDirection == 1 || backDirection == 3)
            {
                if (blockPosition.y - 1.5f * blockScale < 0.0f)
                {
                    blockPosition.y += blockScale;
                    canSet = true;
                }
            }
            else
            {
                if (blockPosition.y - 0.5f * blockScale < 0.0f)
                {
                    blockPosition.y += blockScale;
                    canSet = true;
                }
            }
        }*/

        // @todo 各ブロックがステージ外かどうかの判定
        for (int blockNo = 0; blockNo < blockNum; blockNo++)
        {
            if (transform.GetChild(blockNo).position.y < 0.0f)
            {
                blockPosition.y = blockScale;
                canSet = true;
            }
        }
        return blockPosition;
    }

    /**
     * @brief 修正したブロックの座標を取得する
     * @param direct ブロックの各方向
     * @return ステージ外に出たものを修正した座標
     */
    float GetModifiedBlockPosition(Direction direct)
    {
        float judgedBlockPos = 0.0f;
        int blockNumOfOut = 0;

        for (int blockNo = 0; blockNo < blockNum; blockNo++)
        {
            //float judgeBlockPos = blockPosition.x + transform.GetChild(blockNo).position.x;// 判定するブロックの位置
            float judgeBlockPos = transform.GetChild(blockNo).position.x;// 判定するブロックの位置

            // 判定するブロックの位置がステージ外で以前判定したブロックの位置と異なるとき
            if (judgeBlockPos < 0.0f && judgedBlockPos != judgeBlockPos)
            {
                judgedBlockPos = judgeBlockPos;
                blockNumOfOut++;

                // ステージ外のブロックが2個のとき
                if (2 <= blockNumOfOut)
                {
                    break;
                }
            }
        }

        // ステージ外のブロックがあるとき
        if (0 < blockNumOfOut)
        {
            switch (direct)
            {
                // ブロックの左側を判定するとき
                case Direction.left:
                    blockPosition.x = Mathf.Clamp(blockPosition.x, (float)blockNumOfOut * blockScale, stageScale); ;// x座標をステージ内に戻す
                    break;
            }
        }
        return blockPosition.x;
    }

    /**
     * @brief ブロックの座標を修正する
     * @return 修正したブロックの座標
     */
    Vector3 ModifyBlockPosition()
    {
        /*
        // 奥行の当たり判定
        if(backDirection == 1 || backDirection == 3)
        {
            // 手前の当たり判定
            if(blockPosition.z - 0.5f * blockScale < 0.0f)
            {
                //position.z += blockScale;
                blockPosition.z = 0.5f * blockScale;
            }
            // 奥側の当たり判定
            else if(blockPosition.z + 0.5f * blockScale > stageScale)
            {
                //position.z -= blockScale;
                blockPosition.z = stageScale - 0.5f * blockScale;
            }
        }
        else
        {
            // 手前の当たり判定
            if (blockPosition.z - 1.5f * blockScale < 0.0f)
            {
                //position.z += blockScale;
                blockPosition.z = 1.5f * blockScale;
            }
            // 奥側の当たり判定
            else if(blockPosition.z + 1.5f * blockScale > stageScale)
            {
                //position.z -= blockScale;
                blockPosition.z = stageScale - 1.5f * blockScale;
            }
        }

        // 横側の当たり判定
        if(sideDirection == 1 || sideDirection == 3)
        {
            // 左側の当たり判定
            if(blockPosition.x - blockScale < 0.0f)
            {
                //position.x += blockScale;
                blockPosition.x = blockScale;
            }
            // 右側の当たり判定
            else if(blockPosition.x + blockScale > stageScale)
            {
                blockPosition.x = stageScale - blockScale;
            }
        }
        else
        {
            if(backDirection == 1 || backDirection == 3)
            {
                // 左側の当たり判定
                if (blockPosition.x - 1.5f * blockScale < 0.0f)
                {
                    //position.x += blockScale;
                    blockPosition.x = 1.5f * blockScale;
                }
                // 右側の当たり判定
                else if(blockPosition.x + 1.5f * blockScale > stageScale)
                {
                    //position.x -= blockScale;
                    blockPosition.x = stageScale - 1.5f * blockScale;
                }
            }
            else
            {
                // 左側の当たり判定
                if (blockPosition.x - 0.5f * blockScale < 0.0f)
                {
                    //position.x += blockScale;
                    blockPosition.x = 1.5f * blockScale;
                }
                // 右側の当たり判定
                else if (blockPosition.x + 0.5f * blockScale > stageScale)
                {
                    //position.x -= blockScale;
                    blockPosition.x = stageScale - 0.5f * blockScale;
                }
            }
            
        }*/

        int blockNumOfOut = 0;// ステージ外のブロックの数
        float judgedBlockPos = 0.0f;// 判定したブロックの位置
        Debug.Log("絶対位置:" + blockPosition.x + "各ブロックの位置:" + transform.GetChild(3).position.x);

        // @todo 各ブロックがステージ外かどうかの判定
        // @todo x座標を注目したときにブロック何個分出ているかを取得する(最大2個)
        for (int blockNo = 0; blockNo < blockNum; blockNo++)
        {         
            //float judgeBlockPos = blockPosition.x + transform.GetChild(blockNo).position.x;// 判定するブロックの位置
            float judgeBlockPos = transform.GetChild(blockNo).position.x;// 判定するブロックの位置

            // 判定するブロックの位置がステージ外で以前判定したブロックの位置と異なるとき
            if (judgeBlockPos < 0.0f && judgedBlockPos != judgeBlockPos)
            {
                judgedBlockPos = judgeBlockPos;
                blockNumOfOut++;

                // ステージ外のブロックが2個のとき
                if(2 <= blockNumOfOut)
                {
                    break;
                }
            }
        }

        // ステージ外のブロックがあるとき
        if(0 < blockNumOfOut)
        {
            //blockPosition.x = (float)blockNumOfOut * blockScale;// x座標をステージ内に戻す
            blockPosition.x = Mathf.Clamp(blockPosition.x, (float)blockNumOfOut * blockScale, stageScale); ;// x座標をステージ内に戻す
        }
        /*
        blockNumOfOut = 0;
        judgedBlockPos = 0.0f;

        // @todo 各ブロックがステージ外かどうかの判定
        // @todo y座標を注目したときにブロック何個分出ているかを取得する(最大2個)
        for (int blockNo = 0; blockNo < blockNum; blockNo++)
        {
            if (blockPosition.x + transform.GetChild(blockNo).position.x < 0.0f)
            {
                blockNumOfOut++;
                blockPosition.x = blockScale;
            }
        }

        if (0 < blockNumOfOut)
        {
            blockPosition.x = (float)blockNumOfOut * blockScale;
        }*/

        // @todo 各ブロックの位置の修正(回転した後も含む)
        return blockPosition;
    }

    // @todo 下記の関数はブロックの方向の変数が未使用となるため削除予定
    /**
     * @brief ブロックの方向を
     * @param backDirect 奥方向(正：奥、負：手前)
     * @param sideDirect 横方向(正：左、負：右)
     */
    void blockDirection(int backDirect, int sideDirect)
    {
        backDirection += backDirect;
        sideDirection += sideDirect;

        // ブロックの奥方向の組み合わせが4通りのため
        if(backDirection < 1)
        {
            backDirection = 4;
        }
        else if(4 < backDirection)
        {
            backDirection = 1;
        }

        // ブロックの横方向の組み合わせが4通りのため
        if(sideDirection < 1)
        {
            sideDirection = 4;
        }
        else if(4 < sideDirection)
        {
            sideDirection = 1;
        }
    }
}
