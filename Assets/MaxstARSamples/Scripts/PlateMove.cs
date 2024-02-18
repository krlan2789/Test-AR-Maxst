using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateMove : MonoBehaviour
{
    [SerializeField]
    private float offsetIncreaseRate = 1.0f; // �����ϰ��� �ϴ� ���� �ӵ�
    private Renderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
        void Update()
    {
        renderer.material.SetTextureOffset("_MainTex", new Vector2(renderer.material.mainTextureOffset.x - offsetIncreaseRate * Time.deltaTime, renderer.material.mainTextureOffset.y));
    }
}
