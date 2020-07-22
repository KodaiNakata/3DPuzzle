using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    private Transform blockTransform;// ブロックのトランスフォーム
    private Vector3Int blockPosition;// ブロックの位置
    private Quaternion blockQuaternion;// ブロックの回転角度
    private int blockPosMaxX;// ブロックが動けるX座標の範囲(最大値)
    private int blockPosMinX;// ブロックが動けるX座標の範囲(最小値)
    private int blockPosMaxZ;// ブロックが動けるZ座標の範囲(最大値)
    private int blockPosMinZ;// ブロックが動けるZ座標の範囲(最小値)
    private int beforeBlockPosY;// ブロックのY座標(移動前)
    private int beforeBlockPosX;// ブロックのX座標(移動前)
    private int beforeBlockPosZ;// ブロックのZ座標(移動前)
    private Quaternion beforeQuaternion;// 回転前の角度

    private int blockScale;// ブロックの大きさ

    private const float TIMER_SPAN = 1.0f;// タイマーの期間
    private float timerCount = 0;// タイマーのカウント

    private bool canSet;// 設置できたか
    private bool isCollide;// 衝突したか
    private bool canRotate;// 回転できるか

    private InputDirection inputDirect;// どこの方向を入力したか

    private List<ColliderList> colliders;// 衝突リスト
    private List<Transform> moveBlocks;// 移動対象のブロックのリスト


    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        // @todo 下記のblockPositionはテスト用で適当に配置
        //       本番用に無難に設置できるような座標を決める必要あり
        blockPosition = new Vector3Int(10, 100, 0);
        blockQuaternion = transform.rotation;
        blockTransform = transform;
        blockTransform.position = blockPosition;
        blockTransform.rotation = blockQuaternion;
        blockScale = (int)transform.GetChild(0).lossyScale.x;
        canSet = false;
        isCollide = false;
        inputDirect = InputDirection.no;
        colliders = new List<ColliderList>();
        beforeBlockPosX = blockPosition.x;
        beforeBlockPosY = blockPosition.y;
        beforeBlockPosZ = blockPosition.z;
        beforeQuaternion = blockQuaternion;
        canRotate = true;
        moveBlocks = new List<Transform>();
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        ModifyBlockPos();
        // 設置未完了で未衝突で回転可能な時
        if (!canSet && !isCollide && colliders.Count == 0 && canRotate)
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
                list.priority = 1;// 優先度中に設定
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
                list.priority = 0;// 優先度低に設定
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
                if (!beforeQuaternion.Equals(blockQuaternion))
                {
                    canRotate = false;// 回転不可能にする
                    list.rotated = true;// 衝突リストに回転して衝突
                    list.priority = 2;// 優先度高に設定
                }
                // 入力なしでブロックと衝突したとき
                else if (list.direct == InputDirection.no)
                {
                    list.priority = 0;// 優先度低に設定
                }
                // 入力ありでブロックと衝突したとき
                else
                {
                    list.priority = 1;// 優先度中に設定
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
                                return;// 衝突リストに追加しない
                            }
                        }
                        // 回転なしで同じ入力方向でブロックと衝突済みのとき
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
                        if (colliderList.colliderObj.gameObject.transform.parent != null)
                        {
                            // 同じブロックと落下して衝突済みのとき
                            if (!list.rotated && list.colliderObj.gameObject.transform.parent.name.Equals(colliderList.colliderObj.gameObject.transform.parent.name))
                            {
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
            // リストの中身を優先度順(降順)に並び変える
            tmpList.Sort((a,b) => b.priority - a.priority);
            foreach (ColliderList colliderList in tmpList)
            {
                // 左の壁に衝突したとき
                if (colliderList.colliderObj.gameObject.CompareTag("Wall_Left"))
                {
                    blockPosition.x += blockScale;
                    blockPosMinX = blockPosition.x;// 左へ移動できる範囲を制限
                }
                // 手前の壁に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Foreground"))
                {
                    blockPosition.z += blockScale;
                    blockPosMinZ = blockPosition.z;// 手前へ移動できる範囲を制限
                }
                // 右の壁に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Right"))
                {
                    blockPosition.x -= blockScale;
                    blockPosMaxX = blockPosition.x;// 右へ移動できる範囲を制限
                }
                // 奥側の壁に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Wall_Back"))
                {
                    blockPosition.z -= blockScale;
                    blockPosMaxZ = blockPosition.z;// 奥へ移動できる範囲を制限
                }
                // ステージ（床）に衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Stage"))
                {
                    canSet = true;// 設置完了状態にする
                    // 回転して衝突したとき
                    if (!beforeQuaternion.Equals(blockQuaternion))
                    {
                        blockQuaternion = beforeQuaternion;// 元の角度に戻す
                    }
                    // x方向に移動して衝突したとき
                    if (beforeBlockPosX != blockPosition.x)
                    {
                        blockPosition.x = beforeBlockPosX;// 元の位置に戻す
                    }
                    // y方向に移動(落下)して衝突したとき
                    if (beforeBlockPosY != blockPosition.y)
                    {
                        blockPosition.y = beforeBlockPosY;// 元の位置に戻す
                    }
                    // z方向に移動して衝突したとき
                    if (beforeBlockPosZ != blockPosition.z)
                    {
                        blockPosition.z = beforeBlockPosZ;// 元の位置に戻す
                    }
                }
                // ブロックと衝突したとき
                else if (colliderList.colliderObj.gameObject.CompareTag("Block"))
                {
                    // 回転させてブロックに衝突したとき
                    if (colliderList.rotated)
                    {
                        blockQuaternion = beforeQuaternion;// 元の角度に戻す
                        blockPosition.x = beforeBlockPosX;// 元のx座標に戻す
                        blockPosition.z = beforeBlockPosZ;// 元のz座標に戻す
                        canRotate = true;// 元の位置に戻したため回転可能にする
                    }
                    // 左方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.left)
                    {
                        blockPosition.x += blockScale;
                        //beforeBlockPosX = blockPosition.x;
                        blockPosMinX = blockPosition.x;
                    }
                    // 右方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.right)
                    {
                        blockPosition.x -= blockScale;
                        //beforeBlockPosX = blockPosition.x;
                        blockPosMaxX = blockPosition.x;
                    }
                    // 上方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.up)
                    {
                        blockPosition.z -= blockScale;
                        //beforeBlockPosZ = blockPosition.z;
                        blockPosMaxZ = blockPosition.z;
                    }
                    // 下方向キーが入力されてブロックに衝突したとき
                    else if (colliderList.direct == InputDirection.down)
                    {
                        blockPosition.z += blockScale;
                        //beforeBlockPosZ = blockPosition.z;
                        blockPosMinZ = blockPosition.z;
                    }
                    // 方向キー何も入力せずにブロックに衝突したとき
                    else
                    {
                        canSet = true;// 設置完了状態にする
                        // x方向に移動して衝突したとき
                        if (beforeBlockPosX != blockPosition.x)
                        {
                            blockPosition.x = beforeBlockPosX;// 元の位置に戻す
                        }
                        // z方向に移動して衝突したとき
                        if (beforeBlockPosZ != blockPosition.z)
                        {
                            blockPosition.z = beforeBlockPosZ;// 元の位置に戻す
                        }
                        // y方向に移動(落下)して衝突したとき
                        if (beforeBlockPosY != blockPosition.y)
                        {
                            blockPosition.y = beforeBlockPosY;// 元の位置に戻す
                        }
                        // 回転して衝突したとき
                        if (!beforeQuaternion.Equals(blockQuaternion))
                        {
                            blockQuaternion = beforeQuaternion;// 元の角度に戻す
                        }
                    }
                }
                colliders.Remove(colliderList);
                transform.position = blockPosition;// ステージ内に戻す
                transform.rotation = blockQuaternion;
            }
        }
        // 衝突リストが空の時
        else
        {
            isCollide = false;// 未衝突の状態にする
            canRotate = true;// 未衝突のため回転可能にする
            beforeQuaternion = blockQuaternion;// 未衝突の状態の角度を格納
            beforeBlockPosX = blockPosition.x;// 未衝突の状態のx座標を格納
            beforeBlockPosY = blockPosition.y;// 未衝突の状態のy座標を格納
            beforeBlockPosZ = blockPosition.z;// 未衝突の状態のz座標を格納
        }
    }

    /**
     * @brief ブロックを落下させる
     */
    private void FallBlock()
    {
        timerCount += Time.deltaTime;

        // 一定時間を超えて未設置かつ未衝突かつ衝突リストが空かつ回転可能のとき
        if (timerCount > TIMER_SPAN && !canSet && !isCollide && colliders.Count == 0 && canRotate)
        {
            blockPosMaxX = 40;// x座標の最大値を格納
            blockPosMinX = 0;// x座標の最小値を格納
            blockPosMaxZ = 40;// z座標の最大値を格納
            blockPosMinZ = 0;// z座標の最小値を格納
            inputDirect = InputDirection.no;// 落下したため入力なしとする
            timerCount = 0;// タイマーをリセットする
            blockPosition.y -= blockScale;// ブロックを下へ移動
        }
    }

    /**
     * @brief キー入力
     */
    private void InputKey()
    {
        inputDirect = InputDirection.no;// 入力前のため入力なしとする

        // ブロックの移動
        // Shiftキーが押されていない間
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            // 上方向キーが押された瞬間
            if (Input.GetKeyDown(KeyCode.UpArrow) && !canSet && !isCollide)
            {
                blockPosition.z += blockScale;
                // z座標の最大値を超えたとき
                if (blockPosMaxZ < blockPosition.z)
                {
                    blockPosition.z = blockPosMaxZ;// 上方向には動かない
                }
                inputDirect = InputDirection.up;// 上方向を入力したものとする
                canRotate = true;// 上方向に動かすので回転可能にする
            }
            // 右方向キーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.RightArrow) && !canSet && !isCollide)
            {
                blockPosition.x += blockScale;
                // x座標の最大値を超えたとき
                if (blockPosMaxX < blockPosition.x)
                {
                    blockPosition.x = blockPosMaxX;// 右方向には動かない
                }
                inputDirect = InputDirection.right;// 右方向を入力したものとする
                canRotate = true;// 右方向に動かすので回転可能にする
            }
            // 下方向キーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.DownArrow) && !canSet && !isCollide)
            {
                blockPosition.z -= blockScale;
                // z座標の最小値を下回ったとき
                if (blockPosMinZ > blockPosition.z)
                {
                    blockPosition.z = blockPosMinZ;// 下方向には動かない
                }
                inputDirect = InputDirection.down;// 下方向を入力したものとする
                canRotate = true;// 下方向に動かすので回転可能にする
            }
            // 左方向キーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && !canSet && !isCollide)
            {
                blockPosition.x -= blockScale;
                // x座標の最小値を下回ったとき
                if (blockPosMinX > blockPosition.x)
                {
                    blockPosition.x = blockPosMinX;// 左方向には動かない
                }
                inputDirect = InputDirection.left;// 左方向を入力したものとする
                canRotate = true;// 左方向に動かすので回転可能にする
            }
            // ブロックの回転
            // Aキーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.A) && !canSet && !isCollide && canRotate)
            {
                beforeQuaternion = blockQuaternion;// 回転前の角度を格納する
                
                // ブロックを左に回転させる(z軸中心)
                blockTransform.Rotate(0, 0, 90.0f, Space.World);
                blockQuaternion = blockTransform.rotation;

                blockPosMaxX = 40;// 回転したのでx座標の最大値を40にリセットする
                blockPosMaxZ = 40;// 回転したのでz座標の最大値を40にリセットする
                blockPosMinX = 0;// 回転したのでx座標の最小値を0にリセットする
                blockPosMinZ = 0;// 回転したのでz座標の最小値を0にリセットする
            }
            // Dキーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.D) && !canSet && !isCollide && canRotate)
            {
                beforeQuaternion = blockQuaternion;// 回転前の角度を格納する
                
                // ブロックを右に回転させる(y軸中心)
                blockTransform.Rotate(0, -90.0f, 0, Space.World);
                blockQuaternion = blockTransform.rotation;

                blockPosMaxX = 40;// 回転したのでx座標の最大値を40にリセットする
                blockPosMaxZ = 40;// 回転したのでz座標の最大値を40にリセットする
                blockPosMinX = 0;// 回転したのでx座標の最小値を0にリセットする
                blockPosMinZ = 0;// 回転したのでz座標の最小値を0にリセットする
            }
            // Wキーが押された瞬間
            else if (Input.GetKeyDown(KeyCode.W) && !canSet && !isCollide && canRotate)
            {
                
                beforeQuaternion = blockQuaternion;// 回転前の角度を格納する

                // ブロックを奥に回転させる(x軸中心)
                blockTransform.Rotate(90.0f, 0, 0, Space.World);
                blockQuaternion = blockTransform.rotation;

                blockPosMaxX = 40;// 回転したのでx座標の最大値を40にリセットする
                blockPosMaxZ = 40;// 回転したのでz座標の最大値を40にリセットする
                blockPosMinX = 0;// 回転したのでx座標の最小値を0にリセットする
                blockPosMinZ = 0;// 回転したのでz座標の最小値を0にリセットする
            }
        }
        // ブロックの位置を反映する
        transform.position = blockPosition;
        transform.rotation = blockQuaternion;
    }

    /**
     * @brief 設置完了できたかを確認する
     */
    private void FinishSetting()
    {
        // 設置完了状態かつ未衝突かつ衝突リストが空のとき
        if (canSet && !isCollide && colliders.Count == 0)
        {
            DeleteBlock();// ブロックの消滅
            MoveBlock();// 消滅対象外のブロックの移動
            if (this != null)
            {
                GetComponent<Rigidbody>().isKinematic = false;// 物理演算の影響を受けない
                GetComponent<BlockController>().enabled = false;// スクリプトを非アクティブにする
            }
            BlockGenerator.GetInstatnce().SetStartCreating(true);// ブロックの生成を開始する
        }
    }

    /**
     * @brief ブロックを消滅させる
     * @ todo 消滅時に必要なのはそれぞれのブロックのy座標の位置
     * 子cubeのy座標は消滅する際にどのcubeを対象とするかに必要
     * 子cubeのy座標は消滅後に移動が発生するかの際に必要
     * 1列あたり16このcubeがあれば消滅
     * 同じ親オブジェクト名でもそれぞれのx座標とz座標は違うため検出可能
     * 子cubeが4つすべて消えたら親cubeも消滅
     */
    private void DeleteBlock()
    {
        List<int> blocksPosYList = new List<int>();// 消滅対象のブロックのy座標のリスト
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("ParentBlock");
        List<Transform> deleteChildBlockList = new List<Transform>();// 消滅対象の子ブロックのリスト

        // 消滅対象のブロックのy座標を格納
        for(int blockNo = 0; blockNo < transform.childCount; blockNo++)
        {
            // 設置したブロックのy座標を取得
            int blockPosY = Mathf.RoundToInt(transform.GetChild(blockNo).position.y);

            // リストの中身が重複していない場合
            if (!blocksPosYList.Contains(blockPosY))
            {
                blocksPosYList.Add(blockPosY);// リストに追加する
            }
        }

        blocksPosYList.Sort((a, b) => b - a);// 降順に並び替える

        // 消滅対象のブロックの決定とそれに伴う移動対象のブロックの決定
        for (int blockNo = 0; blockNo < blocksPosYList.Count; blockNo++)
        {
            int blockNum = 0;
            
            // それぞれのブロックを1列ごとにカウント
            foreach (GameObject gameObject in gameObjects)
            {
                for(int childBlockNo = 0; childBlockNo < gameObject.transform.childCount; childBlockNo++)
                {
                    int blockPosY = Mathf.RoundToInt(gameObject.transform.GetChild(childBlockNo).position.y);// それぞれのブロックの位置を格納
                    // 設置したブロックと同じy座標の位置にあるとき
                    if (blocksPosYList[blockNo] == blockPosY)
                    {
                        blockNum++;
                    }
                }

            }
            // 1列にブロックが埋め尽くされているとき
            if (blockNum == 16)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    for (int childBlockNo = 0; childBlockNo < gameObject.transform.childCount; childBlockNo++)
                    {
                        int blockPosY = Mathf.RoundToInt(gameObject.transform.GetChild(childBlockNo).position.y);// それぞれのブロックの位置を格納
                        // 消滅対象のy座標と同じ位置にあるとき
                        if (blocksPosYList[blockNo] == blockPosY)
                        {
                            deleteChildBlockList.Add(gameObject.transform.GetChild(childBlockNo));// 消滅対象として追加
                        }
                        // 消滅対象のy座標より高い位置にあるとき
                        else if (blocksPosYList[blockNo] < blockPosY)
                        {
                            // 消滅対象となっていないとき
                            if (!deleteChildBlockList.Contains(gameObject.transform.GetChild(childBlockNo)))
                            {
                                // 消滅したy座標より上のブロックを下へ移動する対象として格納(ブロック一つ分)
                                moveBlocks.Add(gameObject.transform.GetChild(childBlockNo));
                                //Debug.Log("移動対象：" + gameObject.transform.GetChild(childBlockNo).name);
                            }
                        }
                    }
                }
            }
        }

        // 子ブロックを消滅させる
        foreach (Transform childBlock in deleteChildBlockList)
        {
            //Debug.Log("子ブロック消滅：" + childBlock.name);
            DestroyImmediate(childBlock.gameObject);// 子ブロックを消滅
        }

        // 子ブロックが全て消滅しているか確認をする
        foreach (GameObject gameObject in gameObjects)
        {
            // 子ブロックすべてが消滅したとき
            if (gameObject.transform.childCount == 0)
            {
                //Debug.Log("親オブジェクト消滅：" + gameObject.name);
                DestroyImmediate(gameObject);// 親オブジェクトを消滅
            }
        }
    }

    /**
     * @brief ブロック消滅後のブロックの移動
     */
    private void MoveBlock()
    {
        foreach (Transform block in moveBlocks)
        {
            Vector3 childBlockPos = block.position;
            childBlockPos.y -= blockScale;
            block.position = childBlockPos;
        }
    }
}
