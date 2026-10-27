
using System.Collections;
using UnityEngine;
public class PlayerKnockback : MonoBehaviour 
{ 
    private Rigidbody2D rb; 
    private bool isKnockbacked = false; 
    // ノックバックが持続する時間（秒）
    [SerializeField] private float knockbackDuration = 0.1f; 
    private void Awake() 
    { 
        rb = GetComponent<Rigidbody2D>(); 
    } 
    // 外部から呼ばれるノックバック開始メソッド
    public void ApplyKnockback(Vector2 direction, float force) 
    { 
        if (isKnockbacked) return; 
        // すでにノックバック中なら重ねて処理しない
        StartCoroutine(KnockbackRoutine(direction, force)); 
    }
    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        isKnockbacked = true; // 1. 吹き飛ぶ前に、今までの移動速度を完全にリセット
        rb.linearVelocity = Vector2.zero; // 2. 吹き飛ばす力を加える
        rb.AddForce(direction * force, ForceMode2D.Impulse); // 3. 指定した短い時間だけ待つ
        yield return new WaitForSeconds(knockbackDuration);
        // ★ココが超重要！★// 4. 動かせるようになる直前に、ノックバックの残った勢い（速度）を一度 0 にする！// これにより、猛スピードのまま相手にめり込んで貫通するのを防ぎます
        rb.linearVelocity = Vector2.zero;
        // 5. 操作可能に戻す
        isKnockbacked = false;
    }
        
    // ★重要★ 通常の移動スクリプト側でこのフラグを参照し、trueなら移動入力を無視させてください
    public bool IsKnockbacked => isKnockbacked; 
}