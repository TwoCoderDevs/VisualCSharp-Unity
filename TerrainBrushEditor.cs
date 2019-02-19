using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace TwoCoderDevs
{
    [CustomEditor(typeof(TerrainBrush))]
    public class TerrainBrushEditor : DisplayLayout
    {
        public TerrainBrush TerrainBrush;
        Preset preset;

        private Vector2 oldMousePos = new Vector2(0, 0);

        public bool test = false;

        public int unity5terrainRefreshCounter = 0;

        GUIContent[] presetContents = new GUIContent[0];

        int presetSelectedFromKeyboard = -1;


        public void OnEnable()
        {
            if (TerrainBrush == null)
            {
                TerrainBrush = (TerrainBrush)target;
            }
            if (TerrainBrush == null || !TerrainBrush.enabled)
                return;

            UnityEditor.EditorApplication.update -= EditorUpdate;
            UnityEditor.EditorApplication.update += EditorUpdate;

            Undo.undoRedoPerformed -= TerrainBrush.PerformUndo;
            Undo.undoRedoPerformed += TerrainBrush.PerformUndo;
        }

        public void OnDisable()
        {
            if (TerrainBrush == null)
                TerrainBrush = (TerrainBrush)target;

            if (TerrainBrush == null)
                return;

            UnityEditor.EditorApplication.update -= EditorUpdate;

            Undo.undoRedoPerformed -= TerrainBrush.PerformUndo;
        }

        #region Inspector

        public override void OnInspectorGUI()
        {
            TerrainBrush = (TerrainBrush)target;
            preset = TerrainBrush.preset;

            GetInspectorField();
            margin = 0;
            rightMargin = 0;
            fieldSize = 0.6f;

            if (TerrainBrush.guiHydraulicIcon == null)
            {
                TerrainBrush.guiHydraulicIcon = Resources.Load("TerrainBrushHydraulic") as Texture2D;
            }
            if (TerrainBrush.guiWindIcon == null)
            {
                TerrainBrush.guiWindIcon = Resources.Load("TerrainBrushNoise") as Texture2D;
            }

            Par(5);
            Par(22);

            if (disabled)
            {
                TerrainBrush.paint = false;
            }
            GUILayout.BeginHorizontal();
            TerrainBrush.paint = GUILayout.Toggle(TerrainBrush.paint, new GUIContent("Paint", "A checkbutton that turns erosion or noise painting on/off. When painting is on it is terrain editing with standard Unity tools is not possible, so terrain component is disabled when “Paint” is checked. To enable terrain editing turn off paint mode."), buttonStyle);
            disabled = false;

            Inset(0.1f);
            bool oldIsErosion = preset.isErosion;
            bool oldIsNoise = preset.isNoise;

            preset.isErosion = GUILayout.Toggle(preset.isErosion, new GUIContent(" Erosion", TerrainBrush.guiHydraulicIcon, ""), GUI.skin.button);
            preset.isNoise = GUILayout.Toggle(preset.isNoise, new GUIContent(" Noise", TerrainBrush.guiWindIcon, ""), GUI.skin.button);


            GUILayout.EndHorizontal();
            bool controlClick = (Event.current.control && Event.current.type == EventType.Used);
            if (!controlClick)
            {
                if (oldIsErosion && oldIsNoise)
                {
                    if (!preset.isNoise && oldIsNoise)
                    {
                        preset.isErosion = false;
                        preset.isNoise = true;
                    }
                    if (!preset.isErosion && oldIsErosion)
                    {
                        preset.isErosion = true;
                        preset.isNoise = false;
                    }
                }
                else
                {
                    if (preset.isNoise && !oldIsNoise)
                    {
                        preset.isErosion = false;
                    }
                    if (preset.isErosion && !oldIsErosion)
                    {
                        preset.isNoise = false;
                    }
                }
            }
            else
            {
                TerrainBrush.guiControlUsed = true;
            }

            if (!preset.isErosion && !preset.isNoise)
            {
                if (oldIsErosion)
                    preset.isErosion = true;

                if (oldIsNoise)
                    preset.isNoise = true;

                if (!preset.isErosion && !preset.isNoise)
                    preset.isErosion = true;
            }

            if (!TerrainBrush.guiControlUsed)
            {
                Par(22);
                EditorGUILayout.HelpBox("Use Control-click to select both modes", MessageType.Info);
            }

            margin += 7;


            #region Preset
            Par(5); Par(); EditorGUILayout.Foldout(TerrainBrush.guiShowPreset, "Preset");
            if (TerrainBrush.guiShowPreset)
            {
                bool reCreate = false;
                if (presetContents.Length != TerrainBrush.presets.Length)
                    reCreate = true;

                else
                    for (int i = 0; i < presetContents.Length; i++)
                        if (presetContents[i].text != TerrainBrush.presets[i].name)
                        {
                            reCreate = true;
                            break;
                        }

                if (reCreate)
                {
                    presetContents = new GUIContent[TerrainBrush.presets.Length];
                    for (int i = 0; i < presetContents.Length; i++)
                    {
                        string postfix = "";
                        if (i < 8) postfix = " (key " + (i + 3) + ")";
                        presetContents[i] = new GUIContent(TerrainBrush.presets[i].name + postfix);
                    }
                }

                margin += 10;
                Par();
                int tempSelectedPreset = EditorGUILayout.Popup(TerrainBrush.guiSelectedPreset, presetContents);
                if (presetSelectedFromKeyboard >= 0)
                {
                    tempSelectedPreset = presetSelectedFromKeyboard;
                    presetSelectedFromKeyboard = -1;
                }

                if (tempSelectedPreset != TerrainBrush.guiSelectedPreset && tempSelectedPreset < TerrainBrush.presets.Length)
                {
                    LoadPreset(tempSelectedPreset);
                    TerrainBrush.guiSelectedPreset = tempSelectedPreset;
                }

                //save, add, remove
                Par(); disabled = TerrainBrush.presets.Length == 0;
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Save", "Save current preset changes")) && EditorUtility.DisplayDialog("Overwrite Preset", "Overwrite currently selected preset?", "Save", "Cancel"))
                    SavePreset(TerrainBrush.guiSelectedPreset);

                disabled = false;
                if (GUILayout.Button(new GUIContent("Save As...", "Save current settings as new preset")))
                {
                    SavePresetWindow window = new SavePresetWindow();
                    window.titleContent = new GUIContent("Save Erosion Brush Preset");
                    window.position = new Rect(window.position.x, window.position.y, window.windowSize.x, window.windowSize.y);
                    window.main = this;
                    window.ShowUtility();
                }

                disabled = TerrainBrush.presets.Length == 0;
                if (GUILayout.Button(new GUIContent("Remove", "Remove currently selected preset")) && EditorUtility.DisplayDialog("Remove Preset", "Are you sure you wish to remove currently selected preset?", "Remove", "Cancel"))
                    RemovePreset(TerrainBrush.guiSelectedPreset);
                GUILayout.EndVertical();
                disabled = false;

                margin -= 10;
            }
            #endregion

            #region brush settings
            Par(5);
            Par();
            EditorGUILayout.Foldout(TerrainBrush.guiShowBrush, "Brush Settings");

            if (TerrainBrush.guiShowBrush)
            {
                margin += 10;

                preset.brushSize = Mathf.Pow(EditorGUILayout.Slider(new GUIContent("Brush Size", "Size of the brush in Unity units. Bigger brush size gives better terrain quality, but too big values can slow painting. Brush size is displayed as brighter circle in scene view. Brush could be resized with [ and ] keys."), Mathf.Pow(preset.brushSize, 0.5f), Mathf.Pow(1, 0.5f), Mathf.Pow(TerrainBrush.guiMaxBrushSize, 0.5f)), 2);
                //Quick<float>(ref preset.brushSize, "Brush Size", min: 1, max: TerrainBrush.guiMaxBrushSize, tooltip: "Size of the brush in Unity units. Bigger brush size gives better terrain quality, but too big values can slow painting. Brush size is displayed as brighter circle in scene view. Brush could be resized with [ and ] keys.", quadratic: true);
                preset.brushFallof = EditorGUILayout.Slider(new GUIContent("Brush Falloff", "Decrease of brush opacity from center to rim. This parameter is specified in percent of the brush size. It is displayed as dark blue circle in scene view. Brush inside of the circle has the full opacity, and gradually decreases toward the bright circle."), preset.brushFallof, 0.01f, 0.99f);
                //Quick<float>(ref preset.brushFallof, "Brush Falloff", min: 0.01f, max: 0.99f, tooltip: "Decrease of brush opacity from center to rim. This parameter is specified in percent of the brush size. It is displayed as dark blue circle in scene view. Brush inside of the circle has the full opacity, and gradually decreases toward the bright circle.");
                preset.brushSpacing = EditorGUILayout.Slider(new GUIContent("Brush Spacing", "When pressing and holding mouse button brush goes on making stamps. TerrainBrush will not place brush at the same position where old brush was placed, but in a little distance. This parameter specifies how far from old brush stamp will be placed new one (while mouse is still pressed). It  is specified in percent of the brush size."), preset.brushSpacing, 0, 1);
                //Quick<float>(ref preset.brushSpacing, "Brush Spacing", min: 0, max: 1, tooltip: "When pressing and holding mouse button brush goes on making stamps. TerrainBrush will not place brush at the same position where old brush was placed, but in a little distance. This parameter specifies how far from old brush stamp will be placed new one (while mouse is still pressed). It  is specified in percent of the brush size.");
                //preset.downscale = Mathf.RoundToInt(Mathf.Pow(EditorGUILayout.Slider(new GUIContent("Downscale", "To perform quick operation on heightmaps of large size brush resolution could be scaled down. This will give less detail, but faster stamp."), Mathf.Pow(preset.downscale, 0.5f), Mathf.Pow(1,0.5f), Mathf.Pow(4,0.5f)),2));
                Quick<int>(ref preset.downscale, "Downscale", min: 1, max: 4, tooltip: "To perform quick operation on heightmaps of large size brush resolution could be scaled down. This will give less detail, but faster stamp.", quadratic: true);
                preset.downscale = Mathf.ClosestPowerOfTwo(preset.downscale);
                EditorGUI.BeginDisabledGroup(preset.downscale == 1);
                Quick<bool>(ref preset.preserveDetail, "Preserve Detail", "All the terrain detail edited with Downscale parameter will be returned on upscale");
                EditorGUI.EndDisabledGroup();

                margin -= 10;
            }
            #endregion

            #region generator settings 
            if (preset.isErosion)
            {
                Par(5); Par();
                Foldout(ref TerrainBrush.guiShowErosion, "Erosion Parameters");
                if (TerrainBrush.guiShowErosion)
                {
                    margin += 10;

                    Quick<float>(ref preset.erosion_durability, "Terrain Durability", "Baserock resistance to water erosion. Low values erode terrain more. Lowering this parameter is mainly needed to reduce the number of brush passes (iterations), but will reduce terrain quality as well.", max: 1);
                    Quick<int>(ref preset.erosion_fluidityIterations, "Fluidity Iterations", "This parameter sets how liquid sediment (bedrock raised by torrents) is. Low parameter value will stick sediment on sheer cliffs, high value will allow sediment to drain in hollows. As this parameter sets number of iterations, increasing it to very high values can slow down performance.", min: 1, max: 10);
                    Quick<float>(ref preset.erosion_amount, "Erosion Amount", "Amount of bedrock that is washed away by torrents. Unlike sediment amount, this parameter sets the amount of bedrock that is subtracted from original terrain. Zero value will not erode terrain by water at all.", max: 2);
                    Quick<float>(ref preset.sediment_amount, "Sediment Amount", "Percent of bedrock raised by torrents that returns back to earth ) Unlike erosion amount, this parameter sets amount of land that is added to terrain. Zero value will not generate any sediment at all.", max: 2);
                    Quick<float>(ref preset.ruffle, "Ruffle", "Adds smoe randomness on the slopes of the cliffs.", max: 1);
                    Quick<float>(ref preset.erosion_smooth, "Smooth", "Applies additional smoothness to terrain in order to fit brush terrain into an existing terrain made with Unity standard tools. Low, but non-zero values can remove small pikes made by wind randomness or left from water erosion. Use low values if your terrain heightmap resolution is low.", max: 1);

                    margin -= 10;
                }
            }


            if (preset.isNoise)
            {
                Par(5);
                Par();
                Foldout(ref TerrainBrush.guiShowErosion, "Noise Parameters");

                if (TerrainBrush.guiShowErosion)
                {
                    margin += 10;

                    int tempSeed = preset.noise_seed;
                    Quick<int>(ref tempSeed, "Seed", "Number to initialize random generator. With the same brush size, noise size and seed the noise value will be constant for each heightmap coordinate.", slider: false);

                    Quick<float>(ref preset.noise_amount, "Amount", tooltip: "Magnitude. How much noise affects the surface", quadratic: true, max: 100f);
                    Quick<float>(ref preset.noise_size, "Size", tooltip: "Wavelength. Sets the size of the highest iteration of fractal noise. High values will create more irregular noise. This parameter represents the percentage of brush size.", max: 1000, quadratic: true);
                    Quick<float>(ref preset.noise_detail, "Detail", "Defines the bias of each fractal. Low values sets low influence of low-sized fractals and high influence of high fractals. Low values will give smooth terrain, high values - detailed and even too noisy.", max: 1);
                    Quick<float>(ref preset.noise_uplift, "Uplift", "When value is 0, noise is subtracted from terrain. When value is 1, noise is added to terrain. Value of 0.5 will mainly remain terrain on the same level, lifting or lowering individual areas.", max: 1);

                    margin -= 10;
                }
            }
            #endregion

            #region texture settings
            Par(5); Par(); Foldout(ref TerrainBrush.guiShowTextures, "Textures");
            if (TerrainBrush.guiShowTextures)
            {
                margin += 10;

                TerrainBrush.terrains = TerrainBrush.GetTerrains();

                if (TerrainBrush.terrains.Length != 0)
                {
                    SplatPrototype[] splats = TerrainBrush.terrains[0].terrainData.splatPrototypes;
                    Texture2D[] textures = new Texture2D[splats.Length];
                    for (int i = 0; i < splats.Length; i++) textures[i] = splats[i].texture;

                    Par();
                    Field<bool>(ref preset.foreground.apply, width: 20);
                    Label("Crag", width: 70);
                    Slider<float>(ref preset.foreground.opacity, width: width - 130, max: 10, quadratic: true);
                    Field<float>(ref preset.foreground.opacity, width: 40);
                    Par(42); TextureSelector(ref preset.foreground.num, textures);

                    Par(5); Par();
                    Field<bool>(ref preset.background.apply, width: 20);
                    Label("Sediment", width: 70);
                    Slider<float>(ref preset.background.opacity, width: width - 130, max: 10, quadratic: true);
                    Field<float>(ref preset.background.opacity, width: 40);
                    Par(42); TextureSelector(ref preset.background.num, textures);
                }

                margin -= 10;
            }
            #endregion

            #region apply to whole terrain
            Par(5); Par(); Foldout(ref TerrainBrush.guiShowGlobal, "Global Brush", "Apply Erosion Brush to whole terrain at once");
            if (TerrainBrush.guiShowGlobal)
            {
                margin += 10; Par();
                if (Button("Apply to Whole Terrain"))
                {
                    TerrainBrush.terrains = TerrainBrush.GetTerrains();

                    TerrainBrush.NewUndo();
                    TerrainBrush.referenceUndoState = TerrainBrush.currentUndoState + 1;
                    Undo.RecordObject(TerrainBrush, "Erosion Brush Stroke");
                    TerrainBrush.currentUndoState++;
                    EditorUtility.SetDirty(TerrainBrush);
                    TerrainBrush.AddGlobalUndo();

                    for (int i = 0; i < TerrainBrush.guiApplyIterations; i++)
                        for (int t = 0; t < TerrainBrush.terrains.Length; t++)
                            TerrainBrush.ApplyBrush(TerrainBrush.terrains[t].transform.position + TerrainBrush.terrains[t].terrainData.size / 2, Mathf.Max(TerrainBrush.terrains[t].terrainData.size.x, TerrainBrush.terrains[t].terrainData.size.y), useFallof: false);
                }
                Quick<int>(ref TerrainBrush.guiApplyIterations, "Iterations", max: 20);
                margin -= 10;
            }
            #endregion

            #region settings
            Par(5); Par(); Foldout(ref TerrainBrush.guiShowSettings, "Settings");
            if (TerrainBrush.guiShowSettings)
            {
                margin += 10;
                Quick<Color>(ref TerrainBrush.guiBrushColor, "Brush Color", "Visual representation of the brush.");
                Quick<float>(ref TerrainBrush.guiBrushThickness, "Brush Thickness", "Visual representation of the brush.", slider: false);
                Quick<int>(ref TerrainBrush.guiBrushNumCorners, "Brush Num Corners", "Visual representation of the brush.", slider: false);
                Quick<int>(ref TerrainBrush.guiMaxBrushSize, "Max Brush Size", "Brush size slider maximum. Note that increasing brush size will reduce performance in the quadratic dependence.", slider: false);
                if (TerrainBrush.guiMaxBrushSize > 100) { Par(40); EditorGUI.HelpBox(Inset(), "Increasing brush size will reduce performance in the quadratic dependence.", MessageType.Warning); }
                margin -= 10;
            }
            #endregion

        }
        public void SavePreset(int num, string name = "", bool saveBrushSize = true, bool saveBrushParams = true, bool saveErosionNoiseParams = true, bool saveSplatParams = true)
        {
            Preset presetCopy = TerrainBrush.preset.Copy();

            if (num < 0 || num >= TerrainBrush.presets.Length)
            {
                presetCopy = preset.Copy();
                presetCopy.name = name;
                presetCopy.saveBrushSize = saveBrushSize;
                presetCopy.saveBrushParams = saveBrushParams;
                presetCopy.saveErosionNoiseParams = saveErosionNoiseParams;
                presetCopy.saveSplatParams = saveSplatParams;

                Array.Resize(ref TerrainBrush.presets, TerrainBrush.presets.Length + 1);
                num = TerrainBrush.presets.Length - 1;
            }

            TerrainBrush.presets[num] = presetCopy;

            LoadPreset(num);
        }

        public void LoadPreset(int num)
        {
            if (num < 0 || num > TerrainBrush.presets.Length - 1)
                return;

            Preset preset = TerrainBrush.presets[num];
            TerrainBrush.guiSelectedPreset = num;

            TerrainBrush.preset.name = preset.name;
            TerrainBrush.preset.saveBrushSize = preset.saveBrushSize;
            TerrainBrush.preset.saveBrushParams = preset.saveBrushParams;
            TerrainBrush.preset.saveErosionNoiseParams = preset.saveErosionNoiseParams;
            TerrainBrush.preset.saveSplatParams = preset.saveSplatParams;

            if (preset.saveBrushSize)
            {
                TerrainBrush.preset.brushSize = preset.brushSize;
            }

            if (preset.saveBrushParams)
            {
                TerrainBrush.preset.brushFallof = preset.brushFallof;
                TerrainBrush.preset.brushSpacing = preset.brushSpacing;
                TerrainBrush.preset.downscale = preset.downscale;
                TerrainBrush.preset.blur = preset.blur;
                TerrainBrush.preset.preserveDetail = preset.preserveDetail;
            }

            if (preset.saveErosionNoiseParams)
            {
                TerrainBrush.preset.isErosion = preset.isErosion;

                TerrainBrush.preset.noise_seed = preset.noise_seed;
                TerrainBrush.preset.noise_amount = preset.noise_amount;
                TerrainBrush.preset.noise_size = preset.noise_size;
                TerrainBrush.preset.noise_detail = preset.noise_detail;
                TerrainBrush.preset.noise_uplift = preset.noise_uplift;
                TerrainBrush.preset.noise_ruffle = preset.noise_ruffle;

                TerrainBrush.preset.erosion_iterations = preset.erosion_iterations;
                TerrainBrush.preset.erosion_durability = preset.erosion_durability;
                TerrainBrush.preset.erosion_fluidityIterations = preset.erosion_fluidityIterations;
                TerrainBrush.preset.erosion_amount = preset.erosion_amount;
                TerrainBrush.preset.sediment_amount = preset.sediment_amount;
                TerrainBrush.preset.wind_amount = preset.wind_amount;
                TerrainBrush.preset.erosion_smooth = preset.erosion_smooth;
                TerrainBrush.preset.ruffle = preset.ruffle;
            }

            if (preset.saveSplatParams)
            {
                TerrainBrush.preset.foreground = preset.foreground;
                TerrainBrush.preset.background = preset.background;
            }

            this.Repaint();
        }

        public void RemovePreset(int num)
        {
            ArrayRemoveAt<Preset>(ref TerrainBrush.presets, num);

            TerrainBrush.guiSelectedPreset = Mathf.Clamp(TerrainBrush.guiSelectedPreset, 0, TerrainBrush.presets.Length - 1);
            LoadPreset(TerrainBrush.guiSelectedPreset);
        }

        #endregion //Inspector region


        #region Scene

        public void OnSceneGUI()
        {
            TerrainBrush = (TerrainBrush)target;
            preset = TerrainBrush.preset;

            if (!TerrainBrush.paint || (Event.current.mousePosition - oldMousePos).sqrMagnitude < 1f) return;

            if (Event.current.type == EventType.KeyDown)
            {
                if (TerrainBrush.guiSelectPresetsUsingNumkeys)
                {
                    int key = -1;
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Alpha3:
                            key = 0;
                            break;

                        case KeyCode.Alpha4:
                            key = 1;
                            break;

                        case KeyCode.Alpha5:
                            key = 2;
                            break;

                        case KeyCode.Alpha6:
                            key = 3;
                            break;

                        case KeyCode.Alpha7:
                            key = 4;
                            break;

                        case KeyCode.Alpha8:
                            key = 5;
                            break;

                        case KeyCode.Alpha9:
                            key = 6;
                            break;
                    }

                    if (key >= 0 && key < TerrainBrush.presets.Length)
                    {
                        LoadPreset(key);
                        TerrainBrush.guiSelectedPreset = key;
                    }
                }

                if (Event.current.keyCode == KeyCode.LeftBracket || Event.current.keyCode == KeyCode.RightBracket)
                {
                    float step = (TerrainBrush.preset.brushSize / 10);
                    step = Mathf.RoundToInt(step);
                    step = Mathf.Max(1, step);

                    if (Event.current.keyCode == KeyCode.LeftBracket)
                        TerrainBrush.preset.brushSize -= step;

                    else
                        TerrainBrush.preset.brushSize += step;

                    TerrainBrush.preset.brushSize = Mathf.Min(TerrainBrush.guiMaxBrushSize, TerrainBrush.preset.brushSize);
                }
            }

            TerrainBrush.terrains = TerrainBrush.GetTerrains();

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            SceneView sceneview = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneview == null || sceneview.camera == null) return;
            Vector2 mousePos = Event.current.mousePosition;
            mousePos = new Vector2(mousePos.x / sceneview.camera.pixelWidth, mousePos.y / sceneview.camera.pixelHeight);
#if UNITY_5_4_OR_NEWER
            mousePos *= EditorGUIUtility.pixelsPerPoint;
#endif
            mousePos.y = 1 - mousePos.y;

            Camera cam = sceneview.camera;
            if (cam == null) return;
            Ray aimRay = cam.ViewportPointToRay(mousePos);

            Vector3 brushPos = Vector3.zero; bool terrainsAimed = false;
            RaycastHit hit;
            for (int t = 0; t < TerrainBrush.terrains.Length; t++)
            {
                Collider terrainCollider = TerrainBrush.terrains[t].GetComponent<Collider>();
                if (terrainCollider == null) continue;
                if (terrainCollider.Raycast(aimRay, out hit, Mathf.Infinity)) { brushPos = hit.point; terrainsAimed = true; }
            }
            if (!terrainsAimed)
                return;

            DrawBrush(brushPos, preset.brushSize, TerrainBrush.terrains, color: TerrainBrush.guiBrushColor, thickness: TerrainBrush.guiBrushThickness, numCorners: TerrainBrush.guiBrushNumCorners);
            DrawBrush(brushPos, preset.brushSize / 2, TerrainBrush.terrains, color: TerrainBrush.guiBrushColor / 2, thickness: TerrainBrush.guiBrushThickness, numCorners: TerrainBrush.guiBrushNumCorners);

            HandleUtility.Repaint();

            if (Event.current.commandName == "FrameSelected")
            {
                Event.current.Use();
                UnityEditor.SceneView.lastActiveSceneView.LookAt(brushPos, UnityEditor.SceneView.lastActiveSceneView.rotation, preset.brushSize * 6, UnityEditor.SceneView.lastActiveSceneView.orthographic, false);
            }

            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0 && !Event.current.alt)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    TerrainBrush.NewUndo();

                    TerrainBrush.referenceUndoState = TerrainBrush.currentUndoState + 1;
                    Undo.RecordObject(TerrainBrush, "TS Terrain Brush");
                    TerrainBrush.currentUndoState++;
                    EditorUtility.SetDirty(TerrainBrush);
                }

                TerrainBrush.AddUndo(brushPos, preset.brushSize);

                TerrainBrush.ApplyBrush(brushPos, preset.brushSize);
            }
        }

        public void DrawBrush(Vector3 pos, float radius, Terrain[] terrains, Color color, float thickness = 3f, int numCorners = 32)
        {
            Handles.color = color;

            Vector3[] corners = new Vector3[numCorners + 1];
            float step = 360f / numCorners;
            for (int i = 0; i <= corners.Length - 1; i++)
            {
                Vector3 corner = new Vector3(Mathf.Sin(step * i * Mathf.Deg2Rad), 0, Mathf.Cos(step * i * Mathf.Deg2Rad)) * radius + pos;

                Terrain terrain = null;
                for (int t = 0; t < terrains.Length; t++)
                {
                    Terrain tc = terrains[t];
                    if (tc.transform.position.x < corner.x && tc.transform.position.z < corner.z && tc.transform.position.x + tc.terrainData.size.x > corner.x && tc.transform.position.z + tc.terrainData.size.z > corner.z)
                    {
                        terrain = tc;
                        break;
                    }
                }

                corners[i] = corner;
                if (terrain != null) corners[i].y = terrain.SampleHeight(corner);
            }
            Handles.DrawAAPolyLine(thickness, corners);
        }

        public void EditorUpdate()
        {
            if (TerrainBrush == null)
                return;

            if (TerrainBrush.terrain == null)
                try
                {
                    TerrainBrush.terrain = TerrainBrush.GetComponent<Terrain>();
                }
                catch (Exception e)
                {
                    UnityEditor.EditorApplication.update -= EditorUpdate;
                    e.GetType();
                }

            if (TerrainBrush.terrain == null)
                return;

            RefreshTerrainGui();
        }

        public void RefreshTerrainGui()
        {
            if (TerrainBrush.moveDown)
            {
                TerrainBrush.moveDown = false;
                UnityEditorInternal.ComponentUtility.MoveComponentDown(TerrainBrush);
            }

            if (TerrainBrush.paint && !TerrainBrush.wasPaint)
            {
                TerrainBrush.wasPaint = true;

                System.Type terrainType = null;
                System.Type[] tmp = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetTypes();
                for (int i = tmp.Length - 1; i >= 0; i--)
                {

                    if (tmp[i].Name == "TerrainInspector")
                    {
                        terrainType = tmp[i];
                        break;
                    }
                }

                object[] editors = Resources.FindObjectsOfTypeAll(terrainType);

                for (int i = 0; i < editors.Length; i++)
                {
                    PropertyInfo toolProp = terrainType.GetProperty("selectedTool", BindingFlags.Instance | BindingFlags.NonPublic);

                    toolProp.SetValue(editors[i], -1, null);

                    UnityEditorInternal.ComponentUtility.MoveComponentUp(TerrainBrush);
                    TerrainBrush.moveDown = true;
                }

                TerrainBrush.terrain.hideFlags = HideFlags.NotEditable;
            }

            if (!TerrainBrush.paint && TerrainBrush.wasPaint)
            {
                TerrainBrush.wasPaint = false;
                TerrainBrush.terrain.hideFlags = HideFlags.None;
            }
        }
        #endregion //Scene region

    }

    public class SavePresetWindow : EditorWindow
    {
        public TerrainBrushEditor main;
        public readonly Vector2 windowSize = new Vector2(300, 120);

        public new string name;
        public bool saveBrushSize = false;
        public bool saveBrushParams = true;
        public bool saveErosionNoiseParams = true;
        public bool saveSplatParams = true;

        public void OnGUI()
        {
            EditorGUIUtility.labelWidth = 50;

            name = EditorGUILayout.TextField("Name:", name);

            EditorGUILayout.Space();
            saveBrushSize = EditorGUILayout.ToggleLeft(new GUIContent("Save Brush Size", "Each time the preset will be selected Brush Size will be set to current one."), saveBrushSize);
            saveBrushParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Brush Parameters", "Brush fallof, spacing, downscale and blur"), saveBrushParams);
            if (main.TerrainBrush.preset.isErosion) saveErosionNoiseParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Erosion Parameters", "Durability, fluidity and amounts"), saveErosionNoiseParams);
            else saveErosionNoiseParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Noise Parameters", "Amount, size, detail, uplift and riffle"), saveErosionNoiseParams);
            saveSplatParams = EditorGUILayout.ToggleLeft(new GUIContent("Save Splat Parameters", "Splats num and opacity"), saveSplatParams);

            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Save", "Save current splat to list")))
            {
                main.SavePreset(-1, name, saveBrushSize, saveBrushParams, saveErosionNoiseParams, saveSplatParams);
                main.TerrainBrush.guiSelectedPreset = main.TerrainBrush.presets.Length - 1;
                this.Close();
            }
        }
    }


}