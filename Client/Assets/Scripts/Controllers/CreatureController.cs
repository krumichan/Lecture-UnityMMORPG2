using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    public float _speed = 5.0f;

    protected Vector3Int _cellPos = Vector3Int.zero;
    protected Animator _animator;
    protected SpriteRenderer _sprite;

    CreatureState _state = CreatureState.Idle;
    public CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;

            UpdateAnimation();
        }
    }

    MoveDir _lastDir = MoveDir.Down;
    MoveDir _dir = MoveDir.Down;
    public MoveDir Dir
    {
        get { return _dir; }
        set
        {
            if (_dir == value)
            {
                return;
            }

            _dir = value;
            if (value != MoveDir.None)
            {
                _lastDir = value;
            }

            UpdateAnimation();
        }
    }

    protected virtual void UpdateAnimation()
    {
        if (_state == CreatureState.Idle)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    PlayWithFlip("IDLE_BACK");
                    break;

                case MoveDir.Down:
                    PlayWithFlip("IDLE_FRONT");
                    break;

                case MoveDir.Left:
                    PlayWithFlip("IDLE_RIGHT", true);
                    break;

                case MoveDir.Right:
                default:
                    PlayWithFlip("IDLE_RIGHT");
                    break;
            }
        }
        else if (_state == CreatureState.Moving)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    PlayWithFlip("WALK_BACK");
                    break;

                case MoveDir.Down:
                    PlayWithFlip("WALK_FRONT");
                    break;

                case MoveDir.Left:
                    PlayWithFlip("WALK_RIGHT", true);
                    break;

                case MoveDir.Right:
                    PlayWithFlip("WALK_RIGHT");
                    break;
            }
        }
        else if (_state == CreatureState.Skill)
        {
            // TODO
        }
        else
        {

        }
    }

    protected virtual void PlayWithFlip(string stateName, bool flipX = false)
    {
        _animator.Play(stateName);
        _sprite.flipX = flipX;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 worldPos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        transform.position = worldPos;
    }

    protected virtual void UpdateController()
    {
        UpdatePosition();
        UpdateIsMoving();
    }

    void UpdatePosition()
    {
        if (State != CreatureState.Moving)
        {
            return;
        }

        Vector3 destWorldPos = Managers.Map.CurrentGrid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destWorldPos - transform.position;

        // ���� ����
        float dist = moveDir.magnitude; // ���� ���� ũ��
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destWorldPos;

            // ���������� Animation�� ���� Control.
            // ���� : �̵� ��, Idle -> Moving(��ǥ��ġ��) -> Idle(��ǥ��ġ����) -> Moving(��ǥ��ġ��) -> Idle(��ǥ��ġ����) ...
            // ���� ���� �Ǿ�, Moving Animation�� �ణ ����� ������ �߻�.
            _state = CreatureState.Idle;

            // ��¥�� ���߾��� ���, Idle�� ����.
            // ���� _state = CreatureState.Idle; �� ������ ���,
            // Moving -> Idle�� �Ǿ��� ��, Animation�� Update ���� ����.
            if (_dir == MoveDir.None)
            {
                UpdateAnimation();
            }
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
    }

    void UpdateIsMoving()
    {
        if (State == CreatureState.Idle && _dir != MoveDir.None)
        {
            Vector3Int destination = _cellPos;
            switch (_dir)
            {
                case MoveDir.Up:
                    destination += Vector3Int.up;
                    break;

                case MoveDir.Down:
                    destination += Vector3Int.down;
                    break;

                case MoveDir.Left:
                    destination += Vector3Int.left;
                    break;

                case MoveDir.Right:
                    destination += Vector3Int.right;
                    break;
            }

            if (Managers.Map.CanMove(destination))
            {
                _cellPos = destination;
                State = CreatureState.Moving;
            }
        }
    }
}
