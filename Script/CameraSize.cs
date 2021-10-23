using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSize : MonoBehaviour
{
    public Camera mainCamera;
    public float baseWidth = 4f;
    public float baseHeight = 3f;

    void Awake()
    {
        // アスペクト比固定
        var scale = Mathf.Min(Screen.height / this.baseHeight, Screen.width / this.baseWidth);
        var width = (this.baseWidth * scale) / Screen.width;
        var height = (this.baseHeight * scale) / Screen.height;
        this.mainCamera.rect = new Rect((1.0f - width) * 0.5f, (1.0f - height) * 0.5f, width, height);
    }
    void Update()
    {
        var scale = Mathf.Min(Screen.height / this.baseHeight, Screen.width / this.baseWidth);
        var width = (this.baseWidth * scale) / Screen.width;
        var height = (this.baseHeight * scale) / Screen.height;
        this.mainCamera.rect = new Rect((1.0f - width) * 0.5f, (1.0f - height) * 0.5f, width, height);
    }
}
