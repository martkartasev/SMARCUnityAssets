using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils = DefaultNamespace.Utils;

using VehicleComponents.Sensors;

namespace GameUI
{
    public class CameraManager : MonoBehaviour
    {
        TMP_Dropdown dropdown;
        public Camera currentCam;

        Dictionary<string, string> ddTextToObjectPath;

        void Start()
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
            ddTextToObjectPath = new Dictionary<string, string>();
            // disable all cams except the "main cam" at the start
            Camera[] cams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach(Camera c in cams)
            {
                // dont mess with sensor cameras
                if(c.gameObject.TryGetComponent<Sensor>(out Sensor s)) continue;
                // disable all cams by default. we will enable one later.
                c.enabled = false;
                // disable all audiolisteners. we got no audio. we wont enable these.
                if(c.gameObject.TryGetComponent<AudioListener>(out AudioListener al)) al.enabled=false;
                
                string objectPath = Utils.GetGameObjectPath(c.gameObject);
                string ddText = $"{c.transform.root.name}/{c.name}";
                ddTextToObjectPath.Add(ddText, objectPath);
                dropdown.options.Add(new TMP_Dropdown.OptionData(){text=ddText});
            }
            currentCam = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();
            currentCam.enabled = true;
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            OnValueChanged(0);
        }

        public void OnValueChanged(int ddIndex)
        {
            var selection = dropdown.options[ddIndex];
            string objectPath = ddTextToObjectPath[selection.text];
            GameObject selectedGO = GameObject.Find(objectPath);
            if(selectedGO == null) return;

            currentCam.enabled = false;
            currentCam = selectedGO.GetComponent<Camera>();
            currentCam.enabled = true;
        }

    }

}
