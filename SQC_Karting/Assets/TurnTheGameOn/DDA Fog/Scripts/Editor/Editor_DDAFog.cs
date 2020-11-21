using UnityEngine;
using UnityEditor;


namespace TurnTheGameOn.DDAFog{

	[CustomEditor(typeof(DDAFog))]
	public class Editor_DDAFog : Editor {

		static bool showCameraControl = true;
		static bool showLightingControl = true;
		static bool showControlFog = true;
		static bool showGlobalFogControl = true;

		public override void OnInspectorGUI(){

			DDAFog _DDAFog = (DDAFog)target;


			if (_DDAFog.mainCamera != null && !_DDAFog.useGlobalFog) {
				if (_DDAFog.mainCamera.renderingPath == RenderingPath.DeferredLighting || _DDAFog.mainCamera.renderingPath == RenderingPath.DeferredShading)
					EditorGUILayout.HelpBox ("Current Camera Rendering Path is set to Deferred, it's recommended to enable Global Fog.", MessageType.Warning);
			}
//Camera Settings
			EditorGUILayout.BeginVertical("Box", GUILayout.Width(Screen.width - 43.0f));
			EditorGUILayout.BeginHorizontal();
	//Control Camera
			SerializedProperty controlCamera = serializedObject.FindProperty ("controlCamera");
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (controlCamera, GUIContent.none, true, GUILayout.Width(20));
			if (EditorGUI.EndChangeCheck ())
				serializedObject.ApplyModifiedProperties ();
			showCameraControl = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), showCameraControl, "  Control Main Camera", true);
			EditorGUILayout.EndHorizontal ();
			if (showCameraControl) {		
				//Clear Flags
				SerializedProperty clearFlags = serializedObject.FindProperty ("clearFlags");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (clearFlags, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Match Fog Color
				SerializedProperty matchFogColor = serializedObject.FindProperty ("matchFogColor");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (matchFogColor, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Background Color
				SerializedProperty background = serializedObject.FindProperty ("background");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (background, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Main Camera
				SerializedProperty mainCamera = serializedObject.FindProperty ("mainCamera");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (mainCamera, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
			}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();

//Control Lighting
			EditorGUILayout.BeginVertical("Box", GUILayout.Width(Screen.width - 43.0f));
			EditorGUILayout.BeginHorizontal();
			SerializedProperty controlLighting = serializedObject.FindProperty ("controlLighting");
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (controlLighting, GUIContent.none, true, GUILayout.Width(20));
			if (EditorGUI.EndChangeCheck ())
				serializedObject.ApplyModifiedProperties ();
			showLightingControl = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), showLightingControl, "  Control Environment Lighting", true);
			EditorGUILayout.EndHorizontal ();
			if (showLightingControl) {
				//Ambient Mode
				SerializedProperty ambientMode = serializedObject.FindProperty ("ambientMode");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (ambientMode, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				if (_DDAFog.ambientMode == TurnTheGameOn.DDAFog.DDAFog.MyAmbientMode.Color) {
					//Match Ambient lighting with fog color
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Match Fog Color", GUILayout.Width (Screen.width * 0.42f));
					SerializedProperty matchFogColorAmbient = serializedObject.FindProperty ("matchFogColorAmbient");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (matchFogColorAmbient, GUIContent.none, true, GUILayout.Width(30));
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();
					EditorGUILayout.EndHorizontal ();
				}
				if (_DDAFog.ambientMode == TurnTheGameOn.DDAFog.DDAFog.MyAmbientMode.Gradient) {
					SerializedProperty skyColor = serializedObject.FindProperty ("skyColor");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (skyColor, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();
					SerializedProperty equatorColor = serializedObject.FindProperty ("equatorColor");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (equatorColor, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();
					SerializedProperty groundColor = serializedObject.FindProperty ("groundColor");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (groundColor, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();
				} else if (_DDAFog.ambientMode == TurnTheGameOn.DDAFog.DDAFog.MyAmbientMode.Color){
					SerializedProperty ambientColor = serializedObject.FindProperty ("ambientColor");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (ambientColor, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();
				}
				SerializedProperty ambientIntensity = serializedObject.FindProperty ("ambientIntensity");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (ambientIntensity, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
			}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();

//Fog Settings
			EditorGUILayout.BeginVertical("Box", GUILayout.Width(Screen.width - 43.0f));
			//Control Fog
			EditorGUILayout.BeginHorizontal();
			SerializedProperty controlFog = serializedObject.FindProperty ("controlFog");
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (controlFog, GUIContent.none, true, GUILayout.Width(20));
			if (EditorGUI.EndChangeCheck ())
				serializedObject.ApplyModifiedProperties ();			
			showControlFog = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), showControlFog, "  Control Fog", true);
			EditorGUILayout.EndHorizontal ();
	
			if (showControlFog) {
				//Enable Fog
				SerializedProperty enableFog = serializedObject.FindProperty ("enableFog");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (enableFog, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Fog Position
				SerializedProperty minLevel = serializedObject.FindProperty ("minLevel");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (minLevel, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();

				SerializedProperty maxLevel = serializedObject.FindProperty ("maxLevel");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (maxLevel, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Lerp Axis
				SerializedProperty lerpAxis = serializedObject.FindProperty ("lerpAxis");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (lerpAxis, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Simulation Space
				SerializedProperty simulationSpace = serializedObject.FindProperty ("simulationSpace");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (simulationSpace, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Height Marker
				SerializedProperty heightMarker = serializedObject.FindProperty ("heightMarker");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (heightMarker, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Fog Mode
				SerializedProperty fogMode = serializedObject.FindProperty ("fogMode");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (fogMode, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				if (_DDAFog.fogMode == FogMode.Linear) {
					//Linear Fog Start
					SerializedProperty minLevelFogStart = serializedObject.FindProperty ("minLevelFogStart");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (minLevelFogStart, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();

					SerializedProperty minLevelFogEnd = serializedObject.FindProperty ("minLevelFogEnd");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (minLevelFogEnd, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();

					SerializedProperty maxLevelFogStart = serializedObject.FindProperty ("maxLevelFogStart");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (maxLevelFogStart, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();

					SerializedProperty maxLevelFogEnd = serializedObject.FindProperty ("maxLevelFogEnd");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (maxLevelFogEnd, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();

				} else {
					//Fog Density
					SerializedProperty minLevelDensity = serializedObject.FindProperty ("minLevelDensity");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (minLevelDensity, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();

					SerializedProperty maxLevelDensity = serializedObject.FindProperty ("maxLevelDensity");
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (maxLevelDensity, true);
					if (EditorGUI.EndChangeCheck ())
						serializedObject.ApplyModifiedProperties ();
				}
				SerializedProperty fogGradient = serializedObject.FindProperty ("fogGradient");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (fogGradient, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
			}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();

	//Use Global Fog
			EditorGUILayout.BeginVertical("Box", GUILayout.Width(Screen.width - 43.0f));
			EditorGUILayout.BeginHorizontal();
			SerializedProperty useGlobalFog = serializedObject.FindProperty ("useGlobalFog");
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (useGlobalFog, GUIContent.none, true, GUILayout.Width(20));
			if (EditorGUI.EndChangeCheck ())
				serializedObject.ApplyModifiedProperties ();
			showGlobalFogControl = EditorGUI.Foldout (EditorGUILayout.GetControlRect (), showGlobalFogControl, "  Global Fog", true);
			EditorGUILayout.EndHorizontal ();
			if (showGlobalFogControl) {
				//Use Distance Fog
				SerializedProperty distanceFog = serializedObject.FindProperty ("distanceFog");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (distanceFog, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Exclude Far Pixels
				SerializedProperty excludeFarPixels = serializedObject.FindProperty ("excludeFarPixels");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (excludeFarPixels, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Radial Distance
				SerializedProperty useRadialDistance = serializedObject.FindProperty ("useRadialDistance");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (useRadialDistance, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Height Fog
				SerializedProperty heightFog = serializedObject.FindProperty ("heightFog");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (heightFog, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Height
				SerializedProperty height = serializedObject.FindProperty ("height");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (height, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Height Density
				SerializedProperty heightDensity = serializedObject.FindProperty ("heightDensity");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (heightDensity, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Start Distance
				SerializedProperty startDistance = serializedObject.FindProperty ("startDistance");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (startDistance, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
				//Fog Shader
				SerializedProperty fogShader = serializedObject.FindProperty ("fogShader");
				EditorGUI.BeginChangeCheck ();
				EditorGUILayout.PropertyField (fogShader, true);
				if (EditorGUI.EndChangeCheck ())
					serializedObject.ApplyModifiedProperties ();
			}
			EditorGUILayout.EndVertical ();

		}	

	}
}