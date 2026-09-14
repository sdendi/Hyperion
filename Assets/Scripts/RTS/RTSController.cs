using System.Collections.Generic;
using UnityEngine;

namespace RTS
{
    public class RTSController : MonoBehaviour
    {
        [Header("Selection & Interaction")]
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private LayerMask unitLayer = ~0;
        [SerializeField] private DestinationMarker destinationMarkerPrefab;

        [Header("Selection Box Visuals")]
        [SerializeField] private Color boxBorderColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        [SerializeField] private Color boxFillColor = new Color(0.2f, 0.8f, 0.2f, 0.15f);

        private Camera mainCamera;
        private readonly List<RTSUnit> selectedUnits = new List<RTSUnit>();
        private DestinationMarker activeMarker;

        // Box selection state
        private bool isDraggingBox = false;
        private Vector3 dragStartPos;
        private static Texture2D whiteTexture;

        public IReadOnlyList<RTSUnit> SelectedUnits => selectedUnits;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (whiteTexture == null)
            {
                whiteTexture = new Texture2D(1, 1);
                whiteTexture.SetPixel(0, 0, Color.white);
                whiteTexture.Apply();
            }

            if (destinationMarkerPrefab != null)
            {
                activeMarker = Instantiate(destinationMarkerPrefab);
                activeMarker.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            HandleSelectionInput();
            HandleCommandInput();
        }

        private void HandleSelectionInput()
        {
            // Mouse Down (Left)
            if (Input.GetMouseButtonDown(0))
            {
                dragStartPos = Input.mousePosition;
                isDraggingBox = true;
            }

            // Mouse Up (Left)
            if (Input.GetMouseButtonUp(0) && isDraggingBox)
            {
                isDraggingBox = false;
                Vector3 dragEndPos = Input.mousePosition;
                bool isBox = Vector3.Distance(dragStartPos, dragEndPos) > 15f;

                bool isShiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                if (isBox)
                {
                    SelectUnitsInBox(dragStartPos, dragEndPos, isShiftHeld);
                }
                else
                {
                    SelectSingleUnit(dragEndPos, isShiftHeld);
                }
            }
        }

        private void HandleCommandInput()
        {
            // Right Click for Movement
            if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
                {
                    Vector3 destination = hit.point;

                    // Spawn or trigger destination marker
                    if (activeMarker != null)
                    {
                        activeMarker.Play(destination);
                    }

                    // Order selected units to move
                    CommandMove(destination);
                }
            }
        }

        private void SelectSingleUnit(Vector3 screenPos, bool isShiftHeld)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, unitLayer))
            {
                RTSUnit unit = hit.collider.GetComponentInParent<RTSUnit>();
                if (unit != null)
                {
                    if (isShiftHeld)
                    {
                        if (selectedUnits.Contains(unit))
                        {
                            DeselectUnit(unit);
                        }
                        else
                        {
                            SelectUnit(unit);
                        }
                    }
                    else
                    {
                        DeselectAll();
                        SelectUnit(unit);
                    }
                    return;
                }
            }

            // Clicked empty space
            if (!isShiftHeld)
            {
                DeselectAll();
            }
        }

        private void SelectUnitsInBox(Vector3 screenPos1, Vector3 screenPos2, bool isShiftHeld)
        {
            if (!isShiftHeld)
            {
                DeselectAll();
            }

            Bounds viewportBounds = GetViewportBounds(screenPos1, screenPos2);
            RTSUnit[] allUnits = FindObjectsByType<RTSUnit>(FindObjectsSortMode.None);

            foreach (var unit in allUnits)
            {
                Vector3 unitScreenPos = mainCamera.WorldToViewportPoint(unit.transform.position);
                if (unitScreenPos.z > 0 && viewportBounds.Contains(unitScreenPos))
                {
                    if (!selectedUnits.Contains(unit))
                    {
                        SelectUnit(unit);
                    }
                }
            }
        }

        public void SelectUnit(RTSUnit unit)
        {
            if (unit == null || selectedUnits.Contains(unit)) return;
            selectedUnits.Add(unit);
            unit.SetSelected(true);
        }

        public void DeselectUnit(RTSUnit unit)
        {
            if (unit == null) return;
            if (selectedUnits.Remove(unit))
            {
                unit.SetSelected(false);
            }
        }

        public void DeselectAll()
        {
            foreach (var unit in selectedUnits)
            {
                if (unit != null)
                {
                    unit.SetSelected(false);
                }
            }
            selectedUnits.Clear();
        }

        private void CommandMove(Vector3 destination)
        {
            if (selectedUnits.Count == 1)
            {
                selectedUnits[0].MoveTo(destination);
                return;
            }

            // Basic formation spacing for multiple units
            int count = selectedUnits.Count;
            float spacing = 1.5f;
            int cols = Mathf.CeilToInt(Mathf.Sqrt(count));

            for (int i = 0; i < count; i++)
            {
                int row = i / cols;
                int col = i % cols;
                Vector3 offset = new Vector3((col - (cols - 1) * 0.5f) * spacing, 0f, (row - (count / cols) * 0.5f) * spacing);
                selectedUnits[i].MoveTo(destination + offset);
            }
        }

        private Bounds GetViewportBounds(Vector3 screenPos1, Vector3 screenPos2)
        {
            Vector3 v1 = mainCamera.ScreenToViewportPoint(screenPos1);
            Vector3 v2 = mainCamera.ScreenToViewportPoint(screenPos2);
            Vector3 min = Vector3.Min(v1, v2);
            Vector3 max = Vector3.Max(v1, v2);
            min.z = mainCamera.nearClipPlane;
            max.z = mainCamera.farClipPlane;

            Bounds bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        private void OnGUI()
        {
            if (isDraggingBox && Vector3.Distance(dragStartPos, Input.mousePosition) > 15f)
            {
                Rect rect = GetScreenRect(dragStartPos, Input.mousePosition);
                DrawScreenRect(rect, boxFillColor);
                DrawScreenRectBorder(rect, 2f, boxBorderColor);
            }
        }

        private Rect GetScreenRect(Vector3 screenPos1, Vector3 screenPos2)
        {
            screenPos1.y = Screen.height - screenPos1.y;
            screenPos2.y = Screen.height - screenPos2.y;
            Vector3 topLeft = Vector3.Min(screenPos1, screenPos2);
            Vector3 bottomRight = Vector3.Max(screenPos1, screenPos2);
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        private void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }
    }
}
