using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Diagnostics;

public class ADVManager : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private TextMeshProUGUI _speakerText;
	[SerializeField] private TextMeshProUGUI _messageText;
	[SerializeField] private Image _imageLeft;
	[SerializeField] private Image _imageCenter;
	[SerializeField] private Image _imageRight;
	[SerializeField] private GameObject _nextIcon;
	[SerializeField] private Image _fadeOverlay;

	[Header("Scenario File Name")]
	[SerializeField] private string _scenarioFileName = "sample";

	[SerializeField] private GameObject _advContents;

	private Queue<ScenarioStep> _steps = new Queue<ScenarioStep>();
	private bool _isMessageShowing = false;
	private bool _isSkipping = false;
	private CanvasGroup _canvasGroup;
	private System.Action _OnScenarioFinished;
	private PlayerInputActions _inputActions;
	private AudioSource _seAudioSource;
	private bool _isCooldown = false;
	private float _advanceCooldown = 1f;
	private bool _isPlay = false;
	private bool _isFading = false;
	private bool _isWaitingSE = false; // SE再生待機中フラグを追加
	private Dictionary<string, string> _targetToSideMap = new Dictionary<string, string>();

	public bool IsPlaying => _isPlay; // シナリオが再生中かどうかを取得するプロパティ

    public static ADVManager Instance { get; private set; }

    private void Awake()
	{
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _inputActions = new PlayerInputActions();  // PlayerInputActionsのインスタンスを作成
		_inputActions.ADV.Enable();  // ADVマップを有効にする
		_canvasGroup = _advContents.GetComponent<CanvasGroup>();
	}

	void OnEnable() => _inputActions.ADV.Enable();  // アクションマップを有効にする
	void OnDisable() => _inputActions.ADV.Disable();  // アクションマップを無効にする

	private void Start()
	{
		_seAudioSource = GetComponent<AudioSource>();
		_advContents.gameObject.SetActive(false);
		// StartScenario(_scenarioFileName);
		_inputActions.ADV.StartDemo.performed += ctx => StartScenario(_scenarioFileName);
	}

	/// <summary>
	/// シナリオ再生開始
	/// </summary>
	/// <param name="scenarioName">再生するJSONファイル名</param>
	public void StartScenario(string scenarioName)
	{
		if (_isPlay) return;

		AudioManager.Instance.StopAllSE(); // すべてのSEを停止

        _isPlay = true;
		Time.timeScale = 0f;
                              // UI初期化＆過去の残りをクリア
        _messageText.text = "";
		_speakerText.text = "";
		_imageLeft.sprite = null;
		_imageCenter.sprite = null;
		_imageRight.sprite = null;
		_imageLeft.color = new Color(1, 1, 1, 0);
		_imageCenter.color = new Color(1, 1, 1, 0);
		_imageRight.color = new Color(1, 1, 1, 0);
		_fadeOverlay.color = new Color(0, 0, 0, 0); // 完全に透明な黒
		_fadeOverlay.gameObject.SetActive(false);   // 非表示にしておく

		_steps.Clear(); // シナリオキューをクリア
		_isSkipping = false;
		_isMessageShowing = false;

		_nextIcon.gameObject.SetActive(false);
		_canvasGroup.alpha = 1f;
		_advContents.SetActive(true); // UIを表示

		_scenarioFileName = scenarioName;
		LoadScenario(_scenarioFileName);
		ShowNextStep();
	}

	/// <summary>
	/// フェード、テキスト表示
	/// </summary>
	private void Update()
	{
		if (_isCooldown || _isFading || !_isPlay || _isWaitingSE) return; // ← 追加

		bool isAdvancePressed = _inputActions.ADV.Advance.triggered;
		bool isHoldSpeedUp = _inputActions.ADV.Advance.ReadValue<float>() > 0.5f;

		if (_isMessageShowing)
		{
			_isSkipping = isHoldSpeedUp;
		}
		else if (isAdvancePressed)
		{
			ShowNextStep();
			StartCoroutine(AdvanceCooldown());
			_nextIcon.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// シナリオ進行のクールタイム処理
	/// </summary>
	/// <returns></returns>
	private IEnumerator AdvanceCooldown()
	{
		_isCooldown = true;  // クールダウンを開始
        yield return new WaitForSecondsRealtime(_advanceCooldown);
        // 指定した時間待機
        _isCooldown = false; // クールダウン終了
	}

	/// <summary>
	/// シナリオファイル読み込み
	/// </summary>
	/// <param name="fileName">読み込むファイル名</param>
	private void LoadScenario(string fileName)
	{
		TextAsset json = Resources.Load<TextAsset>("Scenarios/" + fileName);
		if (json == null)
		{
			UnityEngine.Debug.LogError("シナリオファイルが見つかりません: " + fileName);
			return;
		}

		ScenarioWrapper wrapper = JsonUtility.FromJson<ScenarioWrapper>("{\"steps\":" + json.text + "}");
		foreach (var step in wrapper.steps)
		{
			_steps.Enqueue(step);
		}
	}

	/// <summary>
	/// 次の会話、イベントを再生
	/// </summary>
	private void ShowNextStep()
	{
		if (_steps.Count == 0)
		{
			UnityEngine.Debug.Log("シナリオ終了");
			_isPlay = false;
			Time.timeScale = 1f; // ゲームの時間を元に戻す

            _canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
			{
				_advContents.SetActive(false);
				_canvasGroup.alpha = 1f;
				_OnScenarioFinished?.Invoke();
			});
			return;
		}

		var step = _steps.Dequeue();

		switch (step.type)
		{
			case "text":
				StartCoroutine(ShowMessage(step.speaker, step.text));
				break;

			case "show":
				ShowCharacter(step.target, step.sprite, step.side, step.transition);
				ShowNextStep();
				break;

			case "expression":
				ChangeCharacterExpression(step.target, step.sprite, step.side, step.transition);
				ShowNextStep();
				break;

			case "fadeout":
				StartCoroutine(FadeOverlay(1f, 0.5f)); // 黒にフェード
				break;

			case "fadein":
				StartCoroutine(FadeOverlay(0f, 0.5f)); // 黒を消す
				break;

			case "se":
				StartCoroutine(PlaySEAndWait(step.clip, step.wait));
				break;

			case "hide":
				HideCharacter(step.target);
				ShowNextStep();
				break;

			default:
				UnityEngine.Debug.LogWarning("不明なステップタイプ: " + step.type);
				ShowNextStep();
				break;
		}
	}

	/// <summary>
	/// フェードオーバーレイ処理
	/// </summary>
	/// <param name="targetAlpha">対象のα</param>
	/// <param name="duration">フェード間隔</param>
	/// <returns></returns>
	private IEnumerator FadeOverlay(float targetAlpha, float duration)
	{
		_isFading = true;
		_fadeOverlay.gameObject.SetActive(true);

        yield return _fadeOverlay.DOFade(targetAlpha, duration).SetUpdate(true).WaitForCompletion();

        if (targetAlpha == 0f)
		{
			_fadeOverlay.gameObject.SetActive(false);
		}

		_isFading = false;
		ShowNextStep();
	}

	/// <summary>
	/// SE再生
	/// </summary>
	/// <param name="clipName">SEファイル名</param>
	/// <param name="wait">true：効果音再生後にテキスト表示 false：テキストと一緒に再生</param>
	/// <returns></returns>
	private IEnumerator PlaySEAndWait(string clipName, bool wait)
	{
		_isWaitingSE = true; // ← 追加
		AudioClip clip = Resources.Load<AudioClip>("Audio/SE/" + clipName);
		if (clip == null)
		{
			UnityEngine.Debug.LogWarning("SEが見つかりません: " + clipName);
			_isWaitingSE = false; // ← 追加
			ShowNextStep();
			yield break;
		}
		_seAudioSource.PlayOneShot(clip);

    if (wait)
    {
        yield return new WaitForSecondsRealtime(clip.length);
    }

    _isWaitingSE = false; // ← 追加
    ShowNextStep();
	}

	/// <summary>
	/// テキスト表示
	/// </summary>
	/// <param name="speaker">離している人物</param>
	/// <param name="message">内容</param>
	/// <returns></returns>
	private IEnumerator ShowMessage(string speaker, string message)
	{
		_isMessageShowing = true;
		_speakerText.text = speaker;
		_messageText.text = "";

		for (int i = 0; i < message.Length; i++)
		{
			// 特殊タグ <wait> を検出したら、入力待ちする
			if (message.Substring(i).StartsWith("<wait>"))
			{
				i += "<wait>".Length - 1; // タグ分スキップ

				// 入力があるまで待機
				yield return new WaitUntil(() => _inputActions.ADV.Advance.triggered);
			}
			else
			{
				_messageText.text += message[i];
                float wait = _isSkipping ? 0.005f : 0.05f;
                yield return new WaitForSecondsRealtime(wait);
            }
		}

		_isMessageShowing = false;
		_isSkipping = false;

		// 全文表示後に▼マークを表示
		_nextIcon.gameObject.SetActive(true);
	}

	/// <summary>
	/// キャラクター立ち絵を表示
	/// </summary>
	/// <param name="target">誰の</param>
	/// <param name="spriteName">画像ファイル名</param>
	/// <param name="side">表示する位置</param>
	/// <param name="transition">トランジションして表示する</param>
	private void ShowCharacter(string target, string spriteName, string side, string transition)
	{
		// 移動前の位置（前の side）を非表示にする
		HideCharacterAtPreviousPosition(target);

		// 新しい位置にキャラクター画像を表示
		Sprite newSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
		Image targetImage = FindImageByTarget(target, side);

		_targetToSideMap[target] = side;
		ChangeCharacterImage(targetImage, newSprite, transition); // 画像切り替え処理を呼び出す
	}

	/// <summary>
	/// 以前の位置のキャラクター画像を非表示にするメソッド
	/// </summary>
	/// <param name="target">非表示にする人物</param>
	private void HideCharacterAtPreviousPosition(string target)
	{
		// 現在表示されているキャラクターの位置を調べる
		if (_imageLeft.sprite != null && _imageLeft.sprite.name.Contains(target))
		{
			_imageLeft.color = new Color(1, 1, 1, 0); // 左側のキャラクターを非表示にする
		}
		if (_imageCenter.sprite != null && _imageCenter.sprite.name.Contains(target))
		{
			_imageCenter.color = new Color(1, 1, 1, 0); // 中央のキャラクターを非表示にする
		}
		if (_imageRight.sprite != null && _imageRight.sprite.name.Contains(target))
		{
			_imageRight.color = new Color(1, 1, 1, 0); // 右側のキャラクターを非表示にする
		}
	}

	/// <summary>
	/// 表示しているキャラ立ち絵画像を置き換え
	/// </summary>
	/// <param name="target">誰の</param>
	/// <param name="spriteName">画像ファイル名</param>
	/// <param name="side">位置</param>
	/// <param name="transition">トランジションして表示する</param>
	private void ChangeCharacterExpression(string target, string spriteName, string side, string transition)
	{
		// sideが指定されていない場合は、前回の表示位置を使用
		if (string.IsNullOrEmpty(side))
		{
			if (_targetToSideMap.ContainsKey(target))
			{
				side = _targetToSideMap[target];
			}
			else
			{
				UnityEngine.Debug.LogWarning($"sideが指定されておらず、{target} に前回の表示位置も見つかりません。");
				return;
			}
		}

		Sprite newSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
		if (newSprite == null)
		{
			UnityEngine.Debug.LogError($"スプライトが読み込めませんでした: Sprites/{spriteName}");
			return;
		}

		Image targetImage = FindImageByTarget(target, side);
		if (targetImage == null)
		{
			UnityEngine.Debug.LogError($"targetImageがnullです (target: {target}, side: {side})");
			return;
		}

		ChangeCharacterImage(targetImage, newSprite, transition);
	}

	/// <summary>
	/// 画像の切り替え処理をまとめたメソッド
	/// </summary>
	/// <param name="targetImage">対象画像</param>
	/// <param name="newSprite">置き換える画像</param>
	/// <param name="transition">トランジションして表示</param>
	private void ChangeCharacterImage(Image targetImage, Sprite newSprite, string transition)
	{
		if (targetImage == null)
		{
			UnityEngine.Debug.LogError("ターゲット画像がnullです。side: " + targetImage.name);
			return; // 画像がnullの場合は処理を中止
		}

		if (newSprite == null)
		{
			UnityEngine.Debug.LogError("スプライトがnullです。spriteName: " + newSprite.name);
			return; // スプライトがnullの場合は処理を中止
		}

		if (transition == "fade")
		{
            targetImage.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                targetImage.sprite = newSprite;
                targetImage.DOFade(1f, 0.3f).SetUpdate(true);
            });
        }
        else
		{
			targetImage.sprite = newSprite;
			targetImage.color = new Color(1, 1, 1, 1); // 色を元に戻す
		}
	}

	/// <summary>
	/// 置き換える際に前の画像を非表示にする
	/// </summary>
	/// <param name="target">対象の人物</param>
	private void HideCharacter(string target)
	{
		if (!_targetToSideMap.ContainsKey(target)) return;

		string side = _targetToSideMap[target];
		Image image = FindImageByTarget(target, side);

		if (image != null)
		{
            image.DOFade(0f, 0.3f).SetUpdate(true); // フェードアウトで非表示にする
        }
    }

	/// <summary>
	/// sideからスロットを取得
	/// </summary>
	/// <param name="target">対象人物</param>
	/// <param name="side">位置</param>
	/// <returns></returns>
	Image FindImageByTarget(string target, string side)
	{
		switch (side)
		{
			case "left": return _imageLeft;
			case "center": return _imageCenter;
			case "right": return _imageRight;
			default:
				return null;
		}
	}

	[System.Serializable]
	public class ScenarioStep
	{
		public string type;
		public string speaker;
		public string text;
		public string target;
		public string sprite;
		public string side;
		public string transition;
		public string clip;
		public bool wait;
	}

	[System.Serializable]
	public class ScenarioWrapper
	{
		public ScenarioStep[] steps;
	}
}
