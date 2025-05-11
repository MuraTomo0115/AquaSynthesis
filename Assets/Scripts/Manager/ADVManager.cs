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

	private PlayerInputActions inputActions; // PlayerInputActions�̃C���X�^���X

	private bool isCooldown = false; // �N�[���_�E����Ԃ��ǂ���
	private float advanceCooldown = 1f; // �N�[���_�E������ (�b)
	private bool _is_Play = false;

	void Awake()
	{
		inputActions = new PlayerInputActions();  // PlayerInputActions�̃C���X�^���X���쐬
		inputActions.ADV.Enable();  // ADV�}�b�v��L���ɂ���
		_canvasGroup = _advContents.GetComponent<CanvasGroup>();
	}

	void OnEnable() => inputActions.ADV.Enable();  // �A�N�V�����}�b�v��L���ɂ���
	void OnDisable() => inputActions.ADV.Disable();  // �A�N�V�����}�b�v�𖳌��ɂ���

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
		// UI���������ߋ��̎c����N���A
		messageText.text = "";
		speakerText.text = "";
		imageLeft.sprite = null;
		imageCenter.sprite = null;
		imageRight.sprite = null;
		imageLeft.color = new Color(1, 1, 1, 0);
		imageCenter.color = new Color(1, 1, 1, 0);
		imageRight.color = new Color(1, 1, 1, 0);

		steps.Clear(); // �V�i���I�L���[���N���A
		isSkipping = false;
		isMessageShowing = false;

		nextIcon.gameObject.SetActive(false);
		_canvasGroup.alpha = 1f;
		_advContents.SetActive(true); // UI��\��

		scenarioFileName = scenarioName;
		LoadScenario(scenarioFileName);
		ShowNextStep();
	}

	void Update()
	{
		// �N�[���_�E�����łȂ����m�F
		if (isCooldown)
			return;

		// 'Advance'�A�N�V�������g���K�[���ꂽ���m�F
		bool isAdvancePressed = inputActions.ADV.Advance.triggered;

		// 'Advance'�A�N�V������������Ă��邩�i����������Ă��邩�j���m�F
		bool isHoldSpeedUp = inputActions.ADV.Advance.ReadValue<float>() > 0.5f;

		if (isMessageShowing)
		{
			isSkipping = isHoldSpeedUp;
		}
		else if (isAdvancePressed)
		{
			ShowNextStep();  // ���̃X�e�b�v��\��
			StartCoroutine(AdvanceCooldown()); // �N�[���_�E���J�n
			nextIcon.gameObject.SetActive(false); // ���A�C�R�����\���ɂ���
		}
	}

	IEnumerator AdvanceCooldown()
	{
		isCooldown = true;  // �N�[���_�E�����J�n
		yield return new WaitForSeconds(advanceCooldown); // �w�肵�����ԑҋ@
		isCooldown = false; // �N�[���_�E���I��
	}

	void LoadScenario(string fileName)
	{
		TextAsset json = Resources.Load<TextAsset>("Scenarios/" + fileName);
		if (json == null)
		{
			UnityEngine.Debug.LogError("�V�i���I�t�@�C����������܂���: " + fileName);
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
			UnityEngine.Debug.Log("�V�i���I�I��");
			_is_Play = false;

			// ADV���t�F�[�h�A�E�g���Ĕ�\���ɂ���
			_canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
			{
				_advContents.SetActive(false);
				_canvasGroup.alpha = 1f; // ����ĕ\���̂��ߓ����x���Z�b�g
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
				UnityEngine.Debug.LogWarning("�s���ȃX�e�b�v�^�C�v: " + step.type);
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

		// ��b���I������灤�A�C�R����\��
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
		// ���͊ȈՓI�� hero �� ��
		if (target == "hero") return imageLeft;
		// ���� target ����������Ή���ǉ�
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
