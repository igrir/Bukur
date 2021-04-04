using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

namespace Bukur
{
    [ExecuteInEditMode]
    public class PageGenerator:MonoBehaviour
    {
        public Material TargetMaterial;
        public Material CopyMaterial;
    
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        
        private Mesh currentMesh;
        
        public int col = 10;
        public int row = 14;
        
        public List<Transform> Handles = new List<Transform>();
        List<Vector3> vertices = new List<Vector3>();
        private Vector3[] verticesArr;

        [Header("Handles")]
        public GameObject topLeft;
        public GameObject bottomLeft;
        public GameObject topRight;
        public GameObject bottomRight;
        
        private Vector2 initialSize;

        public bool IsBackSide;
        
        bool isFlipping;
        public bool IsReleasing;

        public AnimationCurve IdleCurve;
        public float IdleCurveFactor;
        public AnimationCurve FlippingCurve;
        public float FlippingCurveFactor;
        
        public void Generate()
        {
            InstantiateMesh();
            InstantiateMaterial();
            CreateHandle();
        }

        public void InstantiateMesh()
        {
            vertices.Clear();
            meshFilter = this.gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = this.gameObject.AddComponent<MeshFilter>();
            }

            meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            }

            currentMesh = new Mesh();
            currentMesh.name = this.transform.name;


            for (int i = 0; i < row + 1; i++)
            {
                for (int j = 0; j < col + 1; j++)
                {
                    Vector3 vert = new Vector3(j, i, 0);
                    vertices.Add(vert);
                }
            }

        
            List<int> triangles = new List<int>();
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {

                    if (IsBackSide)
                    {
                        triangles.AddRange(new int[] { j + ((i + 1) * (col + 1)) + 1, j + ((i + 1) * (col + 1)), j + (i * (col + 1))});
                        triangles.AddRange(new int[] { j + (i * (col + 1)), j + ((i) * (col + 1)) + 1,j + ((i + 1) * (col + 1)) + 1});
                    }
                    else
                    {
                        triangles.AddRange(new int[] {j + (i * (col + 1)), j + ((i + 1) * (col + 1)), j + ((i + 1) * (col + 1)) + 1});
                        triangles.AddRange(new int[] {j + ((i + 1) * (col + 1)) + 1, j + ((i ) * (col + 1)) + 1, j + (i * (col + 1))});
                    }
                }
            }
        
            currentMesh.vertices = vertices.ToArray();
            currentMesh.triangles = triangles.ToArray();
        
        
            List<Vector2> uvs = new List<Vector2>();
            if (IsBackSide)
            {
                for (int i = 0; i < row + 1; i++)
                {
                    for (int j = col; j >= 0; j--)
                    {
                        var uv = new Vector2((float)j/(float)col, (float)i/(float)row);
                        uvs.Add(uv);
                    }
                }
            }
            else
            {
                for (int i = 0; i < row + 1; i++)
                {
                    for (int j = 0; j < col + 1; j++)
                    {
                        var uv = new Vector2((float)j/(float)col, (float)i/(float)row);
                        uvs.Add(uv);
                    }
                }
                
            }

            currentMesh.uv = uvs.ToArray();
            currentMesh.RecalculateNormals();
            currentMesh.RecalculateTangents();
            
            meshFilter.mesh = currentMesh;
        
        }

        public void InstantiateMaterial()
        {
            meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            }
            
            CopyMaterial = Instantiate(TargetMaterial);
            meshRenderer.material = CopyMaterial;
        }

        public void CreateHandle()
        {
            for (int i = 0; i < Handles.Count; i++)
            {
                if (Handles[i] != null)
                {
                    DestroyImmediate(Handles[i].gameObject);
                }
            }
            Handles.Clear();
            
            // top left
            topLeft = new GameObject();
            topLeft.name = "TopLeft";
            topLeft.transform.parent = this.transform;
            topLeft.transform.localPosition = new Vector3(0, row, 0);
            Handles.Add(topLeft.transform);
            
            // bottom left
            bottomLeft = new GameObject();
            bottomLeft.name = "BottomLeft";
            bottomLeft.transform.parent = this.transform;
            bottomLeft.transform.localPosition = new Vector3(0, 0, 0);
            Handles.Add(bottomLeft.transform);
            
            // top right
            topRight = new GameObject();
            topRight.name = "TopRight";
            topRight.transform.parent = this.transform;
            topRight.transform.localPosition = new Vector3(col, row, 0);
            Handles.Add(topRight.transform);
            
            // bottom right
            bottomRight = new GameObject();
            bottomRight.name = "BottomRight";
            bottomRight.transform.parent = this.transform;
            bottomRight.transform.localPosition = new Vector3(col, 0, 0);
            Handles.Add(bottomRight.transform);


            initialSize = new Vector2(
                Mathf.Abs(bottomLeft.transform.localPosition.x - topRight.transform.localPosition.x),
                Mathf.Abs(bottomLeft.transform.localPosition.y - topRight.transform.localPosition.y));
        }

        private void Update()
        {


            if (bottomLeft == null) return;
            if (topLeft == null) return;
            if (bottomRight == null) return;
            if (topRight == null) return;
            
            if (meshFilter == null)
            {
                meshFilter = this.gameObject.GetComponent<MeshFilter>();
            }
            
            if (vertices == null || vertices.Count <= 0)
            {
                vertices = new List<Vector3>(meshFilter.sharedMesh.vertices);
            }
            
            verticesArr = vertices.ToArray();

            if (verticesArr.Length > 0)
            {
                if (this.IsReleasing)
                {
                    for (int i = 0; i < row + 1; i++)
                    {
                        
                        for (int j = 0; j < col + 1; j++)
                        {
                            var index = j + (i * (col + 1));
                                
                            float xFactor = (float) j / (float) col;
                            float yFactor = (float) i / (float) row;
                            Vector3 position = Vector3.Lerp(
                                Vector3.Lerp(bottomLeft.transform.localPosition, bottomRight.transform.localPosition, xFactor),
                                Vector3.Lerp(topLeft.transform.localPosition, topRight.transform.localPosition, xFactor),
                                yFactor
                            );
                            
                            Vector3 side1 = topRight.transform.position - bottomLeft.transform.position;
                            Vector3 side2 = bottomRight.transform.position - bottomLeft.transform.position;
                            Vector3 perp = Vector3.Cross(side1, side2).normalized;

                            position.z = position.z + perp.z * IdleCurve.Evaluate(xFactor) * IdleCurveFactor;

                            verticesArr[index] = position;
                
                        }
                    }
                }
                else
                {
                    if (!this.isFlipping)
                    {
                        for (int i = 0; i < row + 1; i++)
                        {
                            for (int j = 0; j < col + 1; j++)
                            {
                                var index = j + (i * (col + 1));
                                
                                float xFactor = ((float) j / (float) col);
                                float yFactor = ((float) i / (float) row);
                                Vector3 position = Vector3.Lerp(
                                    Vector3.Lerp(bottomLeft.transform.localPosition, bottomRight.transform.localPosition, xFactor),
                                    Vector3.Lerp(topLeft.transform.localPosition, topRight.transform.localPosition, xFactor),
                                    yFactor
                                    );
                                
                                Vector3 side1 = topRight.transform.position - bottomLeft.transform.position;
                                Vector3 side2 = bottomRight.transform.position - bottomLeft.transform.position;
                                Vector3 perp = Vector3.Cross(side1, side2).normalized;

                                position.z = perp.z * IdleCurve.Evaluate(xFactor) * IdleCurveFactor;
                                
                                verticesArr[index] = position;
                            }
                        }
                    }
                    else if(this.isFlipping)
                    {
                        for (int i = 0; i < row + 1; i++)
                        {
                            for (int j = 0; j < col + 1; j++)
                            {
                                var index = j + (i * (col + 1));
                                float xFactor = ((float) j / (float) col);
                                float yFactor = ((float) i / (float) row);
                                Vector3 position = Vector3.Lerp(
                                    Vector3.Lerp(bottomLeft.transform.localPosition, bottomRight.transform.localPosition, xFactor),
                                    Vector3.Lerp(topLeft.transform.localPosition, topRight.transform.localPosition, xFactor),
                                    yFactor
                                );
                                
                                Vector3 side1 = topRight.transform.position - bottomLeft.transform.position;
                                Vector3 side2 = bottomRight.transform.position - bottomLeft.transform.position;
                                Vector3 perp = Vector3.Cross(side1, side2).normalized;

                                position.z = position.z + (perp.z * FlippingCurve.Evaluate(xFactor) * FlippingCurveFactor);

                                verticesArr[index] = position;
                            }
                        }
                    }
                }
                meshFilter.sharedMesh.vertices = verticesArr;
            }
            
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            initialSize = new Vector2(
                Mathf.Abs(bottomLeft.transform.localPosition.x - topRight.transform.localPosition.x),
                Mathf.Abs(bottomLeft.transform.localPosition.y - topRight.transform.localPosition.y));
        }

        public Vector2 GetSize()
        {
            return initialSize;
        }

        public void SetIsFlipping(bool isFlipping)
        {
            this.isFlipping = isFlipping;
        }

        public void SetTexture(Texture2D texture)
        {
            if (this.meshRenderer == null)
                meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
            this.meshRenderer.sharedMaterial.SetTexture("_MainTex", texture);
        }
    }
}