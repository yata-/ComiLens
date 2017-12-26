using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : MonoBehaviour {

    private Matrix4x4 _projectionMatrix;
    private const float OverlayDistance = 1;
    // Use this for initialization
    void Start () {
        _projectionMatrix = ConstValue.GetProjectMatrix();
    }
	
	// Update is called once per frame
	void Update () {

	    Matrix4x4 cameraToWorldMatrix = Camera.main.cameraToWorldMatrix; ;

	    Vector3 ccCameraSpacePos = UnProjectVector(_projectionMatrix, new Vector3(0.0f, 0.0f, OverlayDistance));
	    Vector3 tlCameraSpacePos = UnProjectVector(_projectionMatrix, new Vector3(-OverlayDistance, OverlayDistance, OverlayDistance));

	    //position
	    Vector3 position = cameraToWorldMatrix.MultiplyPoint3x4(ccCameraSpacePos);
	    gameObject.transform.position = position;

	    //scale
	    Vector3 scale = new Vector3(Mathf.Abs(tlCameraSpacePos.x - ccCameraSpacePos.x) * 2, Mathf.Abs(tlCameraSpacePos.y - ccCameraSpacePos.y) * 2, 1);
	    gameObject.transform.localScale = scale;

	    // Rotate the canvas object so that it faces the user.
	    Quaternion rotation = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));
	    gameObject.transform.rotation = rotation;
    }
    private Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector3 from = new Vector3(0, 0, 0);
        var axsX = proj.GetRow(0);
        var axsY = proj.GetRow(1);
        var axsZ = proj.GetRow(2);
        from.z = to.z / axsZ.z;
        from.y = (to.y - (from.z * axsY.z)) / axsY.y;
        from.x = (to.x - (from.z * axsX.z)) / axsX.x;
        return from;
    }
}
