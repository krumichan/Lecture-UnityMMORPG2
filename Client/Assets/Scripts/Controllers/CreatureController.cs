using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CreatureController : MonoBehaviour
{
    [SerializeField]
    public float _speed = 5.0f;

    public Vector3Int CellPosition { get; set; } = Vector3Int.zero;

    protected Animator _animator;
    protected SpriteRenderer _sprite;

    [SerializeField]
    protected CreatureState _state = CreatureState.Idle;
    public virtual CreatureState State
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

    protected MoveDir _lastDir = MoveDir.Down;
    [SerializeField]
    protected MoveDir _dir = MoveDir.Down;
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

    public MoveDir GetDirectionFromVector(Vector3Int direction)
    {
        if (direction.x > 0)
        {
            return MoveDir.Right;
        }
        else if (direction.x < 0)
        {
            return MoveDir.Left;
        }
        else if (direction.y > 0)
        {
            return MoveDir.Up;
        }
        else if (direction.y < 0)
        {
            return MoveDir.Down;
        }
        else
        {
            return MoveDir.None;
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

    }

    protected virtual void UpdateMoving()
    {
        Vector3 destWorldPos = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destWorldPos - transform.position;

        // µµÂø ¿©ºÎ
        float dist = moveDir.magnitude; // ¹æÇâ º¤ÅÍ Å©±â
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destWorldPos;
            MoveToNextPosition();
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
    }

    protected virtual void MoveToNextPosition()
    {
        if (_dir == MoveDir.None)
        {
            State = CreatureState.Idle;
            return;
        }

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

        if (Managers.Map.CanMove(destination))
        {
            if (Managers.Object.FindCreature(destination) == null)
            {
                CellPosition = destination;
            }
        }
    }

    protected virtual void UpdateSkill()
    {

    }

    protected virtual void UpdateDead()
    {

    }

    public virtual void OnDamaged()
    {

    }
}
