using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SupportKasumiStatus
{
	public float healRange;
	public float healAmount;
	public float healInterval;
	public float healDuration;
    public float availableTime;
}

public class SupportKasumi : SupportBase
{
	private float _healRange;
	private float _healAmount;
	private float _healInterval;
	private float _healDuration;
	private float _availableTime;
	private float _innerRadius;
    private CircleCollider2D _shieldCollider;

	[Header("�������U��")]
    [SerializeField] private LayerMask targetLayer;

	private Coroutine healCoroutine;
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();

    protected override void Awake()
	{
		base.Awake();
		LoadKasumiStatusFromTable();
        _shieldCollider = GetComponent<CircleCollider2D>();

        if (_shieldCollider != null)
        {
            _innerRadius = _healRange;
            _shieldCollider.radius = _innerRadius;
        }
    }

    /// <summary>
    /// SupportStatus�e�[�u������X�e�[�^�X��ǂݍ���
    /// </summary>
    private void LoadKasumiStatusFromTable()
    {
        // "Kasumi" �Ƃ������O��DB����擾
        var status = DatabaseManager.GetSupportStatusByName("Kasumi");
        if (status != null)
        {
            _healRange = status.HealRange ?? 0f;
            _healAmount = status.HealAmount ?? 0f;
            _healInterval = status.HealInterval ?? 0f;
            _healDuration = status.HealDuration ?? 0f;
            _availableTime = status.AvailableTime; // �ǉ�
            Debug.Log($"{_healAmount} {_healInterval} {_healDuration} {_healRange} {_availableTime}");
        }
        else
        {
            Debug.LogWarning("SupportStatus�e�[�u����Kasumi�̃f�[�^������܂���B");
        }
    }

    /// <summary>
    /// �񕜁E�V�[���h�͈͂ɓ������Ƃ��̏���
    /// </summary>
    /// <param name="other">�Փ˂����I�u�W�F�N�g</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        if (((1 << otherLayer) & targetLayer) == 0) return;

        Vector2 objPos = other.transform.position;
        Vector2 centerPos = transform.position;
        float dist = Vector2.Distance(objPos, centerPos);

        // �͈͂̔��a�̂������l�i�Ⴆ�Δ����̋����j��ݒ�
        float threshold = _healRange * 0.9f;

        if (dist > threshold)
        {
            // �O����������� �� ������
            Destroy(other.gameObject);
        }
        else
        {
            // ������������� �� �͈͊O�ɏo��܂Œǐ�
            trackedObjects.Add(other.gameObject);
        }
    }

    /// <summary>
    /// �񕜁E�V�[���h�͈͂���o���Ƃ��̏���
    /// </summary>
    /// <param name="other">�Փ˂����I�u�W�F�N�g</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        int otherLayer = other.gameObject.layer;
        if (((1 << otherLayer) & targetLayer) == 0) return;

        if (trackedObjects.Contains(other.gameObject))
        {
            trackedObjects.Remove(other.gameObject);
            Destroy(other.gameObject);
        }
    }

    /// <summary>
    /// �񕜃A�N�V�������J�n����
    /// </summary>
    private void OnEnable()
	{
		healCoroutine = StartCoroutine(HealNearbyPlayers());
        StartCoroutine(AvailableTimeWatcher());
	}

    /// <summary>
    /// �񕜃A�N�V�������~����
    /// </summary>
    private void OnDisable()
	{
		if (healCoroutine != null)
		{
			StopCoroutine(healCoroutine);
		}
	}

    /// <summary>
    /// �T�|�[�^�[���ʃA�N�V���������s����
    /// </summary>
    public override void Act()
	{
		// Act���̂ł͉������Ȃ��iOnEnable�Ŏ����J�n�j
	}

    /// <summary>
    /// �񕜃A�N�V���������s����R���[�`��
    /// </summary>
    /// <returns>�񕜃N�[���^�C��</returns>
    private IEnumerator HealNearbyPlayers()
	{
		float elapsed = 0f;
		while (elapsed < _healDuration)
		{
			Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _healRange);
			foreach (var hit in hits)
			{
				if (hit.CompareTag("Player"))
				{
					PlayerMovement player = hit.GetComponent<PlayerMovement>();
					if (player != null)
					{
						player.Heal(_healAmount);
					}
				}
			}
			yield return new WaitForSeconds(_healInterval);
			elapsed += _healInterval;
		}
		EndAct();
	}

    /// <summary>
    /// �o���\���Ԃ��Ď����A0�ɂȂ�����EndAct���Ă�
    /// </summary>
    private IEnumerator AvailableTimeWatcher()
    {
        float timer = _availableTime;
        while (timer > 0f)
        {
            yield return null;
            timer -= Time.deltaTime;
        }
        EndAct();
    }

    /// <summary>
    /// Gizmos��`�悷��i�G�f�B�^��ł̃f�o�b�O�p�j
    /// </summary>
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _healRange);
	}
}