using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

namespace Bukur
{
    [ExecuteInEditMode]
    public class Sheet : MonoBehaviour
    {

        public Transform TopHandle;
        public Transform BottomHandle;
        
        public Transform[] TopHandleTargets;
        public Transform[] BottomHandleTargets;

        private PageGenerator[] pages;

        public bool releasing;
        private Vector3 releaseTopHandlePos;
        private Vector3 releaseBottomHandlePos;
        
        void Start()
        {
            Init();
        }

        public void Init()
        {
            if(pages == null)
                pages = GetComponentsInChildren<PageGenerator>();

            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Init();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (TopHandle != null)
            {
                for (int i = 0; i < TopHandleTargets.Length; i++)
                {
                    if (TopHandleTargets[i] != null)
                    {
                        TopHandleTargets[i].position = TopHandle.position;
                    }
                }
            }

            if (BottomHandle != null)
            {
                for (int i = 0; i < BottomHandleTargets.Length; i++)
                {
                    if (BottomHandleTargets[i] != null)
                    {
                        BottomHandleTargets[i].position = BottomHandle.position;
                    }
                }
            }
            
            if (this.releasing)
            {
                TopHandle.transform.localPosition = Vector3.Lerp(TopHandle.transform.localPosition, releaseTopHandlePos , Time.deltaTime * 10);
                BottomHandle.transform.localPosition = Vector3.Lerp(BottomHandle.transform.localPosition, releaseBottomHandlePos, Time.deltaTime * 10);

                if (Mathf.Abs(Vector3.Distance(releaseTopHandlePos, TopHandle.transform.localPosition)) == 0 &&
                    Mathf.Abs(Vector3.Distance(releaseBottomHandlePos, BottomHandle.transform.localPosition)) == 0)
                {
                    this.releasing = false;
                    
                    SetIsReleasing(false);
                }
            }
        }
        
        public Vector2 GetSize()
        {
            return pages[0].GetSize();
        }

        public void ReleasePosition(Vector3 releaseTopHandlePos, Vector3 releaseBottomHandlePos, bool isImmediate = false)
        {
            this.releaseTopHandlePos = releaseTopHandlePos;
            this.releaseBottomHandlePos = releaseBottomHandlePos;

            if (isImmediate)
            {

                TopHandle.transform.localPosition = releaseTopHandlePos;
                BottomHandle.transform.localPosition = releaseBottomHandlePos;
                
                this.releasing = false;
                SetIsReleasing(false);
            }
            else
            {
                this.releasing = true;
                SetIsReleasing(true);
            }
            

        }

        public void FlipRight(bool isImmediate)
        {
            var size = GetSize();
            ReleasePosition(new Vector3(size.x, size.y), new Vector3(size.x, 0), isImmediate);
        }
        
        public void FlipLeft(bool isImmediate)
        {
            var size = GetSize();
            ReleasePosition(new Vector3(-size.x, size.y), new Vector3(-size.x, 0), isImmediate);
        }

        public void SetFlipping(bool isFlipping)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetIsFlipping(isFlipping);
            }
        }

        public void SetIsReleasing(bool isReleasing)
        {
            this.releasing = isReleasing;
            for (int i =0; i < pages.Length; i++)
            {
                pages[i].IsReleasing = isReleasing;
            }
        }

        public void SetTopPageTexture(Texture2D texture)
        {
            if(pages == null)
                pages = GetComponentsInChildren<PageGenerator>();
            
            if(pages.Length > 0)
                pages[0].SetTexture(texture);
        }
        
        public void SetBackPageTexture(Texture2D texture)
        {
            if(pages == null)
                pages = GetComponentsInChildren<PageGenerator>();
            
            if(pages.Length > 1)
                pages[1].SetTexture(texture);
        }
        
        
        public void SetTopPageCurveProperties(AnimationCurve idleCurve, float idleFactor, AnimationCurve flippingCurve, float flipFactor)
        {
            if(pages == null)
                pages = GetComponentsInChildren<PageGenerator>();

            if (pages.Length > 0)
            {
                pages[0].IdleCurve = idleCurve;
                pages[0].IdleCurveFactor = idleFactor;
                pages[0].FlippingCurve = flippingCurve;
                pages[0].FlippingCurveFactor = flipFactor;
            }
        }
        
        public void SetBackPageCurveProperties(AnimationCurve idleCurve, float idleFactor, AnimationCurve flippingCurve, float flipFactor)
        {
            if(pages == null)
                pages = GetComponentsInChildren<PageGenerator>();

            if (pages.Length > 0)
            {
                pages[1].IdleCurve = idleCurve;
                pages[1].IdleCurveFactor = idleFactor;
                pages[1].FlippingCurve = flippingCurve;
                pages[1].FlippingCurveFactor = flipFactor;
            }
        }

    }
    
}
