using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coSkill;
    Coroutine _coPatrol;
    Coroutine _coSearch;

    [SerializeField]
    Vector3Int _destination;

    [SerializeField]
    GameObject _target;

    [SerializeField]
    float _searchRange = 10.0f;

    [SerializeField]
    float _skillRange = 1.0f;

    [SerializeField]
    bool _rangedSkill = false;

    public override CreatureState State
    {
        get { return PositionInfo.State; }
        set
        {
            if (PositionInfo.State == value)
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

        _rangedSkill = (Random.Range(0, 2) == 0 ? true : false);

        if (_rangedSkill)
        {
            _skillRange = 10.0f;
        }
        else
        {
            _skillRange = 1.0f;
        }
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

            Vector3Int direction = destination - CellPosition;
            if (direction.magnitude <= _skillRange && (direction.x == 0 || direction.y == 0))
            {
                Dir = GetDirectionFromVector(direction);
                State = CreatureState.Skill;

                if (_rangedSkill)
                {
                    _coSkill = StartCoroutine("CoStartShootArrow");
                }
                else
                {
                    _coSkill = StartCoroutine("CoStartPunch");
                }
                
                return;
            }
        }

        // target이 움직일수도 있기 때문에,
        // Frame마다 연산한다.
        List<Vector3Int> path = Managers.Map.FindPath(CellPosition, destination, ignoreDestinationCollision: true);

        // 찾지 못했거나, 너무 먼 경우.
        if (path.Count < 2 || (_target != null && path.Count > 20))
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        // path[0] : 현재 위치.
        Vector3Int nextPosition = path[1];
        Vector3Int moveCellDirection = nextPosition - CellPosition;

        Dir = GetDirectionFromVector(moveCellDirection);

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

        Managers.Object.Remove(Id);
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

    IEnumerator CoStartPunch()
    {
        // 피격 판정.
        GameObject go = Managers.Object.FindCreature(base.GetFrontCellPosition());
        if (go != null)
        {
            CreatureController creature = go.GetComponent<CreatureController>();
            if (creature != null)
            {
                creature.OnDamaged();
            }
        }

        // 대기 시간.
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Moving;
        _coSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        GameObject arrow = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController aController = arrow.GetComponent<ArrowController>();
        aController.Dir = _lastDir;
        aController.CellPosition = CellPosition;

        // 대기 시간.
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Moving;
        _coSkill = null;
    }
}
