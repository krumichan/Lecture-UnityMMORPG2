using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    public float _speed = 5.0f;

    public Vector3Int CellPosition { get; set; } = Vector3Int.zero;

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

    public Vector3Int GetFrontCellPosition()
    {
        Vector3Int cellPosition = CellPosition;
        
        switch (_lastDir)
        {
            case MoveDir.Up:
                cellPosition += Vector3Int.up;
                break;

            case MoveDir.Down:
                cellPosition += Vector3Int.down;
                break;

            case MoveDir.Left:
                cellPosition += Vector3Int.left;
                break;

            case MoveDir.Right:
                cellPosition += Vector3Int.right;
                break;
        }

        return cellPosition;
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
            switch (_lastDir)
            {
                case MoveDir.Up:
                    PlayWithFlip("ATTACK_BACK");
                    break;

                case MoveDir.Down:
                    PlayWithFlip("ATTACK_FRONT");
                    break;

                case MoveDir.Left:
                    PlayWithFlip("ATTACK_RIGHT", true);
                    break;

                case MoveDir.Right:
                    PlayWithFlip("ATTACK_RIGHT");
                    break;
            }
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
        Vector3 worldPos = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0.5f);
        transform.position = worldPos;
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;

            case CreatureState.Moving:
                UpdateMoving();
                break;

            case CreatureState.Skill:
                UpdateSkill();
                break;

            case CreatureState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle()
    {
        if (_dir != MoveDir.None)
        {
            Vector3Int destination = CellPosition;
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

            State = CreatureState.Moving;

            if (Managers.Map.CanMove(destination))
            {
                if (Managers.Object.FindCreature(destination) == null)
                {
                    CellPosition = destination;
                }
            }
        }
    }

    protected virtual void UpdateMoving()
    {
        Vector3 destWorldPos = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destWorldPos - transform.position;

        // 도착 여부
        float dist = moveDir.magnitude; // 방향 벡터 크기
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destWorldPos;

            // 예외적으로 Animation을 직접 Control.
            // 이유 : 이동 시, Idle -> Moving(목표위치로) -> Idle(목표위치도착) -> Moving(목표위치로) -> Idle(목표위치도착) ...
            // 위와 같이 되어, Moving Animation이 약간 끊기는 현상이 발생.
            _state = CreatureState.Idle;

            // 진짜로 멈추었을 경우, Idle로 설정.
            // 위의 _state = CreatureState.Idle; 만 수행할 경우,
            // Moving -> Idle이 되었을 시, Animation이 Update 되지 않음.
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

    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {

    }
}
