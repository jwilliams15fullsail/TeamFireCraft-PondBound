using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
namespace Artngame.TEM
{
    public class GroundBurnProjectile : MonoBehaviour
    {
        public Camera cameraA;
        public GameObject groundBurner;
        public Transform burnLocation;
        public float rateOfFire = 4;

        Vector3 dest;
        float timeFire;
        GroundBurnFollowTerrain groundBurnScript;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if ((Mouse.current != null && Mouse.current.leftButton.isPressed)  //isPressed //wasPressedThisFrame
                 && Time.time >= timeFire
            )
            {
#else
            if(Input.GetButton("Fire1") && Time.time >= timeFire)
            {
#endif
                timeFire = Time.time + 1 / rateOfFire;
                fireGroundBurner();
            }
        }

        public void fireGroundBurner()
        {
            Ray ray = cameraA.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
            dest = ray.GetPoint(1000);
            InstantiateFireBurner();
        }

        void InstantiateFireBurner()
        {
            GameObject projectile = Instantiate(groundBurner, burnLocation.position,Quaternion.identity);
            groundBurnScript = groundBurner.GetComponent<GroundBurnFollowTerrain>();
            rotatetoTarget(projectile, dest,true);
            projectile.GetComponent<Rigidbody>().linearVelocity = transform.forward * groundBurnScript.velocity;
        }
        void rotatetoTarget(GameObject projectile, Vector3 dest, bool onY) {
            Vector3 dir = dest - projectile.transform.position;
            Quaternion rot = Quaternion.LookRotation(dir);

            if (onY)
            {
                rot.x = rot.z = 0;
            }

            projectile.transform.localRotation = Quaternion.Lerp(projectile.transform.rotation, rot,1);
        }

    }
}