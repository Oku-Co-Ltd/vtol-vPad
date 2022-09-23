/* Written by okureya // Omnith LLC */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Harmony;
using UnityEngine.UI;
using vPad.Components;
using vPad.Util;
using Object = UnityEngine.Object;

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
            if (!__instance.isLocal)
            {
                ModDebug.Log($"Skipping non-local actor '{__instance.actor.actorName}'");
                return;
            }

            var vehicleName = VTScenario.current.vehicle.vehicleName;
            // check to see if we support this vehicle
            if (!vPadOku.Settings.Enabled.dictionary.ContainsKey(vehicleName))
            {
                // we'll go ahead and add it to be true by default
                ModDebug.Log($"New aircraft '{vehicleName}', adding to settings");
                vPadOku.Settings.Enabled.dictionary.Add(vehicleName, true);
                void Action(bool val)
                {
                    vPadOku.Settings.Enabled.dictionary[vehicleName] = val;
                    vPadOku.Instance.HaveSettingsChanged.Value = true;
                }
                // add delegate to the actions dict
                vPadOku.Settings.EnableActions.Add(vehicleName, Action);
                // trigger settings change -> should write to disk
                vPadOku.Instance.HaveSettingsChanged.Value = true;
            }
            // if we do, check to see if we have it enabled
            if (!vPadOku.Settings.Enabled.dictionary[vehicleName])
            {
                ModDebug.Log($"'{vehicleName}' vPad disabled by user, breaking");
                return;
            }

            ModDebug.Log("Instantiating vPad onto player vehicle...");

            // let's hook up the vPad right here to the vehicle
            var t = Traverse.Create(__instance);
            var vehicleRb = t.Field("vehicleRb").GetValue<Rigidbody>();
            var vehicle = vehicleRb.gameObject;
            var vPadGo = Object.Instantiate(vPadOku.Instance.vPadPrefab, vehicle.transform);
            var setScale = vPadOku.Settings.vPadScale;
            vPadGo.transform.localScale = new Vector3(setScale,setScale,setScale);
            
            // use vehicle's properties for MFD
            var vMfd = vPadGo.GetComponentInChildren<MFD>();
            vMfd.battery = vehicle.GetComponentInChildren<Battery>();

            // tell the vPad to initialize using vehicle's MFD manager and homepage
            MFDManager mfdManager;
            try
            {
                // test MFDManager first
                ModDebug.Log("Trying existing MFDManager first");
                mfdManager = vehicle.GetComponentsInChildren<MFDManager>(true)
                    .First(elem => elem.name == "MFDManager");

                //var mfdManPrefab = vehicle.GetComponentsInChildren<MFDManager>(true).First(elem => elem.name == "MFDManager");
                // create our own MFD manager from the existing one
                //var mfdManager = Object.Instantiate(mfdManPrefab, vPadGo.transform);
            }
            catch (Exception)
            {
                // else, we need to add our own MFDManager
                ModDebug.Log("Can't use existing MFDManager, creating a new one");
                var mfdManPrefab = new GameObject();
                var mfdManGo = Object.Instantiate(mfdManPrefab, vPadGo.transform);
                mfdManager = mfdManGo.GetComponent<MFDManager>();
                // TODO: rip the F/A-26's mfd manager config with its pages
            }
            // add MFD component to manager and active-cycle it so it initializes everything
            mfdManager.gameObject.SetActive(false);
            mfdManager.mfds = new List<MFD>(mfdManager.mfds.Concat(new List<MFD> { vMfd }));

            // add brightness adjuster to the aircraft's MFDBrightnessAdjuster
            var mfdBrightAdjust = vehicle.GetComponentInChildren<MFDBrightnessAdjuster>(true);
            var brightImg = vPadGo.GetComponentsInChildren<Image>().First(elem => elem.name.Contains("brightness"));
            mfdBrightAdjust.images = mfdBrightAdjust.images.AddToArray(brightImg);

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
