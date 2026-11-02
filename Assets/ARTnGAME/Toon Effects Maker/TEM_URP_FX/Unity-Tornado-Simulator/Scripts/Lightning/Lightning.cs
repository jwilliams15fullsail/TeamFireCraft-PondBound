using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
namespace Artngame.TEM
{
    //Cool lightning from http://drilian.com/2009/02/25/lightning-bolts/
    public class Lightning : MonoBehaviour
    {
        //public GameObject smallLightningObj;

        public GameObject lineRenderer;

        //Store all coordinates in one segment in this list
        List<Segment> lightningSegments = new List<Segment>();
        //A list with all linerenderers so we can delete them when we create a new lightning
        List<GameObject> lineRenderers = new List<GameObject>();

        //Lightning parameters
        Vector3 startPos;
        Vector3 endPos;
        //How detailed should this lightning be
        public int generations = 4;
        //The maximum amount to offset a lightning vertex
        public float maximumOffset = 60f;
        //Whats the probability that a lightningsegment will branch off
        public float splitProbability = 0.8f;
        //Width
        public float width = 0.8f;
        public float smallWidth = 0.3f;

        public float maxLightingHeight = 100;
        public float maxLightingLength = 100;
        void Start()
        {
            startPos = transform.position + new Vector3(0, Random.Range(0, maxLightingHeight), 0);
            endPos = startPos - transform.up * (230f + Random.Range(0, maxLightingLength));

            GenerateLightning(startPos, endPos, generations, maximumOffset, splitProbability);

            Destroy(gameObject, 0.5f);
        }



        void Update()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if ((Mouse.current != null && Mouse.current.leftButton.isPressed)  //isPressed //wasPressedThisFrame
                 
            )
            {
#else
            if (Input.GetMouseButton(0))
            {
#endif
                //Destroy the old line renderers - use pooling in the future
                for (int i = 0; i < lineRenderers.Count; i++)
                {
                    Destroy(lineRenderers[i]);
                }


                GenerateLightning(startPos, endPos, generations, maximumOffset, splitProbability);
            }
        }



        void GenerateLightning(Vector3 startPos, Vector3 endPos, int generations, float offsetAmount, float splitProb)
        {
            //Init
            lightningSegments.Clear();

            List<Segment> tempSegments = new List<Segment>();

            //Add the start and end pos
            lightningSegments.Add(new Segment(startPos, endPos, false));

            //Direction of the camera so the lightning is always fasing the camera
            Vector3 camDir = (Camera.main.transform.position - ((startPos + endPos) / 2f));

            //Generate the lightning
            for (int i = 0; i < generations; i++)
            {
                //For each segment that was in segmentList when this generation started
                for (int j = 0; j < lightningSegments.Count; j++)
                {
                    Vector3 startPoint = lightningSegments[j].start;
                    Vector3 endPoint = lightningSegments[j].end;

                    //Find the center of the 2 vectors
                    Vector3 midPoint = (startPoint + endPoint) / 2f;

                    //Find the perpendicular vector to the center and move it by random amount
                    //The direction of this segment
                    Vector3 segmentDir = (midPoint - startPoint);
                    //A vector perpendicular is
                    Vector3 perpendicular = Vector3.Cross(segmentDir, camDir).normalized;

                    //Move the midpoint to this position
                    midPoint += perpendicular * Random.Range(-offsetAmount, offsetAmount);

                    //Create two new segments
                    tempSegments.Add(new Segment(startPoint, midPoint, lightningSegments[j].isBranch));
                    tempSegments.Add(new Segment(midPoint, endPoint, lightningSegments[j].isBranch));

                    //Should this section split?
                    if (Random.Range(0f, 1f) < splitProb)
                    {
                        //Rotate the new section
                        float angle = Random.Range(-30f, 30f);

                        if (angle < 0f)
                        {
                            angle -= 20f;
                        }
                        else
                        {
                            angle += 20f;
                        }

                        Vector3 newEndPoint = RotatePointAroundPivot(endPoint, midPoint, new Vector3(angle, 0f, 0f));

                        //Also make it a little shorter
                        Vector3 dir = (newEndPoint - midPoint).normalized;

                        newEndPoint = midPoint + dir * (newEndPoint - midPoint).magnitude * 0.6f;

                        tempSegments.Add(new Segment(midPoint, newEndPoint, true));
                    }
                }

                //Change offset amount
                offsetAmount /= 2f;

                //Add temp to main
                lightningSegments.Clear();

                for (int j = 0; j < tempSegments.Count; j++)
                {
                    lightningSegments.Add(tempSegments[j]);
                }

                tempSegments.Clear();
            }


            //Create the lightning with a line renderer
            AddToLineRenderer();
        }



        Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }



        void AddToLineRenderer()
        {
            lineRenderers.Clear();

            for (int i = 0; i < lightningSegments.Count; i++)
            {
                //Instantiate a new line renderer
                GameObject newLine = Instantiate(lineRenderer);

                //Parent it
                newLine.transform.parent = transform;

                //Add it to the list with all line renderers
                lineRenderers.Add(newLine);

                //Get the line renderer
                LineRenderer thisLine = newLine.GetComponent<LineRenderer>();

                //Set the coordinates
                thisLine.SetPosition(0, lightningSegments[i].start);
                thisLine.SetPosition(1, lightningSegments[i].end);

                //Change scale if it's a branch
                if (lightningSegments[i].isBranch)
                {
                    //thisLine.SetWidth(smallWidth, smallWidth);
                    thisLine.startWidth = smallWidth;
                    thisLine.endWidth = smallWidth;
                }
                else
                {
                    //thisLine.SetWidth(width, width);
                    thisLine.startWidth = width;
                    thisLine.endWidth = width;
                }
            }
        }
    }




    public struct Segment
    {
        public Vector3 start;
        public Vector3 end;
        //Also keep track if this segment is part of a branch to the main lightning so it can be smaller
        public bool isBranch;

        public Segment(Vector3 start, Vector3 end, bool isBranch)
        {
            this.start = start;
            this.end = end;
            this.isBranch = isBranch;
        }
    }
}