
using RoR2;
using UnityEngine;

    internal enum SettCameraParams
{
    DEFAULT,
    EMOTE,
}

namespace SettMod.Modules
{

    internal static class CameraParams
    {
        internal static CharacterCameraParamsData defaultCameraParamsSett;
        internal static CharacterCameraParamsData emoteCameraParamsSett;

        internal static float defaultVerticalOffset = 1.53f;

        internal static void InitializeParams()
        {
            emoteCameraParamsSett = NewCameraParams("ccpSettEmote", 70f, defaultVerticalOffset, new Vector3(0f, -0.6f, -8.5f));
        }

        private static CharacterCameraParamsData NewCameraParams(string name, float pitch, float pivotVerticalOffset, Vector3 idealPosition)
        {
            return NewCameraParams(name, pitch, pivotVerticalOffset, idealPosition, 0.1f);
        }

        private static CharacterCameraParamsData NewCameraParams(string name, float pitch, float pivotVerticalOffset, Vector3 idealPosition, float wallCushion)
        {
            CharacterCameraParamsData newParams = new CharacterCameraParamsData();

            newParams.maxPitch = pitch;
            newParams.minPitch = -pitch;
            newParams.pivotVerticalOffset = pivotVerticalOffset;
            newParams.idealLocalCameraPos = idealPosition;
            newParams.wallCushion = wallCushion;

            return newParams;
        }


        internal static CameraTargetParams.CameraParamsOverrideHandle OverrideSettCameraParams(CameraTargetParams camParams, SettCameraParams camera, float transitionDuration = 0.5f)
        {

            CharacterCameraParamsData paramsData = GetNewSettParams(camera);

            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = paramsData,
                priority = 0,
            };

            return camParams.AddParamsOverride(request, transitionDuration);
        }

        internal static CharacterCameraParams CreateCameraParamsWithData(SettCameraParams camera)
        {

            CharacterCameraParams newSettCameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();

            newSettCameraParams.name = camera.ToString().ToLower() + "Params";

            newSettCameraParams.data = GetNewSettParams(camera);

            return newSettCameraParams;
        }

        internal static CharacterCameraParamsData GetNewSettParams(SettCameraParams camera)
        {
            CharacterCameraParamsData paramsData = defaultCameraParamsSett;

            switch (camera)
            {

                default:
                case SettCameraParams.DEFAULT:
                    paramsData = defaultCameraParamsSett;
                    break;
                case SettCameraParams.EMOTE:
                    paramsData = emoteCameraParamsSett;
                    break;
            }

            return paramsData;
        }
    }
}