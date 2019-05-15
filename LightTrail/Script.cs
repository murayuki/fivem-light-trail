﻿using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace LightTrail
{
    public class Script : BaseScript
    {
        public const string dict = "core";
        public const string particleName = "veh_light_red_trail";
        public const string evolutionPropertyName = "speed";
        //public const string particleName = "veh_slipstream";
        //public const string evolutionPropertyName = "slipstream";

        public const string brakelight_l = "brakelight_l";
        public const string brakelight_r = "brakelight_r";
        public const string taillight_r = "taillight_r";
        public const string taillight_l = "taillight_l";

        public string boneName1;
        public string boneName2;

        public int ptfxHandle1;
        public int ptfxHandle2;

        public int PlayerPed = -1;
        public int CurrentVehicle = -1;
        public Vector3 offset = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;
        public Vector3 color = new Vector3(1.0f, 0.0f, 0.0f);
        public float scale = 1.0f;
        public float alpha = 1.0f;

        public bool EnablePTFX => CurrentVehicle != -1;
            //&& GetEntitySpeed(CurrentVehicle) != 0.0f
            //&& (IsControlJustPressed(1, (int)Control.VehicleBrake) || IsDisabledControlJustPressed(1, (int)Control.VehicleBrake));

        public Script()
        {
            Tick += Initialize;
        }

        public async Task Initialize()
        {
            Tick -= Initialize;

            RequestNamedPtfxAsset(dict);
            while (!HasNamedPtfxAssetLoaded(dict)) await Delay(0);

            Tick += GetCurrentVehicle;
            Tick += Loop;
        }

        public async Task Loop()
        {
            if (EnablePTFX)
            {
                UseParticleFxAssetNextCall(dict);

                if (!DoesParticleFxLoopedExist(ptfxHandle1))
                {
                    StartParticleFx(ref ptfxHandle1, particleName, CurrentVehicle, boneName1, offset, rotation, scale, color);
                }

                if (!DoesParticleFxLoopedExist(ptfxHandle2))
                {
                    StartParticleFx(ref ptfxHandle2, particleName, CurrentVehicle, boneName2, offset, rotation, scale, color);
                }
            }

            await Task.FromResult(0);
        }

        public void StartParticleFx(ref int handle, string ptfxName, int entity, string boneName, Vector3 offset, Vector3 rotation, float scale, Vector3 color)
        {
            int boneIndex = GetEntityBoneIndexByName(entity, boneName);

            // NON LOOPED ONE
            //Vector3 bonePosition = GetWorldPositionOfEntityBone(entity, boneIndex);

            // LOOPED ONE
            handle = StartParticleFxLoopedOnEntityBone(ptfxName, entity, offset.X, offset.Y, offset.Z, rotation.X, rotation.Y, rotation.Z, boneIndex, scale, false, false, false);
            SetParticleFxLoopedEvolution(handle, evolutionPropertyName, 1.0f, false);
            SetParticleFxLoopedColour(handle, color.X, color.Y, color.Z, false);
            SetParticleFxLoopedAlpha(handle, alpha);
        }

        public void Reset()
        {
            if (DoesParticleFxLoopedExist(ptfxHandle1)) RemoveParticleFx(ptfxHandle1, false);
            if (DoesParticleFxLoopedExist(ptfxHandle2)) RemoveParticleFx(ptfxHandle2, false);
        }

        /// <summary>
        /// Updates the <see cref="CurrentVehicle"/>
        /// </summary>
        /// <returns></returns>
        private async Task GetCurrentVehicle()
        {
            PlayerPed = PlayerPedId();

            if (IsPedInAnyVehicle(PlayerPed, false))
            {
                int vehicle = GetVehiclePedIsIn(PlayerPed, false);

                if (GetPedInVehicleSeat(vehicle, -1) == PlayerPed && !IsEntityDead(vehicle))
                {
                    // Update current vehicle and get its preset
                    if (vehicle != CurrentVehicle)
                    {
                        CurrentVehicle = vehicle;
                        EnsureBones(CurrentVehicle);
                    }
                }
                else
                {
                    // If current vehicle isn't a car or player isn't driving current vehicle or vehicle is dead
                    CurrentVehicle = -1;
                    Reset();
                }
            }
            else
            {
                // If player isn't in any vehicle
                CurrentVehicle = -1;
                Reset();
            }

            await Task.FromResult(0);
        }

        /// <summary>
        /// Some vehicles have no brakelights and use taillights as brakelights
        /// </summary>
        /// <param name="entity"></param>
        private void EnsureBones(int entity)
        {
            boneName1 = GetEntityBoneIndexByName(CurrentVehicle, brakelight_l) != -1 ? brakelight_l : taillight_l;
            boneName2 = GetEntityBoneIndexByName(CurrentVehicle, brakelight_r) != -1 ? brakelight_r : taillight_r;
        }
    }

}
