using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserControl : MonoBehaviour
{
    [SerializeField]
    private int speed;
    private int maxHp = 100;
    [SerializeField]
    private int dropHp;
    [SerializeField]
    private int curHp;
    private Vector3 target, mousePos;
    [SerializeField]
    private Slider hpSlider;
    [SerializeField]
    private GameObject effectPrefab;

    private Transform startTransform;
    private void Start()
    {
        startTransform = this.transform;
        SetHP(maxHp);
        StartCoroutine(Drop());
        target = transform.position;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                SetTargetPos();
                var prefab = Instantiate(effectPrefab);
                prefab.transform.position = target;
            }
        }
        Move();
    }
    private IEnumerator Drop()
    {
        while (true)
        {
            SetHP(curHp - 1);
            yield return new WaitForSeconds(1f);
        }
    }
    private void Move()
    {
        hpSlider.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2f, 0));
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
    }
    public void SetTargetPos()
    {
        mousePos = Input.mousePosition;
        Vector3 pos = Camera.main.ScreenToWorldPoint(mousePos);
        target = new Vector3(pos.x, pos.y, 0);

        string moveData = "#Move#" + target.x + ',' + target.y;
        GameManager.Instance.SendCommand(moveData);
    }

    private void SetHP(int hp)
    {
        curHp = hp;
        hpSlider.value = curHp;
    }

    public void DropHP(int drop)
    {
        SetHP(curHp - drop);
    }

    public void Revive()
    {
        SetHP(maxHp);
    }

    public int GetHP()
    {
        return curHp;
    }
}
