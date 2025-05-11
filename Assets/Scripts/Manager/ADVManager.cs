using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Diagnostics;

public class ADVManager : MonoBehaviour
{
	[Header("UI References")]
	public Text speakerText;
	public Text messageText;
	public Image imageLeft;
	public Image imageCenter;
	public Image imageRight;
	public GameObject nextIcon;

	[Header("Scenario")]
	public string scenarioFileName = "sample";

	[SerializeField]
	private GameObject _advContents;

	private Queue<ScenarioStep> steps = new Queue<ScenarioStep>();
	private bool isMessageShowing = false;
	private bool isSkipping = false;
	private CanvasGroup _canvasGroup;

	public System.Action OnScenarioFinished;

	private PlayerInputActions inputActions; // PlayerInputActionsのインスタンス

	private bool isCooldown = false; // クールダウン状態かどうか
	private float advanceCooldown = 1f; // クールダウン時間 (秒)
	private bool _is_Play = false;

	void Awake()
	{
		inputActions = new PlayerInputActions();  // PlayerInputActionsのインスタンスを作成
		inputActions.ADV.Enable();  // ADVマップを有効にする
		_canvasGroup = _advContents.GetComponent<CanvasGroup>();
	}

	void OnEnable() => inputActions.ADV.Enable();  // アクションマップを有効にする
	void OnDisable() => inputActions.ADV.Disable();  // アクションマップを無効にする

	void Start()
	{
		_advContents.gameObject.SetActive(false);
		// StartScenario(scenarioFileName);
		inputActions.ADV.StartDemo.performed += ctx => StartScenario(scenarioFileName);
	}

	public void StartScenario(string scenarioName)
	{
		if (_is_Play) return;

		_is_Play = true;
		// UI初期化＆過去の残りをクリア
		messageText.text = "";
		speakerText.text = "";
		imageLeft.sprite = null;
		imageCenter.sprite = null;
		imageRight.sprite = null;
		imageLeft.color = new Color(1, 1, 1, 0);
		imageCenter.color = new Color(1, 1, 1, 0);
		imageRight.color = new Color(1, 1, 1, 0);

		steps.Clear(); // シナリオキューをクリア
		isSkipping = false;
		isMessageShowing = false;

		nextIcon.gameObject.SetActive(false);
		_canvasGroup.alpha = 1f;
		_advContents.SetActive(true); // UIを表示

		scenarioFileName = scenarioName;
		LoadScenario(scenarioFileName);
		ShowNextStep();
	}

	void Update()
	{
		// クールダウン中でないか確認
		if (isCooldown)
			return;

		// 'Advance'アクションがトリガーされたか確認
		bool isAdvancePressed = inputActions.ADV.Advance.triggered;

		// 'Advance'アクションが押されているか（長押しされているか）を確認
		bool isHoldSpeedUp = inputActions.ADV.Advance.ReadValue<float>() > 0.5f;

		if (isMessageShowing)
		{
			isSkipping = isHoldSpeedUp;
		}
		else if (isAdvancePressed)
		{
			ShowNextStep();  // 次のステップを表示
			StartCoroutine(AdvanceCooldown()); // クールダウン開始
			nextIcon.gameObject.SetActive(false); // ▽アイコンを非表示にする
		}
	}

	IEnumerator AdvanceCooldown()
	{
		isCooldown = true;  // クールダウンを開始
		yield return new WaitForSeconds(advanceCooldown); // 指定した時間待機
		isCooldown = false; // クールダウン終了
	}

	void LoadScenario(string fileName)
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
			steps.Enqueue(step);
		}
	}

	void ShowNextStep()
	{
		if (steps.Count == 0)
		{
			UnityEngine.Debug.Log("シナリオ終了");
			_is_Play = false;

			// ADVをフェードアウトして非表示にする
			_canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
			{
				_advContents.SetActive(false);
				_canvasGroup.alpha = 1f; // 次回再表示のため透明度リセット
				OnScenarioFinished?.Invoke();
			});

			return;
		}
		var step = steps.Dequeue();

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
				ChangeCharacterExpression(step.target, step.sprite, step.transition);
				ShowNextStep();
				break;

			default:
				UnityEngine.Debug.LogWarning("不明なステップタイプ: " + step.type);
				ShowNextStep();
				break;
		}
	}

	IEnumerator ShowMessage(string speaker, string message)
	{
		isMessageShowing = true;
		speakerText.text = speaker;
		messageText.text = "";

		foreach (char c in message)
		{
			messageText.text += c;
			float wait = isSkipping ? 0.005f : 0.05f;
			yield return new WaitForSeconds(wait);
		}

		messageText.text = message;
		isMessageShowing = false;
		isSkipping = false;

		// 会話が終わったら▽アイコンを表示
		nextIcon.gameObject.SetActive(true);
	}

	void ShowCharacter(string target, string spriteName, string side, string transition)
	{
		Sprite newSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
		Image targetImage = GetImageBySide(side);

		if (transition == "fade")
		{
			targetImage.DOFade(0f, 0.3f).OnComplete(() =>
			{
				targetImage.sprite = newSprite;
				targetImage.DOFade(1f, 0.3f);
			});
		}
		else
		{
			targetImage.sprite = newSprite;
			targetImage.color = new Color(1, 1, 1, 1);
		}
	}

	void ChangeCharacterExpression(string target, string spriteName, string transition)
	{
		Sprite newSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
		Image targetImage = FindImageByTarget(target);

		if (transition == "fade")
		{
			targetImage.DOFade(0f, 0.2f).OnComplete(() =>
			{
				targetImage.sprite = newSprite;
				targetImage.DOFade(1f, 0.2f);
			});
		}
		else
		{
			targetImage.sprite = newSprite;
			targetImage.color = new Color(1, 1, 1, 1);
		}
	}

	Image GetImageBySide(string side)
	{
		switch (side)
		{
			case "left": return imageLeft;
			case "center": return imageCenter;
			case "right": return imageRight;
			default: return imageLeft;
		}
	}

	Image FindImageByTarget(string target)
	{
		// 今は簡易的に hero → 左
		if (target == "hero") return imageLeft;
		// 他に target が増えたら対応を追加
		return imageRight;
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
	}

	[System.Serializable]
	public class ScenarioWrapper
	{
		public ScenarioStep[] steps;
	}
}
