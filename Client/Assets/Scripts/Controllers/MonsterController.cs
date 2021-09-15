using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coPatrol;
    Coroutine _coSearch;

    [SerializeField]
    Vector3Int _destination;

    [SerializeField]
    GameObject _target;

    [SerializeField]
    float _searchRange = 5.0f;

    public override CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
            {
                return;
            }

            base.State = value;
            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        // Animator, SpriteRenderer를
        // 준비해야 하므로, Init() 뒤에 수행.
        State = CreatureState.Idle;
        Dir = MoveDir.None;

        _speed = 3.0f;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }

        if (_coSearch == null)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    protected override void MoveToNextPosition()
    {
        Vector3Int destination = _destination;
        if (_target != null)
        {
            destination = _target.GetComponent<CreatureController>().CellPosition;
        }

        // target이 움직일수도 있기 때문에,
        // Frame마다 연산한다.
        List<Vector3Int> path = Managers.Map.FindPath(CellPosition, destination, ignoreDestinationCollision: true);

        // 찾지 못했거나, 너무 먼 경우.
        if (path.Count < 2 || (_target != null && path.Count > 10))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        // path[0] : 현재 위치.
        Vector3Int nextPosition = path[1];
        Vector3Int moveCellDirection = nextPosition - CellPosition;

        if (moveCellDirection.x > 0)
        {
            Dir = MoveDir.Right;
        }
        else if (moveCellDirection.x < 0)
        {
            Dir = MoveDir.Left;
        }
        else if (moveCellDirection.y > 0)
        {
            Dir = MoveDir.Up;
        }
        else if (moveCellDirection.y < 0)
        {
            Dir = MoveDir.Down;
        }
        else
        {
            Dir = MoveDir.None;
        }

        if (Managers.Map.CanMove(nextPosition) && Managers.Object.FindCreature(nextPosition) == null)
        {
            CellPosition = nextPosition;
        }
        else
        {
            State = CreatureState.Idle;
        }
    }

    public override void OnDamaged()
    {
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);

        Managers.Object.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for (int i = 0; i < 10; ++i)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);
            Vector3Int destination = CellPosition + new Vector3Int(xRange, yRange, 0);

            if (Managers.Map.CanMove(destination) && Managers.Object.FindCreature(destination) == null)
            {
                _destination = destination;
                State = CreatureState.Moving;
                yield break;
            }
        }

        State = CreatureState.Idle;
    }

    IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (_target != null)
            {
                continue;
            }

            _target = Managers.Object.FindCreature((go) =>
            {
                PlayerController pc = go.GetComponent<PlayerController>();
                if (pc == null)
                {
                    return false;
                }

                Vector3Int direction = (pc.CellPosition - CellPosition);
                if (direction.magnitude > _searchRange)
                {
                    return false;
                }

                return true;
            });
        }
    }
}
