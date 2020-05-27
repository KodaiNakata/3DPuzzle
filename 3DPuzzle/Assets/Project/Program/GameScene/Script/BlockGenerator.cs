using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/**
 * @file BlockGenerator.cs
 * @brief ブロックの生成を扱うファイル
 * @author Kodai Nakata
 */

/**
 * @class BlockGenerator
 * @brief ブロックの生成を扱うクラス
 */
public class BlockGenerator : MonoBehaviour
{
    private static BlockGenerator instance;// 自クラスのインスタンス
    private const int BLOCK_KIND = 7;// ブロックの種類
    private bool startCreating;// 生成開始

    [SerializeField]
    private GameObject[] blockPrefab = default;// ブロックのPrefabを格納する配列

    void Start()
    {
        instance = GetComponent<BlockGenerator>();
        CreateBlock();
        this.startCreating = false;
    }

    void Update()
    {
        // 生成開始の合図が来たとき
        if (this.startCreating)
        {
            CreateBlock();
            this.startCreating = false;
        }
    }

    /**
     * @brief インスタンスを取得する
     * @return インスタンス
     */
    public static BlockGenerator GetInstatnce()
    {
        return instance;
    }

    /**
     * @brief ブロックを生成する
     */
    private void CreateBlock()
    {
        Debug.Log("ブロック生成");
        int randomBlockNo = Random.Range(0, BLOCK_KIND);// 全ブロックから1種類を取得する
        //int randomBlockNo = 2;
        GameObject newBlockObject = Instantiate(blockPrefab[randomBlockNo]) as GameObject;// 取得したブロックを生成する
        newBlockObject.transform.position = new Vector3Int(10,100,0);
    }

    /**
     * @brief 生成開始のセッター関数
     * @param startCreating 生成開始するか
     */
    public void SetStartCreating(bool startCreating)
    {
        this.startCreating = startCreating;
    }
}
