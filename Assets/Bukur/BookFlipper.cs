using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.UIElements;
using UnityEngine;

namespace Bukur
{
    public class BookFlipper : MonoBehaviour
    {

        [SerializeField] private Camera camera;
        
        public List<Sheet> Sheets = new List<Sheet>();

        List<Sheet> LeftSheets = new List<Sheet>();
        List<Sheet> RightSheets = new List<Sheet>();
        
        // private int currentPage = 0;

        [SerializeField] private Collider leftArea;
        [SerializeField] private Collider rightArea;
        
        private Collider lastHitArea;
        private Vector3 currentDownPos;
        private Vector3 endPos;

        private Sheet currentFlippingSheet;
        
        public float BetweenPagesSpace = 0.3f;
        public AnimationCurve IdleCurve;
        public float IdleCurveFactor;
        public AnimationCurve FlippingCurve;
        public float FlippingCurveFactor;

        [Header("Settings")]
        public int SheetCount = 4;

        private int FlippedSheetIndex = 0;
        
        [Header("Page Textures")]
        public List<Texture2D> Pages = new List<Texture2D>();

        // Start is called before the first frame update
        void Start()
        {
            RightSheets.Clear();
            // initial release
            for (int i = Sheets.Count-1; i >= 0 ; i--)
            {
                var sheet = Sheets[i];
                sheet.Init();
                sheet.FlipRight(isImmediate:true);
            }
            
            
            // initial left sheets
            for (int i = Sheets.Count-1; i >= 0 ; i--)
            {
                RightSheets.Add(Sheets[i]);
            }
            
            SetCurves();

            InitPageSheets();

        }

        void SetCurves()
        {
            for (int i = 0; i < Sheets.Count; i++)
            {
                Sheets[i].SetTopPageCurveProperties(IdleCurve, IdleCurveFactor, FlippingCurve, FlippingCurveFactor);
                Sheets[i].SetBackPageCurveProperties(IdleCurve, -IdleCurveFactor, FlippingCurve, -FlippingCurveFactor);
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                FlipLeft(isImmediate: true);
            }else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                FlipRight(isImmediate: true);
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePos);
                
                if (Physics.Raycast(ray, out var info))
                {
                    if (info.collider == rightArea)
                    {
                        if (RightSheets.Count > 0)
                        {
                            currentFlippingSheet = RightSheets[RightSheets.Count-1];
                        }
                    }else if (info.collider == leftArea)
                    {

                        if (LeftSheets.Count > 0)
                        {
                            currentFlippingSheet = LeftSheets[LeftSheets.Count-1];
                        }

                    }

                    if (currentFlippingSheet != null)
                    {
                        currentFlippingSheet.SetFlipping(true);
                    }
                }
            }
            
            
            if (Input.GetMouseButton(0))
            {
                var mousePos = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out var info))
                {
                    if (info.collider == rightArea || info.collider == leftArea)
                    {
                        lastHitArea = info.collider;   
                        
                        currentDownPos = info.point;

                    }
                }
                
                if (currentFlippingSheet != null)
                {
                    currentFlippingSheet.SetIsReleasing(false);
                    
                    var pageWidth = currentFlippingSheet.GetSize().x;

                    float zRatio = ((currentDownPos.x - this.transform.position.x) / pageWidth) * (float)Math.PI/2f;
                    var zPos = Mathf.Cos(zRatio ) * -pageWidth;
                    var xPos = Mathf.Sin(zRatio) * pageWidth;
                    
                    var topPos = new Vector3(xPos, currentFlippingSheet.TopHandle.transform.localPosition.y, zPos);
                    var botPos = new Vector3(xPos, currentFlippingSheet.BottomHandle.transform.localPosition.y, zPos);

                    currentFlippingSheet.TopHandle.transform.localPosition = Vector3.Lerp(currentFlippingSheet.TopHandle.transform.localPosition, topPos, Time.deltaTime * 15);
                    currentFlippingSheet.BottomHandle.transform.localPosition = Vector3.Lerp(currentFlippingSheet.BottomHandle.transform.localPosition , botPos, Time.deltaTime * 10);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                var mousePos = Input.mousePosition;
                Ray ray = camera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out var info))
                {
                    ReleasePage(info.collider);
                }
                else
                {
                    ReleasePage(lastHitArea);
                }

                if (currentFlippingSheet != null)
                {
                    currentFlippingSheet.SetFlipping(false);
                    currentFlippingSheet = null;
                }
            }
            
           UpdateSheetsOrder();
            
        }

        void UpdateSheetsOrder()
        {
            for (int i = 0; i < LeftSheets.Count; i++)
            {
                var prevPos = LeftSheets[i].transform.localPosition;
                LeftSheets[i].transform.localPosition = Vector3.Lerp(prevPos, new Vector3(prevPos.x, prevPos.y, (LeftSheets.Count-i-1)  * this.BetweenPagesSpace), Time.deltaTime * 10) ;
            }
            
            for (int i = 0; i < RightSheets.Count; i++)
            {
                var prevPos = RightSheets[i].transform.localPosition;
                RightSheets[i].transform.localPosition = Vector3.Lerp(prevPos, new Vector3(prevPos.x, prevPos.y, (RightSheets.Count-i-1) * this.BetweenPagesSpace), Time.deltaTime * 10);
            }
        }
        
        void ReleasePage(Collider releaseArea) {
            if (releaseArea == leftArea)
            {
                if (currentFlippingSheet != null)
                {
                    ProcessFlipLeft(currentFlippingSheet, isImmediate: false);
                }
            }else if (releaseArea == rightArea)
            {
                if (currentFlippingSheet != null)
                {
                    ProcessFlipRight(currentFlippingSheet, isImmediate: false);
                }
            }
        }

        void FlipLeft(bool isImmediate)
        {
            var index = RightSheets.Count - 1;
            if (index >= 0)
            {
                var targetSheet = RightSheets[index];
                
                ProcessFlipLeft(targetSheet, isImmediate);
            }
        }
        
        void FlipRight(bool isImmediate)
        {
            var index = LeftSheets.Count - 1;
            if (index >= 0)
            {
                var targetSheet = LeftSheets[index];

                ProcessFlipRight(targetSheet, isImmediate);
            }
        }

        void ProcessFlipLeft(Sheet targetSheet, bool isImmediate)
        {
            if (!LeftSheets.Contains(targetSheet))
            {
                RightSheets.Remove(targetSheet);
                LeftSheets.Add(targetSheet);

                FlippedSheetIndex++;
                
                // move one left sheets to the right
                if (FlippedSheetIndex > 2  && FlippedSheetIndex < SheetCount-1)
                {
                    var teleportedSheet = LeftSheets[0];
                    teleportedSheet.FlipRight(isImmediate: true);
                    RightSheets.Insert(0, teleportedSheet);
                    LeftSheets.Remove(teleportedSheet);
                    UpdateSheetsOrder();
                   
                    // update its value
                    teleportedSheet.SetTopPageTexture(Pages[(FlippedSheetIndex*2)+2]);
                    teleportedSheet.SetBackPageTexture(Pages[(FlippedSheetIndex*2)+3]);
                }
            }
            targetSheet.FlipLeft(isImmediate);

        }

        void ProcessFlipRight(Sheet targetSheet, bool isImmediate)
        {
            if (!RightSheets.Contains(targetSheet))
            {
                LeftSheets.Remove(targetSheet);
                RightSheets.Add(targetSheet);

                FlippedSheetIndex--;
                

                // move one right sheets to the left
                if (FlippedSheetIndex >= 2 && FlippedSheetIndex < SheetCount-2)
                {
                    var teleportedSheet = RightSheets[0];
                    teleportedSheet.FlipLeft(isImmediate: true);
                    LeftSheets.Insert(0, teleportedSheet);
                    RightSheets.Remove(teleportedSheet);
                    UpdateSheetsOrder();
                    
                    // update its value
                    teleportedSheet.SetTopPageTexture(Pages[(FlippedSheetIndex*2)-4]);
                    teleportedSheet.SetBackPageTexture(Pages[(FlippedSheetIndex*2)-3]);
                }
            }
            targetSheet.FlipRight(isImmediate);
        }

        void InitPageSheets()
        {
            if (SheetCount < 4)
            {
                for (int i = Sheets.Count-1; i >= 0; i--)
                {
                    if (i > SheetCount-1)
                    {
                        Sheets[i].gameObject.SetActive(false);
                    }
                }
            }
            
            // initial texture pages
            int itPage = 0;
            for (int i = RightSheets.Count - 1; i >= 0 ; i--)
            {
                RightSheets[i].SetTopPageTexture(Pages[itPage]);
                itPage++;
                RightSheets[i].SetBackPageTexture(Pages[itPage]);
                itPage++;
            }
        }
        
    }
}
