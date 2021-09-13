using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    public Grid _grid = null;
    public float _speed = 5.0f;

    Vector3Int _cellPos = Vector3Int.zero;
    bool _isMoving = false;
    Animator _animator;

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

            switch (value)
            {
                case MoveDir.Up:
                    _animator.Play("WALK_BACK");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;

                case MoveDir.Down:
                    _animator.Play("WALK_FRONT");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;

                case MoveDir.Left:
                    _animator.Play("WALK_RIGHT");
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    break;

                case MoveDir.Right:
                    _animator.Play("WALK_RIGHT");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;

                case MoveDir.None:
                {
                    switch (_dir)
                    {
                        case MoveDir.Up:
                            _animator.Play("IDLE_BACK");
                            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            break;

                        case MoveDir.Down:
                            _animator.Play("IDLE_FRONT");
                            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            break;

                        case MoveDir.Left:
                            _animator.Play("IDLE_RIGHT");
                            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                            break;

                        case MoveDir.Right:
                        default:
                            _animator.Play("IDLE_RIGHT");
                            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            break;
                    }
                }
                    break;
            }

            _dir = value;
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 worldPos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        transform.position = worldPos;
    }

    void Update()
    {
        GetDirInput();
        UpdatePosition();
        UpdateIsMoving();
    }

    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
        }
    }

    void UpdatePosition()
    {
        if (_isMoving == false)
        {
            return;
        }

        Vector3 destWorldPos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destWorldPos - transform.position;

        // µµÂø ¿©ºÎ
        float dist = moveDir.magnitude; // ¹æÇâ º¤ÅÍ Å©±â
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = destWorldPos;
            _isMoving = false;
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _isMoving = true;
        }
    }

    void UpdateIsMoving()
    {
        if (_isMoving == false)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _cellPos += Vector3Int.up;
                    _isMoving = true;
                    break;

                case MoveDir.Down:
                    _cellPos += Vector3Int.down;
                    _isMoving = true;
                    break;

                case MoveDir.Left:
                    _cellPos += Vector3Int.left;
                    _isMoving = true;
                    break;

                case MoveDir.Right:
                    _cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
            }
        }
    }
}
