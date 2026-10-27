//using UnityEngine;
//public class ZombieSpawner : MonoBehaviour
//{
//    public GameObject zombiePrefab;
//    // ゾンビのプレハブ
//    public float spawnInterval = 5.0f;
//    // 湧く間隔（秒）// スポーンさせる範囲
//    public float minX = -8.0f;
//    public float maxX = 8.0f;
//    public float spawnY = -7.0f;
//    [Header("上限設定")]
//    public int maxZombies = 4;

//    // 地面の少し上のY座標
//    private float timer;
//    private Camera mainCamera;

//    private void Start()
//    {
//        mainCamera = Camera.main;
//    }
//    void Update()
//    {
//        timer += Time.deltaTime;
//        if (timer >= spawnInterval)
//        {
//            // 1. 今ゲーム内にいるゾンビの数を数える
//            int currentZombieCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
//            // 2. もし現在の数が上限（maxZombies）より少なければ、新しいゾンビを湧かせる
//            if (currentZombieCount < maxZombies)
//            {
//                SpawnZombieInsideScreen();
//            }
//            timer = 0;
//        }
//    }
//    void SpawnZombieInsideScreen()
//    {
//        // 1. カメラが見ている画面の左端と右端のX座標を計算
//        float screenLeftX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x; 
//        float screenRightX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x; 
//        // 2. 画面のギリギリ端だとゾンビが半分見切れてしまうので、少し内側に余白（マージン）を作る
//        float margin = 1.0f; 
//        // 3. カメラの左端から右端の「内側」でランダムなX座標を決定
//        float spawnX = Random.Range(screenLeftX + margin, screenRightX - margin); 
//        // 4. 決定した座標にゾンビを生成
//        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0); 
//        Instantiate(zombiePrefab, spawnPosition, Quaternion.identity); 
//    }

//}
using UnityEngine;
public class ZombieSpawner : MonoBehaviour 
{ 
    public GameObject zombiePrefab; 
    // ゾンビのプレハブ
    public float spawnInterval = 5.0f; // 湧く間隔（秒
    [Header("上限設定")] 
    public int maxZombies = 4; 
    [Header("地面検知の設定")] public LayerMask groundLayer; 
    // インスペクターで「Ground」レイヤーを選択してください
    public float raySpawnY = 10f; // レイを飛ばす開始高度（一番高い地面より上に設定）
    public float yOffset = 0f; // ゾンビが地面に埋まる場合の微調整用（必要なら0.5などを入れる）
    private float timer; private Camera mainCamera; 
    private void Start() 
    {
        mainCamera = Camera.main; 
    } 
    void Update() 
    { 
        timer += Time.deltaTime; 
        if (timer >= spawnInterval) 
        { 
            // 1. 今ゲーム内にいるゾンビの数を数える
            int currentZombieCount = GameObject.FindGameObjectsWithTag("Enemy").Length; 
           // 2. もし現在の数が上限（maxZombies）より少なければ、新しいゾンビを湧かせる
           if (currentZombieCount < maxZombies) 
            {
                SpawnZombieInsideScreen(); 
            } 
            timer = 0; 
        } 
    } 
    void SpawnZombieInsideScreen() 
    { 
        // 1. カメラが見ている画面の左端と右端のX座標を計算
        float screenLeftX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x; 
        float screenRightX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x; 
        // 2. 画面のギリギリ端だとゾンビが半分見切れてしまうので、少し内側に余白（マージン）を作る
        float margin = 1.0f; 
        // 3. カメラの左端から右端の「内側」でランダムなX座標を決定
        float spawnX = Random.Range(screenLeftX + margin, screenRightX - margin); 
        // ★追加: 空中の高い位置から真下に向けてレイ（Raycast）を飛ばす
        Vector2 rayOrigin = new Vector2(spawnX, raySpawnY); 
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, groundLayer); 
        // ★追加: 地面に当たった場合のみゾンビを生成する
        if (hit.collider != null) 
        { 
            // hit.point.y で地面の正確な高さを取得
            Vector3 spawnPosition = new Vector3(spawnX, hit.point.y + yOffset, 0f); 
            Instantiate(zombiePrefab, spawnPosition, Quaternion.identity); 
        } 
        
    } 
}