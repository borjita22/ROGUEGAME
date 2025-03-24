using UnityEngine;

//Este componente se va añadir a todos aquellos objetos que siempre deban estar mirando hacia la camara
public class BillboardElement : MonoBehaviour
{
    private Camera mainCamera;

	private void Awake()
	{
        mainCamera = Camera.main;
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void LateUpdate()
	{
        transform.rotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
	}
}
