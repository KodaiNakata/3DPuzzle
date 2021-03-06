﻿using System.Collections;
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
    //! 自クラスのインスタンス
    private static BlockGenerator instance;
    //! ブロックの種類
    private const int BLOCK_KIND = 7;
    //! 生成開始
    private bool startCreating;

    //! ブロックのPrefabを格納する配列
    [SerializeField]
    private GameObject[] blockPrefab = default;

    /**
     * @brief 最初のフレームに入る前に呼び出される関数
     */
    void Start()
    {
        instance = GetComponent<BlockGenerator>();
        CreateBlock();
        startCreating = false;
    }

    /**
     * @brief 1フレームごとに呼び出される関数
     */
    void Update()
    {
        // ポーズ状態のとき
        if (PauseController.IsPaused())
        {
            return;// ブロックの生成不可
        }

        // 生成開始の合図が来たとき
        if (startCreating)
        {
            CreateBlock();
            startCreating = false;
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
        int randomBlockNo = Random.Range(0, BLOCK_KIND);// 全ブロックから1種類を取得する
        //int randomBlockNo = 2;
        GameObject newBlockObject = Instantiate(blockPrefab[randomBlockNo]) as GameObject;// 取得したブロックを生成する
        newBlockObject.transform.position = new Vector3Int(20,180,20);
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
