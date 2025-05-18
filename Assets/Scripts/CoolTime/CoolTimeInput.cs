using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class CoolTimeInput : MonoBehaviour
{
    public Image coolDownImage;                  // クールタイム表示用の画像（fillAmountを使って円形ゲージにする）
    public TextMeshProUGUI coolDownText;        // クールタイムの残り時間を表示するテキスト
    public float coolTime = 5f;                  // クールタイムの長さ（秒）
    private bool _isCoolingDown = false;          // クールタイム中かどうかの状態管理（private変数は頭に_）

    public Key triggerKey = Key.Q;                // クールタイム開始トリガーとなるキー（デフォルトはQキー）

    /// <summary>
    /// 初期化処理。
    /// クールタイムUIを非表示にし、fillAmountをリセットする
    /// </summary>
    void Start()
    {
        // クールタイムUIを最初は非表示にする
        coolDownImage.gameObject.SetActive(false);
        coolDownText.gameObject.SetActive(false);

        // fillAmountを0にしてゲージを空の状態にリセット
        coolDownImage.fillAmount = 0f;
    }

    /// <summary>
    /// 毎フレーム呼ばれる更新処理。
    /// 指定したキーが押されたらクールタイム処理を開始する
    /// </summary>
    void Update()
    {
        // 指定キーが押されていて、かつクールタイム中でなければクールタイム開始
        if (Keyboard.current[triggerKey].wasPressedThisFrame && !_isCoolingDown)
        {
            Debug.Log("クールタイム発動！");
            StartCoroutine(StartCoolDown());
        }
    }

    /// <summary>
    /// クールタイム処理を開始し、UI表示と残り時間の更新を行うコルーチン。
    /// クールタイム終了後はUIを非表示に戻し、状態を解除する
    /// </summary>
    /// <returns>IEnumerator コルーチン用の列挙子</returns>
    private System.Collections.IEnumerator StartCoolDown()
    {
        _isCoolingDown = true;  // クールタイム状態に切り替え

        // クールタイムUIを表示（ゲージとテキスト）
        coolDownImage.gameObject.SetActive(true);
        coolDownText.gameObject.SetActive(true);

        // タイマーにクールタイムの秒数をセット
        float timer = coolTime;

        // クールタイムが0になるまで繰り返す
        while (timer > 0f)
        {
            timer -= Time.deltaTime;                         // 毎フレーム経過時間を引く
            float ratio = Mathf.Clamp01(timer / coolTime);  // 残り時間の割合を0～1で計算
            coolDownImage.fillAmount = ratio;                // fillAmountで円ゲージの残り時間を表示
            coolDownText.text = timer.ToString("F1");       // 残り秒数を小数点1桁で表示

            yield return null;  // 次のフレームまで待機
        }

        // クールタイム終了したらUIを非表示に戻す
        coolDownImage.fillAmount = 0f;
        coolDownImage.gameObject.SetActive(false);
        coolDownText.gameObject.SetActive(false);

        _isCoolingDown = false;  // クールタイム状態を解除
    }
}
