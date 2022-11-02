using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMoving : MonoBehaviour
{
    [SerializeField]
    private float speed = 0.1f;

    private MeshRenderer meshRenderer = null;
    private Vector2 offset = Vector2.zero;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        StartCoroutine(AddSpeed());
    }

    void Update()
    {
        offset.x += speed * Time.deltaTime;
        meshRenderer.material.SetTextureOffset("_MainTex", offset);
    }
    private IEnumerator AddSpeed()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            speed += 0.01f;
        }
    }
}
