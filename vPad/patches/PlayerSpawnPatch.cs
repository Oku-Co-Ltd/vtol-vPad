/* Copyright 2022 okureya, Omnith LLC. Attributes CC-BY 3.0 */
using System.Linq;
using UnityEngine;
using Harmony;
using vPad.Components;
using vPad.Util;

namespace vPad.patches
{
    [HarmonyPatch(typeof(PlayerSpawn), "OnPreSpawnUnit")]
    class PlayerSpawnPatch
    {
        static void Postfix(PlayerSpawn __instance)
        {
            if (!VTScenario.current.vehicle.vehiclePrefab)
            {
                ModDebug.LogWarning("No selected vehicle present! Not loading vPad");
                return;
            }

            // check to see if we support this vehicle
            if (!vPadOku.Settings.Enabled.dictionary.ContainsKey(VTScenario.current.vehicle.vehicleName))
            {
                ModDebug.Log($"The aircraft '{VTScenario.current.vehicle.vehicleName}' is not supported, breaking");
                return;
            }
            // if we do, check to see if we have it enabled
            if (!vPadOku.Settings.Enabled.dictionary[VTScenario.current.vehicle.vehicleName])
            {
                ModDebug.Log($"'{VTScenario.current.vehicle.vehicleName}' vPad disabled by user, breaking");
                return;
            }

            ModDebug.Log("Instantiating vPad onto player vehicle...");

            // let's hook up the vPad right here to the vehicle
            Traverse t = Traverse.Create(__instance);
            var vehicleRb = t.Field("vehicleRb").GetValue<Rigidbody>();
            GameObject vehicle = vehicleRb.gameObject;
            GameObject vPadGo = Object.Instantiate(vPadOku.Instance.vPadPrefab, vehicle.transform);
            
            // use vehicle's properties for MFD
            var vMfd = vPadGo.GetComponentInChildren<MFD>();
            vMfd.battery = vehicle.GetComponentInChildren<Battery>();
            // tell the vPad to initialize using vehicle's MFD manager and homepage
            var mfdManager = vehicle.GetComponentInChildren<MFDManager>(true);
            // add MFD component to manager and active-cycle it so it initializes everything
            mfdManager.gameObject.SetActive(false);
            mfdManager.mfds.Add(vMfd);
            mfdManager.gameObject.SetActive(true);

            // add functionality to VR Interactable for vPad grip (redundant if the interactable is on the root object)
            var objInt = vPadGo.GetComponentsInChildren<VRInteractable>()
                .FirstOrDefault(elem => elem.interactableName == "vPad");
            if (objInt == default(VRInteractable) || !objInt)
            {
                ModDebug.LogWarning("couldn't find vPad grip interactable!");
                return;
            }

            // add our draggable object component to this ... thanks C-137 for the idea!
            var objDrag = objInt.gameObject.AddComponent<vDraggableObject>();
            objDrag.TargetGripGo = vPadGo;
            // set its initial position in front of the player
            objDrag.SetupTargetPosition();
            // next, set up listeners to the VRInteractable object
                // called when grip is first grabbed (no local var referencing!)
            objInt.OnInteract.AddListener(() => objInt.GetComponent<vDraggableObject>().StartDraggingTarget(objInt));
                // called when grip is released
            //objInt.OnStopInteract.AddListener( objDragComponent.StopDraggingTarget );
                // called every frame while grip is held
            //objInt.OnInteracting.AddListener(() => {});

            ModDebug.Log("...instantiation complete!");
        }
    }
}
