using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenCapture : MonoBehaviour
{

    [SerializeField] int counter = 0;
    [SerializeField, Range(0, 4)] int superSize = 0;

    protected void Start()
    {
        counter = 0;
    }

    protected void Update()
    {
        if (Input.GetKeyUp("s"))
        {
            ScreenShot();
        }
    }

    void ScreenShot()
    {
        var name = "take_" + counter.ToString("0000") + ".png";
        UnityEngine.ScreenCapture.CaptureScreenshot(name, superSize);
        counter++;
    }

}

