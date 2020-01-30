using RAGE;
using RAGETimer.Shared;
using System;

namespace ClientCameraEffects
{
    public class ClientCameraEffects : Events.Script
    {

        public ClientCameraEffects()
        {
            Events.Add("StartPlayerSwitch", ShitCameraToNewLocation);
        }

        private void ShitCameraToNewLocation(object[] args)
        {
            var targetPos = (Vector3)args[0];
            var player = RAGE.Elements.Player.LocalPlayer;

            float posX = player.Position.X;
            float posY = player.Position.Y;
            float posZ = player.Position.Z + 50f;

            float rotX = 0;
            float rotY = 0;
            float rotZ = 0;

            //rotX = player.GetRotation(0).X;
            //rotY = player.GetRotation(0).Y;
            //rotZ = player.GetRotation(0).Z;

            float targetX = targetPos.X;
            float targetY = targetPos.Y;
            float targetZ = targetPos.Z;


            int camera = RAGE.Game.Cam.CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", posX, posY, posZ, 0, 0, 0, 2, false, 0);
            RAGE.Game.Cam.SetCamActive(camera, true);
            RAGE.Game.Cam.SetCamFov(camera, 5.0f);
            RAGE.Game.Cam.PointCamAtEntity(camera, player.Handle, 0f, 0f, 35f, true);
            RAGE.Game.Cam.RenderScriptCams(true, false, 0, false, false, 0);


            int firstHeight = 300;
            int secondHeight = 600;
            int thirdHeight = 1400;

            SetCameraPosWithEffect(camera, posX, posY, posZ + firstHeight, 1000, () =>
            {
                SetCameraPosWithEffect(camera, posX, posY, posZ + secondHeight, 1000, () =>
                {
                    SetCameraPosWithEffect(camera, posX, posY, posZ + thirdHeight, 1000, () =>
                    {
                        var interpolateBaby = new Timer(() =>
                        {

                            var cameraRot = RAGE.Game.Cam.GetCamRot(camera, 2);
                            int cameraTo = RAGE.Game.Cam.CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", targetX, targetY, targetZ + thirdHeight, cameraRot.X, cameraRot.Y, cameraRot.Z, 5, false, 0);
                            RAGE.Game.Cam.SetCamActive(cameraTo, true);
                            //RAGE.Game.Cam.PointCamAtEntity(cameraTo, player.Handle, 0f, 0f, 0f, true);

                            RAGE.Game.Cam.SetCamActiveWithInterp(cameraTo, camera, 1500, 4, 1);
                            RAGE.Game.Cam.RenderScriptCams(true, true, 1, false, false, 0);

                            var finalDun = new Timer(() =>
                            {
                                SetCameraPosWithEffect(cameraTo, targetX, targetY, targetZ + secondHeight, 1000, () =>
                                {
                                    SetCameraPosWithEffect(cameraTo, targetX, targetY, targetZ + firstHeight, 1000, () =>
                                    {
                                        //SetCameraPosWithEffect(cameraTo, targetX, targetY, targetZ + firstHeight, 1000, () =>
                                        {
                                            RAGE.Game.Cam.RenderScriptCams(false, false, 0, false, false, 0);
                                            RAGE.Game.Graphics.StartScreenEffect("SwitchShortNeutralIn", 0, false);
                                        }

                                    });

                                });
                            }, 1500);

                        }, 100);

                    });

                });

            });
        }

        private void SetCameraPosWithEffect(int camera, float posX, float posY, float posZ, uint time = 1000, System.Action action = null, string effect = "SwitchShortNeutralIn")
        {
            var finalDun = new Timer(() =>
            {
                RAGE.Game.Cam.SetCamCoord(camera, posX, posY, posZ);
                RAGE.Game.Graphics.StartScreenEffect(effect, 0, false);
                RAGE.Game.Cam.RenderScriptCams(true, false, 0, false, false, 0);
                action();
            }, time, 1);
        }

    }
}
