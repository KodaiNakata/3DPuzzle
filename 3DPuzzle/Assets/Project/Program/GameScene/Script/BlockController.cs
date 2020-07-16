using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
    private enum InputDirection{
        left,// 左方向入力あり
        right,// 右方向入力あり
        up,// 上方向入力あり
        down,// 下方向入力あり
        no,// 入力なし
    };

    /**
     * @struct ColliderList
     * @brief 衝突の構造体
     */
    private struct ColliderList
    {
        public int priority;// 優先度(値が高いほど優先度が高め)
        public Collider colliderObj;// 衝突オブジェクト
        public InputDirection direct;// 入力方向
        public bool rotated;// 回転したか
    }

    private Vector3Int blockPosition;// ブロックの位置
    private int blockPosMaxX;// ブロックが動けるX座標の範囲(最大値)
    private int blockPosMinX;// ブロックが動けるX座標の範囲(最小値)
    private int blockPosMaxZ;// ブロックが動けるZ座標の範囲(最大値)
    private int blockPosMinZ;// ブロックが動けるZ座標の範囲(最小値)
    private int beforeBlockPosY;// ブロックのY座標(移動前)
    private int beforeBlockPosX;// ブロックのX座標(移動前)
    private int beforeBlockPosZ;// ブロックのZ座標(移動前)
    private Quaternion beforeQuaternion;// 回転前の角度

    private int blockScale;// ブロックの大きさ
    private int blockNum;// ブロックの数

    private const float TIMER_SPAN = 1.0f;// タイマーの期間
    private float timerCount = 0;// タイマーのカウント

    private bool canSet;// 設置できたか
    private bool isCollide;// 衝突したか
    private bool canRotate;// 回転できるか

    private InputDirection inputDirect;// どこの方向を入力したか

    private List<ColliderList> colliders;// 衝突リスト

    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        // @todo 下記のblockPositionはテスト用で適当に配置
        //       本番用に無難に設置できるような座標を決める必要あり
        blockPosition = new Vector3Int(10, 100, 0);
        beforeQuaternion = transform.rotation;
        blockScale = (int)transform.GetChild(0).lossyScale.x;
        blockNum = transform.childCount;
        canSet = false;
        isCollide = false;
        inputDirect = InputDirection.no;
        colliders = new List<ColliderList>();
        beforeBlockPosX = blockPosition.x;
        beforeBlockPosY = blockPosition.y;
        beforeBlockPosZ = blockPosition.z;
        canRotate = true;
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        ModifyBlockPos();
        // 設置未完了で未衝突の時
        if (!canSet && !isCollide && colliders.Count == 0)
        {
            FallBlock();
            InputKey();
        }
        FinishSetting();
    }

    /**
     * @brief 衝突（すり抜けあり）を検出する
     * @param other 衝突したオブジェクト
     */
    void OnTriggerStay(Collider other)
    {
        // 衝突リストがnullでないとき
        if (colliders != null)
        {
            // 4方向の壁に衝突したとき
            if (other.gameObject.CompareTag("Wall_Left") ||
                other.gameObject.CompareTag("Wall_Foreground") ||
                other.gameObject.CompareTag("Wall_Right") ||
                other.gameObject.CompareTag("Wall_Back"))
            {
                ColliderList list = new ColliderList();
                list.colliderObj = other;
                list.direct = inputDirect;
                list.priority = 1;// 優先度高めに設定
                list.rotated = false;// 4方向の壁は回転後でもステージ内に移動したいのでfalseとする

                foreach (ColliderList colliderList in colliders)
                {
                    // 同じ方向の壁と衝突済みの時
                    if (list.colliderObj.gameObject.CompareTag(colliderList.colliderObj.gameObject.tag))
                    {
                        return;// 衝突リストに追加しない
                    }
                    // 同じ方向の入力でブロックに衝突済みの時
                    else if (colliderList.colliderObj.gameObject.CompareTag("Block") &&
                             colliderList.direct == list.direct)
                    {
                        return;// 衝突リストに追加しない
                    }
                }
                colliders.Add(list);// 新しい衝突として検出する
                isCollide = true;// 衝突した状態にする
            }
            // ステージの床に衝突したとき
            else if (other.gameObject.CompareTag("Stage"))
            {
                ColliderList list = new ColliderList();
                list.colliderObj = other;
                list.direct = InputDirection.no;// 床と衝突するので入力なしを格納
                list.priority = 0;// 一番優先度低めに設定
                list.rotated = false;// 床は回転後でもステージ内に移動したいのでfalseとする

                foreach (ColliderList colliderList in colliders)
                {
                    // ステージの床と衝突済みの時
                    if (colliderList.colliderObj.gameObject.CompareTag("Stage"))
                    {
                        return;// 衝突リストに追加しない
                    }
                    // 入力なしでブロックと衝突済みの時
                    else if (colliderList.colliderObj.gameObject.CompareTag("Block") &&
                             colliderList.direct == InputDirection.no)
                    {
                        return;// 衝突リストに追加しない
                    }
                }
                Debug.Log("床と衝突した");
                colliders.Add(list);// 新しい衝突として検出する
                isCollide = true;// 衝突した状態にする
            }
            // ブロックと衝突したとき
            else if (other.gameObject.CompareTag("Block"))
            {
                ColliderList list = new ColliderList();
                list.colliderObj = other;
                list.direct = inputDirect;
                list.rotated = false;

                // 回転をして衝突したとき
                if (!beforeQuaternion.Equals(transform.rotation))
                {
                    list.rotated = true;
                    list.priority = 2;
                }
                // 入力なしでブロックと衝突したとき
                else if (list.direct == InputDirection.no)
                {
                    list.priority = 0;// 優先度低めに設定
                }
                // 入力ありでブロックと衝突したとき
                else
                {
                    list.priority = 1;// 優先度高めに設定
                }
                // 同じものと衝突していないかのチェック
                foreach (ColliderList colliderList in colliders)
                {
                    // ステージの床と衝突済みのとき
                    if (colliderList.colliderObj.gameObject.CompareTag("Stage") &&
                             colliderList.direct == list.direct)
                    {
                        return;// 衝突リストに追加しない
                    }
                    // 左側の壁と衝突済みのとき
                    else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Left") &&
                             colliderList.direct == list.direct)
                    {
                        return;// 衝突リストに追加しない
                    }
                    // 手前側の壁と衝突済みのとき
                    else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Foreground") &&
                             colliderList.direct == list.direct)
                    {
                        return;// 衝突リストに追加しない
                    }
                    // 右側の壁と衝突済みのとき
                    else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Right") &&
                             colliderList.direct == list.direct)
                    {
                        return;// 衝突リストに追加しない
                    }
                    // 奥側の壁と衝突済みのとき
                    else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Back") &&
                             colliderList.direct == list.direct)
                    {
                        return;// 衝突リストに追加しない
                    }
                    // ブロックと衝突済みのとき
                    else
                    {
                        // 回転して衝突したとき
                        if (list.rotated)
                        {
                            // 別のブロックと回転して衝突済みのとき
                            if (colliderList.rotated == list.rotated)
                            {
                                return;// 衝突リストに追加しない
                            }
                            // 別のブロックと回転なし(落下して)衝突済みのとき
                            else if (colliderList.direct == InputDirection.no)
                            {
                                Debug.Log("別ブロックと落下衝突済み");
                                return;// 衝突リストに追加しない
                            }
                        }
                        // 回転なしで同じ方向に別のブロックと衝突済みのとき
                        else if (colliderList.direct == list.direct)
                        {
                            // 入力なしで衝突したとき
                            if (list.direct == InputDirection.no)
                            {
                                // 回転して衝突済みのとき
                                if (colliderList.rotated)
                                {
                                    return;// 衝突リストに追加しない
                                }
                            }
                            // 入力ありで衝突したとき
                            else
                            {
                                return;// 衝突リストに追加しない
                            }
                        }
                        if (colliderList.colliderObj.gameObject.transform.parent.name != null)
                        {
                            // 同じブロックと衝突済みのとき
                            if (list.colliderObj.gameObject.transform.parent.name.Equals(colliderList.colliderObj.gameObject.transform.parent.name))
                            {
                                Debug.Log("同じブロックと衝突済み");
                                return;// 衝突リストに追加しない
                            }
                        }
                    }
                }
                colliders.Add(list);// 新しい衝突として検出する
                isCollide = true;// 衝突した状態にする
                canSet = false;// 設置未完了
            }
        }
    }

    /**
     * @brief ブロックの位置を修正する
     */
    private void ModifyBlockPos()
    {
        // 衝突のリストがnullでないかつ空でないとき
        if(colliders?.Count > 0)
        {
            inputDirect = InputDirection.no;

            List<ColliderList> tmpList = new List<ColliderList>();

            foreach (ColliderList colliderList in colliders)
            {
                tmpList.Add(colliderList);
            }
            // リストの中身を優先度順に並び変える
            tmpList.Sort((a,b) => b.priority - a.priority);
            foreach (ColliderList colliderList in tmpList)
            {
                // 左の壁に衝突したとき
                if (colliderList.colliderObj.gameObject.CompareTag("Wall_Left"))
                {
                    blockPosition.x += blockScale;
                    beforeBlockPosX = blockPosition.x;
                    blockPosMinX = blockPosition.x;// 左へ移動できる範囲を制限
                }
                // 手前の壁に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Foreground"))
                {
                    blockPosition.z += blockScale;
                    beforeBlockPosZ = blockPosition.z;
                    blockPosMinZ = blockPosition.z;// 手前へ移動できる範囲を制限
                }
                // 右の壁に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Right"))
                {
                    blockPosition.x -= blockScale;
                    beforeBlockPosX = blockPosition.x;
                    blockPosMaxX = blockPosition.x;// 右へ移動できる範囲を制限
                }
                // 奥側の壁に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Back"))
                {
                    blockPosition.z -= blockScale;
                    beforeBlockPosZ = blockPosition.z;
                    blockPosMaxZ = blockPosition.z;// 奥へ移動できる範囲を制限
                }
                // ステージ（床）に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Stage"))
                {
                    canSet = true;// 設置完了状態にする
                    if (beforeBlockPosX != blockPosition.x)
                    {
                        blockPosition.x = beforeBlockPosX;
                    }
                    if (beforeBlockPosY != blockPosition.y)
                    {
                        blockPosition.y = beforeBlockPosY;
                    }
                    if (beforeBlockPosZ != blockPosition.z)
                    {
                        blockPosition.z = beforeBlockPosZ;
                    }
                }
                // ブロックと衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Block"))
                {
                    // 回転させてブロックに衝突したとき
                    if (colliderList.rotated)
                    {
                        Debug.Log("回転前：" + transform.rotation);
                        transform.rotation = beforeQuaternion;
                        Debug.Log("回転後：" + transform.rotation);
                        blockPosition.x = Vector3Int.FloorToInt(transform.position).x;
                        blockPosition.z = Vector3Int.FloorToInt(transform.position).z;
                        Debug.Log("回転操作前の位置：(" + blockPosition.x + "," + blockPosition.y + "," + blockPosition.z + ")");
                        blockPosition.x = beforeBlockPosX;
                        //blockPosition.y = beforeBlockPosY;
                        blockPosition.z = beforeBlockPosZ;
                        Debug.Log("回転操作後の位置：(" + blockPosition.x + "," + blockPosition.y + "," + blockPosition.z + ")");
                        canRotate = false;
                    }
                    // 左方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.left)
                    {
                        blockPosition.x += blockScale;
                        beforeBlockPosX = blockPosition.x;
                        blockPosMinX = blockPosition.x;
                    }
                    // 右方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.right)
                    {
                        blockPosition.x -= blockScale;
                        beforeBlockPosX = blockPosition.x;
                        blockPosMaxX = blockPosition.x;
                    }
                    // 上方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.up)
                    {
                        blockPosition.z -= blockScale;
                        beforeBlockPosZ = blockPosition.z;
                        blockPosMaxZ = blockPosition.z;
                    }
                    // 下方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.down)
                    {
                        blockPosition.z += blockScale;
                        beforeBlockPosZ = blockPosition.z;
                        blockPosMinZ = blockPosition.z;
                    }
                    // 方向キー何も入力せずにブロックに衝突したとき
                    else
                    {
                        canSet = true;// 設置完了状態にする
                        if (beforeBlockPosX != blockPosition.x)
                        {
                            blockPosition.x = beforeBlockPosX;
                        }
                        if (beforeBlockPosZ != blockPosition.z)
                        {
                            blockPosition.z = beforeBlockPosZ;
                        }
                        if (beforeBlockPosY != blockPosition.y)
                        {
                            blockPosition.y = beforeBlockPosY;
                        }
                    }
                }
                colliders.Remove(colliderList);
                transform.position = blockPosition;// ステージ内に戻す
            }
        }
        // 衝突リストが空の時
        else
        {
            isCollide = false;// 未衝突の状態にする
        }
    }

    /**
     * @brief ブロックを落下させる
     */
    private void FallBlock()
    {
        timerCount += Time.deltaTime;

        // 一定時間を超えたとき
        if (timerCount > TIMER_SPAN && !canSet && !isCollide && colliders.Count == 0)
        {
            blockPosMaxX = 40;
            blockPosMinX = 0;
            blockPosMaxZ = 40;
            blockPosMinZ = 0;
            inputDirect = InputDirection.no;
            //beforeBlockPosX = blockPosition.x;
            beforeBlockPosY = blockPosition.y;
            //beforeBlockPosZ = blockPosition.z;
            timerCount = 0;// タイマーをリセットする
            blockPosition.y -= blockScale;// ブロックを下へ移動
            beforeQuaternion = transform.rotation;
            transform.position = blockPosition;// ブロックの位置を反映する
            canRotate = true;
        }
    }

    /**
     * @brief キー入力
     */
    private void InputKey()
    {
        // ブロックの移動
        // Shiftキーが押されていない間
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            // 上方向キーが押された瞬間
            if (Input.GetKeyDown(KeyCode.UpArrow) && !canSet && !isCollide)
            {
                beforeBlockPosZ = blockPosition.z;
                blockPosition.z += blockScale;
                if (blockPosMaxZ < blockPosition.z)
                {
                    blockPosition.z = blockPosMaxZ;
                }
                inputDirect = InputDirection.up;
                canRotate = true;
            }
            // 右方向キーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.RightArrow) && !canSet && !isCollide)
            {
                beforeBlockPosX = blockPosition.x;
                blockPosition.x += blockScale;
                if (blockPosMaxX < blockPosition.x)
                {
                    blockPosition.x = blockPosMaxX;
                }
                inputDirect = InputDirection.right;
                canRotate = true;
            }
            // 下方向キーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.DownArrow) && !canSet && !isCollide)
            {
                beforeBlockPosZ = blockPosition.z;
                blockPosition.z -= blockScale;
                if (blockPosMinZ > blockPosition.z)
                {
                    blockPosition.z = blockPosMinZ;
                }
                inputDirect = InputDirection.down;
                canRotate = true;
            }
            // 左方向キーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && !canSet && !isCollide)
            {
                beforeBlockPosX = blockPosition.x;
                blockPosition.x -= blockScale;
                if (blockPosMinX > blockPosition.x)
                {
                    blockPosition.x = blockPosMinX;
                }
                inputDirect = InputDirection.left;
                canRotate = true;
            }
            // ブロックの回転
            // Aキーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.A) && !canSet && !isCollide && canRotate)
            {
                /*beforeBlockPosX = blockPosition.x;
                beforeBlockPosY = blockPosition.y;
                beforeBlockPosZ = blockPosition.z;*/
                // 回転前の角度を格納する
                beforeQuaternion = transform.rotation;

                // ブロックを左に回転させる(z軸中心)
                transform.Rotate(0, 0, 90.0f, Space.World);
                blockPosMaxX = 40;
                blockPosMaxZ = 40;
                blockPosMinX = 0;
                blockPosMinZ = 0;
            }
            // Dキーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.D) && !canSet && !isCollide && canRotate)
            {
                /*beforeBlockPosX = blockPosition.x;
                beforeBlockPosY = blockPosition.y;
                beforeBlockPosZ = blockPosition.z;*/
                // 回転前の角度を格納する
                beforeQuaternion = transform.rotation;

                // ブロックを右に回転させる(y軸中心)
                transform.Rotate(0, -90.0f, 0, Space.World);
                blockPosMaxX = 40;
                blockPosMaxZ = 40;
                blockPosMinX = 0;
                blockPosMinZ = 0;
            }
            // Wキーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.W) && !canSet && !isCollide && canRotate)
            {
                /*beforeBlockPosX = blockPosition.x;
                beforeBlockPosY = blockPosition.y;
                beforeBlockPosZ = blockPosition.z;*/
                // 回転前の角度を格納する
                beforeQuaternion = transform.rotation;

                // ブロックを奥に回転させる(x軸中心)
                transform.Rotate(90.0f, 0, 0, Space.World);
                blockPosMaxX = 40;
                blockPosMaxZ = 40;
                blockPosMinX = 0;
                blockPosMinZ = 0;
            }
        }
        // ブロックの位置を反映する
        transform.position = blockPosition;
    }

    /**
     * @brief 設置完了できたかを確認する
     */
    private void FinishSetting()
    {
        // 設置完了状態かつ未衝突かつ衝突リストの個数が0のとき
        if (canSet && !isCollide && colliders.Count == 0)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<BlockController>().enabled = false;// スクリプトを非アクティブにする
            BlockGenerator.GetInstatnce().SetStartCreating(true);// ブロックの生成を開始する
        }
    }
}
