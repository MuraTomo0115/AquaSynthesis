using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

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
	private PlayerMovement _playerMovement;
	private CoolTimeInput _coolTimeInput;
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
	private bool _isWaitingSE = false; // SE�Đ��ҋ@���t���O��ǉ�
	private Dictionary<string, string> _targetToSideMap = new Dictionary<string, string>();

	public bool IsPlaying => _isPlay; // �V�i���I���Đ������ǂ������擾����v���p�e�B

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

		_inputActions = new PlayerInputActions();  // PlayerInputActions�̃C���X�^���X���쐬
		_inputActions.ADV.Enable();  // ADV�}�b�v��L���ɂ���
		_canvasGroup = _advContents.GetComponent<CanvasGroup>();
		_seAudioSource = GetComponent<AudioSource>();
		_advContents.gameObject.SetActive(false);
		// StartScenario(_scenarioFileName);
		_inputActions.ADV.StartDemo.performed += ctx => StartScenario(_scenarioFileName);
	}

	private void Start()
	{
		_playerMovement = FindObjectOfType<PlayerMovement>();
		_coolTimeInput = FindObjectOfType<CoolTimeInput>();
	}

	void OnEnable() => _inputActions.ADV.Enable();  // �A�N�V�����}�b�v��L���ɂ���
	void OnDisable() => _inputActions.ADV.Disable();  // �A�N�V�����}�b�v�𖳌��ɂ���

	/// <summary>
	/// �V�i���I�Đ��J�n
	/// </summary>
	/// <param name="scenarioName">�Đ�����JSON�t�@�C����</param>
	public void StartScenario(string scenarioName)
	{
		if (_isPlay) return;

		_playerMovement.isCanAction = false;
		_coolTimeInput.SetCanRecord(false); // ゴーストの記録を無効化
		InputActionHolder.Instance.menuInputActions.Disable(); // ADV開始時にメニュー操作を無効化

		AudioManager.Instance.StopAllSE(); // ���ׂĂ�SE���~

		_isPlay = true;
		Time.timeScale = 0f;
		// UI���������ߋ��̎c����N���A
		_messageText.text = "";
		_speakerText.text = "";
		_imageLeft.sprite = null;
		_imageCenter.sprite = null;
		_imageRight.sprite = null;
		_imageLeft.color = new Color(1, 1, 1, 0);
		_imageCenter.color = new Color(1, 1, 1, 0);
		_imageRight.color = new Color(1, 1, 1, 0);
		_fadeOverlay.color = new Color(0, 0, 0, 0); // ���S�ɓ����ȍ�
		_fadeOverlay.gameObject.SetActive(false);   // ��\���ɂ��Ă���

		_steps.Clear(); // �V�i���I�L���[���N���A
		_isSkipping = false;
		_isMessageShowing = false;

		_nextIcon.gameObject.SetActive(false);
		_canvasGroup.alpha = 1f;
		_advContents.SetActive(true); // UI��\��

		_scenarioFileName = scenarioName;
		LoadScenario(_scenarioFileName);
		ShowNextStep();
	}

	/// <summary>
	/// �t�F�[�h�A�e�L�X�g�\��
	/// </summary>
	private void Update()
	{
		if (_isCooldown || _isFading || !_isPlay || _isWaitingSE) return; // �� �ǉ�

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
	/// �V�i���I�i�s�̃N�[���^�C������
	/// </summary>
	/// <returns></returns>
	private IEnumerator AdvanceCooldown()
	{
		_isCooldown = true;  // �N�[���_�E�����J�n
		yield return new WaitForSecondsRealtime(_advanceCooldown);
		// �w�肵�����ԑҋ@
		_isCooldown = false; // �N�[���_�E���I��
	}

	/// <summary>
	/// �V�i���I�t�@�C���ǂݍ���
	/// </summary>
	/// <param name="fileName">�ǂݍ��ރt�@�C����</param>
	private void LoadScenario(string fileName)
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
			_steps.Enqueue(step);
		}
	}

	/// <summary>
	/// ���̉�b�A�C�x���g���Đ�
	/// </summary>
	private void ShowNextStep()
	{
		if (_steps.Count == 0)
		{
			_isPlay = false;
			Time.timeScale = 1f; // �Q�[���̎��Ԃ����ɖ߂�
			InputActionHolder.Instance.playerInputActions.Enable();
			InputActionHolder.Instance.menuInputActions.Enable(); // ADV終了後にメニュー操作を有効化


			_playerMovement.isCanAction = true;
			_coolTimeInput.SetCanRecord(true); // ゴーストの記録を再度有効化

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
				StartCoroutine(FadeOverlay(1f, 0.5f)); // ���Ƀt�F�[�h
				break;

			case "fadein":
				StartCoroutine(FadeOverlay(0f, 0.5f)); // ��������
				break;

			case "se":
				StartCoroutine(PlaySEAndWait(step.clip, step.wait));
				break;

			case "hide":
				HideCharacter(step.target);
				ShowNextStep();
				break;

			default:
				UnityEngine.Debug.LogWarning("�s���ȃX�e�b�v�^�C�v: " + step.type);
				ShowNextStep();
				break;
		}
	}

	/// <summary>
	/// �t�F�[�h�I�[�o�[���C����
	/// </summary>
	/// <param name="targetAlpha">�Ώۂ̃�</param>
	/// <param name="duration">�t�F�[�h�Ԋu</param>
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
	/// SE�Đ�
	/// </summary>
	/// <param name="clipName">SE�t�@�C����</param>
	/// <param name="wait">true�F���ʉ��Đ���Ƀe�L�X�g�\�� false�F�e�L�X�g�ƈꏏ�ɍĐ�</param>
	/// <returns></returns>
	private IEnumerator PlaySEAndWait(string clipName, bool wait)
	{
		_isWaitingSE = true; // �� �ǉ�
		AudioClip clip = Resources.Load<AudioClip>("Audio/SE/" + clipName);
		if (clip == null)
		{
			UnityEngine.Debug.LogWarning("SE��������܂���: " + clipName);
			_isWaitingSE = false; // �� �ǉ�
			ShowNextStep();
			yield break;
		}
		_seAudioSource.PlayOneShot(clip);

		if (wait)
		{
			yield return new WaitForSecondsRealtime(clip.length);
		}

		_isWaitingSE = false; // �� �ǉ�
		ShowNextStep();
	}

	/// <summary>
	/// �e�L�X�g�\��
	/// </summary>
	/// <param name="speaker">�����Ă���l��</param>
	/// <param name="message">���e</param>
	/// <returns></returns>
	private IEnumerator ShowMessage(string speaker, string message)
	{
		_isMessageShowing = true;
		_speakerText.text = speaker;
		_messageText.text = "";

		for (int i = 0; i < message.Length; i++)
		{
			// ����^�O <wait> �����o������A���͑҂�����
			if (message.Substring(i).StartsWith("<wait>"))
			{
				i += "<wait>".Length - 1; // �^�O���X�L�b�v

				// ���͂�����܂őҋ@
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

		// �S���\����Ɂ��}�[�N��\��
		_nextIcon.gameObject.SetActive(true);
	}

	/// <summary>
	/// �L�����N�^�[�����G��\��
	/// </summary>
	/// <param name="target">�N��</param>
	/// <param name="spriteName">�摜�t�@�C����</param>
	/// <param name="side">�\������ʒu</param>
	/// <param name="transition">�g�����W�V�������ĕ\������</param>
	private void ShowCharacter(string target, string spriteName, string side, string transition)
	{
		// �ړ��O�̈ʒu�i�O�� side�j���\���ɂ���
		HideCharacterAtPreviousPosition(target);

		// �V�����ʒu�ɃL�����N�^�[�摜��\��
		Sprite newSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
		Image targetImage = FindImageByTarget(target, side);

		_targetToSideMap[target] = side;
		ChangeCharacterImage(targetImage, newSprite, transition); // �摜�؂�ւ��������Ăяo��
	}

	/// <summary>
	/// �ȑO�̈ʒu�̃L�����N�^�[�摜���\���ɂ��郁�\�b�h
	/// </summary>
	/// <param name="target">��\���ɂ���l��</param>
	private void HideCharacterAtPreviousPosition(string target)
	{
		// ���ݕ\������Ă���L�����N�^�[�̈ʒu�𒲂ׂ�
		if (_imageLeft.sprite != null && _imageLeft.sprite.name.Contains(target))
		{
			_imageLeft.color = new Color(1, 1, 1, 0); // �����̃L�����N�^�[���\���ɂ���
		}
		if (_imageCenter.sprite != null && _imageCenter.sprite.name.Contains(target))
		{
			_imageCenter.color = new Color(1, 1, 1, 0); // �����̃L�����N�^�[���\���ɂ���
		}
		if (_imageRight.sprite != null && _imageRight.sprite.name.Contains(target))
		{
			_imageRight.color = new Color(1, 1, 1, 0); // �E���̃L�����N�^�[���\���ɂ���
		}
	}

	/// <summary>
	/// �\�����Ă���L���������G�摜��u������
	/// </summary>
	/// <param name="target">�N��</param>
	/// <param name="spriteName">�摜�t�@�C����</param>
	/// <param name="side">�ʒu</param>
	/// <param name="transition">�g�����W�V�������ĕ\������</param>
	private void ChangeCharacterExpression(string target, string spriteName, string side, string transition)
	{
		// side���w�肳��Ă��Ȃ��ꍇ�́A�O��̕\���ʒu���g�p
		if (string.IsNullOrEmpty(side))
		{
			if (_targetToSideMap.ContainsKey(target))
			{
				side = _targetToSideMap[target];
			}
			else
			{
				return;
			}
		}

		Sprite newSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
		if (newSprite == null)
		{
			UnityEngine.Debug.LogError($"�X�v���C�g���ǂݍ��߂܂���ł���: Sprites/{spriteName}");
			return;
		}

		Image targetImage = FindImageByTarget(target, side);
		if (targetImage == null)
		{
			UnityEngine.Debug.LogError($"targetImage��null�ł� (target: {target}, side: {side})");
			return;
		}

		ChangeCharacterImage(targetImage, newSprite, transition);
	}

	/// <summary>
	/// �摜�̐؂�ւ��������܂Ƃ߂����\�b�h
	/// </summary>
	/// <param name="targetImage">�Ώۉ摜</param>
	/// <param name="newSprite">�u��������摜</param>
	/// <param name="transition">�g�����W�V�������ĕ\��</param>
	private void ChangeCharacterImage(Image targetImage, Sprite newSprite, string transition)
	{
		if (targetImage == null)
		{
			UnityEngine.Debug.LogError("�^�[�Q�b�g�摜��null�ł��Bside: " + targetImage.name);
			return; // �摜��null�̏ꍇ�͏����𒆎~
		}

		if (newSprite == null)
		{
			UnityEngine.Debug.LogError("�X�v���C�g��null�ł��BspriteName: " + newSprite.name);
			return; // �X�v���C�g��null�̏ꍇ�͏����𒆎~
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
			targetImage.color = new Color(1, 1, 1, 1); // �F�����ɖ߂�
		}
	}

	/// <summary>
	/// �u��������ۂɑO�̉摜���\���ɂ���
	/// </summary>
	/// <param name="target">�Ώۂ̐l��</param>
	private void HideCharacter(string target)
	{
		if (!_targetToSideMap.ContainsKey(target)) return;

		string side = _targetToSideMap[target];
		Image image = FindImageByTarget(target, side);

		if (image != null)
		{
			image.DOFade(0f, 0.3f).SetUpdate(true); // �t�F�[�h�A�E�g�Ŕ�\���ɂ���
		}
	}

	/// <summary>
	/// side����X���b�g���擾
	/// </summary>
	/// <param name="target">�Ώېl��</param>
	/// <param name="side">�ʒu</param>
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

	private string _pendingScenarioName;    // 次のボスシナリオを開始するための名前

	/// <summary>
	/// ボスシナリオを開始するためのメソッド
	/// 5秒後に指定されたシナリオを開始します。
	/// </summary>
	/// <param name="scenarioName"></param>
	public void BossScenario(string scenarioName)
	{
		_pendingScenarioName = scenarioName;
		Invoke(nameof(InvokeStartScenario), 5.0f);
	}

	/// <summary>
	/// 5秒後に指定されたシナリオを開始するためのメソッド
	/// </summary>
	/// <param name="scenarioName">開始するシナリオの名前</param>
	private void InvokeStartScenario()
	{
		StartScenario(_pendingScenarioName);
	}
}
