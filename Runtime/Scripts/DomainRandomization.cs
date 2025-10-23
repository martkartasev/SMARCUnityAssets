using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DomainRandomization : MonoBehaviour
{
    [Header("Sun Light Settings")]
    [Tooltip("The sun light source in the scene")]
    public GameObject TheSun;
    Light sunLight;
    Transform sunTF;
    [Tooltip("Degrees")]
    public float SunRotationHorizontalBase = 0f;
    public float SunRotationHorizontalRange = 30f; // rotation.y
    public float SunRotationVerticalBase = 45f;
    public float SunRotationVerticalRange = 10f; // rotation.x
    [Tooltip("Kelvin")]
    public float SunEmissionTemperatureBase = 10000f; // Kelvin
    public float SunEmissionTemperatureRange = 2000f; // Kelvin
    [Tooltip("Thousands of Lux")]
    public float SunIntensityThousandsBase = 40f; // thousand Lux
    public float SunIntensityThousandsRange = 10f; // thousand Lux

    [Header("Sky and Fog Settings")]
    public GameObject SkyAndFogSettings;
    Volume skyAndFogVolume;
    Fog fog;
    PhysicallyBasedSky sky;
    [Range(0f, 1f)]
    public float AerosolDensityBase = 0f;
    public float AerosolDensityRange = 0.05f;
    public float FogDistanceBase = 1000f;
    public float FogDistanceRange = 20f;

    [Header("Water Surface Settings")]
    [Tooltip("The ocean or water surface in the scene, usually called Ocean")]
    public WaterSurface WaterSurface;
    public float DistantWindSpeedBase = 10f;
    public float DistantWindSpeedRange = 10f;
    public float LocalWindSpeedBase = 2f;
    public float LocalWindSpeedRange = 2f;
    public float CurrentOrientationBase = 0f;
    public float CurrentOrientationRange = 360f;
    public float CurrentSpeedBase = 1f;
    public float CurrentSpeedRange = 1f;

    [Header("Cameras")]
    public Transform CamTF;
    [Tooltip("If set, cameras will look at this target after moving")]
    public Transform LookAtTarget;
    [Tooltip("Randomize a relative offset to look at around the target")]
    public float LookAtTargetOffsetHorizontalRange = 3f;
    [Tooltip("Randomize a relative offset to look at around the target")]
    public float LookAtTargetOffsetVerticalRange = 1f;
    [Tooltip("Range in meters for randomizing camera positions around their original position")]
    public float CameraHorizontalPositionRange = 0.5f;
    public float CameraVerticalPositionRange = 0.5f;
    [Tooltip("Range in degrees for randomizing camera rotations")]
    public float CameraRotationRange = 10f;






    void Start()
    {
        if (TheSun != null)
        {
            sunLight = TheSun.GetComponent<Light>();
            sunTF = TheSun.transform;
        }
        if(SkyAndFogSettings != null)
        {
            skyAndFogVolume = SkyAndFogSettings.GetComponent<Volume>();
            skyAndFogVolume.profile.TryGet<Fog>(out fog);
            skyAndFogVolume.profile.TryGet<PhysicallyBasedSky>(out sky);
        }
    }

    public void RandomizeCameras()
    {
        if (CamTF == null) return;
        
        Vector3 positionOffset = Vector3.zero;
        // Randomize position
        if(CameraHorizontalPositionRange > 0f)
        {
            Vector3 randomHorizontalOffset = new(
                Random.Range(-CameraHorizontalPositionRange, CameraHorizontalPositionRange),
                0f,
                Random.Range(-CameraHorizontalPositionRange, CameraHorizontalPositionRange)
            );
            positionOffset += randomHorizontalOffset;
        }

        // Randomize vertical position
        if (CameraVerticalPositionRange > 0f)
        {
            Vector3 randomVerticalOffset = new(
                0f,
                Random.Range(-CameraVerticalPositionRange, CameraVerticalPositionRange),
                0f
            );
            positionOffset += randomVerticalOffset;
        }

        CamTF.localPosition = positionOffset;

        // Randomize rotation
        if (CameraRotationRange > 0f)
        {
            Vector3 randomRotation = new(
                Random.Range(-CameraRotationRange, CameraRotationRange),
                Random.Range(-CameraRotationRange, CameraRotationRange),
                Random.Range(-CameraRotationRange, CameraRotationRange)
            );
            CamTF.localRotation = Quaternion.Euler(CamTF.localRotation.eulerAngles + randomRotation);
        }

        // Look at target if specified
        if (LookAtTarget != null)
        {
            Vector3 lookAt = LookAtTarget.position;
            // Apply random offset to look at target
            if (LookAtTargetOffsetHorizontalRange > 0f)
            {
                lookAt += new Vector3(
                    Random.Range(-LookAtTargetOffsetHorizontalRange, LookAtTargetOffsetHorizontalRange),
                    0f,
                    Random.Range(-LookAtTargetOffsetHorizontalRange, LookAtTargetOffsetHorizontalRange)
                );
            }
            if (LookAtTargetOffsetVerticalRange > 0f)   
            {
                lookAt += new Vector3(
                    0f,
                    Random.Range(-LookAtTargetOffsetVerticalRange, LookAtTargetOffsetVerticalRange),
                    0f
                );
            }
            CamTF.LookAt(lookAt);
        }
        
    }

    public void RandomizeSkyAndFog()
    {
        if (skyAndFogVolume == null)
        {
            Start();
        }

        if (fog != null && FogDistanceRange > 0f)
        {
            float randomFogOffset = Random.Range(-FogDistanceRange, FogDistanceRange);
            fog.meanFreePath.value = Mathf.Max(1f, FogDistanceBase + randomFogOffset);
        }

        if (sky != null && AerosolDensityRange > 0f)
        {
            float randomAerosolOffset = Random.Range(-AerosolDensityRange, AerosolDensityRange);
            sky.aerosolDensity.value = Mathf.Max(0f, AerosolDensityBase + randomAerosolOffset);
        }
    }

    public void RandomizeSun()
    {
        if (sunLight == null || sunTF == null)
        {
            Start();
        }

        // Randomize sun rotation
        if (SunRotationHorizontalRange > 0f || SunRotationVerticalRange > 0f)
        {
            float randomY = Random.Range(-SunRotationHorizontalRange, SunRotationHorizontalRange);
            float randomX = Random.Range(-SunRotationVerticalRange, SunRotationVerticalRange);
            float newX = SunRotationVerticalBase + randomX;
            newX = Mathf.Max(newX, 0f);
            float newY = SunRotationHorizontalBase + randomY;

            sunTF.rotation = Quaternion.Euler(newX, newY, 0f);
        }

        if (SunEmissionTemperatureRange > 0f)
        {
            // Randomize sun temperature
            float randomTemperatureOffset = Random.Range(-SunEmissionTemperatureRange, SunEmissionTemperatureRange);
            sunLight.colorTemperature = SunEmissionTemperatureBase + randomTemperatureOffset;
        }

        if (SunIntensityThousandsRange > 0f)
        {
            // Randomize sun intensity
            float randomIntensityOffset = Random.Range(-SunIntensityThousandsRange * 1000f, SunIntensityThousandsRange * 1000f);
            sunLight.intensity = SunIntensityThousandsBase * 1000f + randomIntensityOffset;
        }

    }

    public void RandomizeWaterSurface()
    {
        if (WaterSurface == null) return;

        if (DistantWindSpeedRange > 0f)
        {
            float randomDistantWindSpeedOffset = Random.Range(-DistantWindSpeedRange, DistantWindSpeedRange);
            WaterSurface.largeWindSpeed = Mathf.Max(0f, DistantWindSpeedBase + randomDistantWindSpeedOffset);
        }

        if (LocalWindSpeedRange > 0f)
        {
            float randomLocalWindSpeedOffset = Random.Range(-LocalWindSpeedRange, LocalWindSpeedRange);
            WaterSurface.ripplesWindSpeed = Mathf.Max(0f, LocalWindSpeedBase + randomLocalWindSpeedOffset);
        }

        if (CurrentOrientationRange > 0f)
        {
            float randomCurrentOrientationOffset = Random.Range(-CurrentOrientationRange, CurrentOrientationRange);
            WaterSurface.largeOrientationValue = (CurrentOrientationBase + randomCurrentOrientationOffset) % 360f;
        }

        if (CurrentSpeedRange > 0f)
        {
            float randomCurrentSpeedOffset = Random.Range(-CurrentSpeedRange, CurrentSpeedRange);
            WaterSurface.largeCurrentSpeedValue = Mathf.Max(0f, CurrentSpeedBase + randomCurrentSpeedOffset);
        }
    }
    
    public void RandomizeAll()
    {
        RandomizeSun();
        RandomizeSkyAndFog();
        RandomizeWaterSurface();
        RandomizeCameras();
    }

    void OnDrawGizmos()
    {
        if (CamTF != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(
                CameraHorizontalPositionRange * 2f,
                CameraVerticalPositionRange * 2f,
                CameraHorizontalPositionRange * 2f
            ));
        }

        if (LookAtTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(LookAtTarget.position, new Vector3(
                LookAtTargetOffsetHorizontalRange * 2f,
                LookAtTargetOffsetVerticalRange * 2f,
                LookAtTargetOffsetHorizontalRange * 2f
            ));
        }


    }

}