using UnityEngine;
using System.Collections;

namespace TurnTheGameOn.DDAFog{
	[ExecuteInEditMode]
	public class DDAFog : MonoBehaviour {

		public enum MyClearFlags { Skybox, SolidColor, DepthOnly, DontClear };
		public enum LerpAxis { x, y, z };
		public enum MyAmbientMode { Skybox, Gradient, Color }

		// Camera Settings
		[Tooltip("Allows this script to control the assigned camera's clear flags and background color settings.")]
		public bool controlCamera = false;
		[Tooltip("Override to control the assigned camera's clear flags setting.")]
		public MyClearFlags clearFlags = MyClearFlags.SolidColor;
		[Tooltip("Override to match the assigned camera's background color setting with the current fog color.")]
		public bool matchFogColor;
		[Tooltip("Override to control the assigned camera's background color setting.")]
		public Color background;
		[Tooltip("Assign a camera to control clear flags and background color settings.")]
		public Camera mainCamera;

		// Environment Lighting Settings
		[Tooltip("Allows this script to control the scene's ambient lighting, normally set in Window/Lighting/Environment Lighting.")]
		public bool controlLighting = false;
		[Tooltip("The source of the ambient light that shines into the scene.")]
		public MyAmbientMode ambientMode;
		[Tooltip("The color used for the ambient light shining into the scene.")]
		public Color ambientColor = Color.grey;
		[Tooltip("Override to match the ambient lighting color setting with the current fog color.")]
		public bool matchFogColorAmbient;
		[Tooltip("Ambient lighting coming from above.")]
		public Color skyColor = Color.grey;
		[Tooltip("Ambient lighting coming from the sides.")]
		public Color equatorColor = Color.grey;
		[Tooltip("Ambient lighting coming from below.")]
		public Color groundColor = Color.grey;
		[Tooltip("How much the light from the Ambient Source affects the scene.")]
		[Range(0,8)] public float ambientIntensity = 1.0f;

		//Fog Settings
		[Tooltip("Override to control the scene's fog settings, normally set in Window/Lighting/Fog.")]
		public bool controlFog = true;
		[Tooltip("Enables fog.")]
		public bool enableFog = true;
		[Tooltip("Transform position for min fog level.")]
		public float minLevel = -0.0f;
		[Tooltip("Transform position for max fog level.")]
		public float maxLevel = 50.0f;
		[Tooltip("Axis used to lerp min and max position fog settings.")]
		public LerpAxis lerpAxis = LerpAxis.y;
		[Tooltip("Transform simulation space that is referenced to control fog settings.")]
		public ParticleSystemSimulationSpace simulationSpace = ParticleSystemSimulationSpace.World;
		[Tooltip("Override to control the scene's fog mode, normally set in Window/Lighting/Fog.")]
		public FogMode fogMode = FogMode.ExponentialSquared;
		[Tooltip("Fog Density as or after max level position.")]
		public float maxLevelDensity = 5.0f;
		[Tooltip("Fog Density as or after min level position.")]
		public float minLevelDensity = 100.0f;
		private float fogDensity;
		[Tooltip("Linear Fog Density Start at or after max level position.")]
		public float maxLevelFogStart = 5.0f;
		[Tooltip("Linear Fog Density Start at or before min level position.")]
		public float minLevelFogStart = 100.0f;
		private float fogStartDistance;
		[Tooltip("Linear Fog Density End at or after max level position.")]
		public float maxLevelFogEnd = 5.0f;
		[Tooltip("Linear Fog Density End at or before min level position.")]
		public float minLevelFogEnd = 100.0f;
		private float fogEndDistance;
		[Tooltip("Gradient used between min and max positions.")]
		public Gradient fogGradient;
		private Color fogColor;
		private float scaleValue;
		[Tooltip("Assign a height marker to have another transform in the scene control fog parameters.")]
		public Transform heightMarker;

		// Global Fog Image Effect Settings
		[Tooltip("Enables global fog.")]
		public bool useGlobalFog;
		[Tooltip("Apply distance-based fog.")]
		public bool  distanceFog = true;
		[Tooltip("Exclude far plane pixels from distance-based fog (Skybox or clear color).")]
		public bool  excludeFarPixels = false;
		[Tooltip("Distance fog is based on radial distance from camera.")]
		public bool  useRadialDistance = false;
		[Tooltip("Apply height-based fog.")]
		public bool  heightFog = true;
		[Tooltip("Fog top Y coordinate.")]
		public float height = 1.0f;
		[Tooltip("Density of fog height.")]
		[Range(0.001f,10.0f)]public float heightDensity = 0.001f;
		[Tooltip("Push fog away from the camera by this amount.")]
		public float startDistance = 10.0f;
		public Shader fogShader;
		private Material fogMaterial = null;

		private bool previousFogState;
		private Color previousFogColor;
		private float previousFogStartDistance;
		private float previousFogEndDistance;
		private FogMode previousFogMode;
		private float previousFogDensity;

		void OnPreRender(){
			previousFogState = RenderSettings.fog;
			previousFogColor = RenderSettings.fogColor;
			previousFogStartDistance = RenderSettings.fogStartDistance;
			previousFogEndDistance = RenderSettings.fogEndDistance;
			previousFogMode = RenderSettings.fogMode;
			previousFogDensity = RenderSettings.fogDensity;

			if(mainCamera != null && controlCamera){
				//set camera clear flags
				if (clearFlags == MyClearFlags.Skybox) {
					mainCamera.clearFlags = CameraClearFlags.Skybox;
				} else if (clearFlags == MyClearFlags.SolidColor) {
					mainCamera.clearFlags = CameraClearFlags.SolidColor;
				} else if (clearFlags == MyClearFlags.DepthOnly) {
					mainCamera.clearFlags = CameraClearFlags.Depth;
				} else if (clearFlags == MyClearFlags.DontClear) {
					mainCamera.clearFlags = CameraClearFlags.Nothing;
				}
				//set camera background color
				if (matchFogColor)
					background = fogColor;
				mainCamera.backgroundColor = background;
			}

			if(controlLighting){					
				if(ambientMode == MyAmbientMode.Skybox){
					RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
				}else if(ambientMode == MyAmbientMode.Gradient){
					RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
					RenderSettings.ambientSkyColor = skyColor;
					RenderSettings.ambientEquatorColor = equatorColor;
					RenderSettings.ambientGroundColor = groundColor;
				}else if(ambientMode == MyAmbientMode.Color){
					if (matchFogColorAmbient)
						ambientColor = fogColor;
					RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
					RenderSettings.ambientLight = ambientColor;
				}
			}
			RenderSettings.ambientIntensity = ambientIntensity;

			if (controlFog) {
				RenderSettings.fog = enableFog;
				GetFogSettings ();
				if (fogMode == FogMode.Exponential) {
					RenderSettings.fogMode = FogMode.Exponential;
					RenderSettings.fogDensity = fogDensity;
				} else if (fogMode == FogMode.ExponentialSquared) {
					RenderSettings.fogMode = FogMode.ExponentialSquared;
					RenderSettings.fogDensity = fogDensity;
				} else if (fogMode == FogMode.Linear) {
					RenderSettings.fogMode = FogMode.Linear;
					RenderSettings.fogStartDistance = fogStartDistance;
					RenderSettings.fogEndDistance = fogEndDistance;
				}
				RenderSettings.fogColor = fogColor;
			}

		}

		void OnPostrender(){
			RenderSettings.fog = previousFogState;
			RenderSettings.fogColor = previousFogColor;
			RenderSettings.fogStartDistance = previousFogStartDistance;
			RenderSettings.fogEndDistance = previousFogEndDistance;
			RenderSettings.fogMode = previousFogMode;
			RenderSettings.fogDensity = previousFogDensity;
		}

		void GetFogSettings(){
			if(heightMarker != null){
				if(lerpAxis == LerpAxis.x){
					if(heightMarker.position.x > minLevel){
						if (simulationSpace == ParticleSystemSimulationSpace.World) {
							scaleValue = Mathf.Abs(minLevel + heightMarker.position.x) / Mathf.Abs( maxLevel - minLevel);
						} else if (simulationSpace == ParticleSystemSimulationSpace.Local) {
							scaleValue = heightMarker.localPosition.x / maxLevel;
						}
					}else{ scaleValue = 0; }
				}else if(lerpAxis == LerpAxis.y){
					if(heightMarker.position.y > minLevel){
						if (simulationSpace == ParticleSystemSimulationSpace.World) {
							scaleValue = Mathf.Abs(minLevel + heightMarker.position.y) / Mathf.Abs( maxLevel - minLevel);
						} else if (simulationSpace == ParticleSystemSimulationSpace.Local) {
							scaleValue = heightMarker.localPosition.y / maxLevel; scaleValue = transform.localPosition.y / maxLevel;
						}
					}else{ scaleValue = 0; }
				}else if(lerpAxis == LerpAxis.z){
					if(heightMarker.position.z > minLevel){
						if (simulationSpace == ParticleSystemSimulationSpace.World) {
							scaleValue = Mathf.Abs(minLevel + heightMarker.position.z) / Mathf.Abs( maxLevel - minLevel);
						} else if (simulationSpace == ParticleSystemSimulationSpace.Local) {
							scaleValue = heightMarker.localPosition.z / maxLevel; scaleValue = transform.localPosition.z / maxLevel;
						}
					}else{ scaleValue = 0; }
				}
			}else{
				if (lerpAxis == LerpAxis.x) {
					if (transform.position.x > minLevel) {
						if (simulationSpace == ParticleSystemSimulationSpace.World)
							scaleValue = transform.position.x / maxLevel;
						else if (simulationSpace == ParticleSystemSimulationSpace.Local)
							scaleValue = transform.localPosition.x / maxLevel;
					}else{ scaleValue = 0; }
				} else if (lerpAxis == LerpAxis.y) {
					if (transform.position.y > minLevel) {
						if (simulationSpace == ParticleSystemSimulationSpace.World)
							scaleValue = transform.position.y / maxLevel;
						else if (simulationSpace == ParticleSystemSimulationSpace.Local)
							scaleValue = transform.localPosition.y / maxLevel;
					}else{ scaleValue = 0; }
				} else if (lerpAxis == LerpAxis.z) {
					if (transform.position.z > minLevel) {
						if (simulationSpace == ParticleSystemSimulationSpace.World)
							scaleValue = transform.position.z / maxLevel;
						else if (simulationSpace == ParticleSystemSimulationSpace.Local)
							scaleValue = transform.localPosition.z / maxLevel;
					}else{ scaleValue = 0; }
				}
			}
			if (scaleValue > 1) {	scaleValue = 1;	}
			else if (scaleValue < 0) {	scaleValue = 0;	}
			if(fogGradient != null)
				fogColor = fogGradient.Evaluate (scaleValue);
			fogDensity = Mathf.Lerp (minLevelDensity, maxLevelDensity, scaleValue);
			fogStartDistance = Mathf.Lerp (maxLevelFogStart, minLevelFogStart, scaleValue);
			fogEndDistance = Mathf.Lerp (maxLevelFogEnd, minLevelFogEnd, scaleValue);
		}

		[ImageEffectOpaque]
		void OnRenderImage (RenderTexture source, RenderTexture destination){
			if (useGlobalFog) {
				fogMaterial = new Material (fogShader);
				if ( (!distanceFog && !heightFog)) {
					Graphics.Blit (source, destination);
					return;
				}

				Camera cam = GetComponent<Camera> ();
				Transform camtr = cam.transform;
				float camNear = cam.nearClipPlane;
				float camFar = cam.farClipPlane;
				float camFov = cam.fieldOfView;
				float camAspect = cam.aspect;

				Matrix4x4 frustumCorners = Matrix4x4.identity;

				float fovWHalf = camFov * 0.5f;

				Vector3 toRight = camtr.right * camNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * camAspect;
				Vector3 toTop = camtr.up * camNear * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);

				Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
				float camScale = topLeft.magnitude * camFar / camNear;

				topLeft.Normalize ();
				topLeft *= camScale;

				Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
				topRight.Normalize ();
				topRight *= camScale;

				Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
				bottomRight.Normalize ();
				bottomRight *= camScale;

				Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
				bottomLeft.Normalize ();
				bottomLeft *= camScale;

				frustumCorners.SetRow (0, topLeft);
				frustumCorners.SetRow (1, topRight);
				frustumCorners.SetRow (2, bottomRight);
				frustumCorners.SetRow (3, bottomLeft);

				var camPos = camtr.position;
				float FdotC = camPos.y - height;
				float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
				float excludeDepth = (excludeFarPixels ? 1.0f : 2.0f);
				fogMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
				fogMaterial.SetVector ("_CameraWS", camPos);
				fogMaterial.SetVector ("_HeightParams", new Vector4 (height, FdotC, paramK, heightDensity * 0.5f));
				fogMaterial.SetVector ("_DistanceParams", new Vector4 (-Mathf.Max (startDistance, 0.0f), excludeDepth, 0, 0));

				var sceneMode = RenderSettings.fogMode;
				var sceneDensity = RenderSettings.fogDensity;
				var sceneStart = RenderSettings.fogStartDistance;
				var sceneEnd = RenderSettings.fogEndDistance;
				Vector4 sceneParams;
				bool linear = (sceneMode == FogMode.Linear);
				float diff = linear ? sceneEnd - sceneStart : 0.0f;
				float invDiff = Mathf.Abs (diff) > 0.0001f ? 1.0f / diff : 0.0f;
				sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
				sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
				sceneParams.z = linear ? -invDiff : 0.0f;
				sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;
				fogMaterial.SetVector ("_SceneFogParams", sceneParams);
				fogMaterial.SetVector ("_SceneFogMode", new Vector4 ((int)sceneMode, useRadialDistance ? 1 : 0, 0, 0));

				int pass = 0;
				if (distanceFog && heightFog)
					pass = 0; // distance + height
				else if (distanceFog)
					pass = 1; // distance only
				else
					pass = 2; // height only
				CustomGraphicsBlit (source, destination, fogMaterial, pass);
			}
		}

		void CustomGraphicsBlit (RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)	{
			if (useGlobalFog) {
				RenderTexture.active = dest;
				fxMaterial.SetTexture ("_MainTex", source);
				GL.PushMatrix ();
				GL.LoadOrtho ();
				fxMaterial.SetPass (passNr);
				GL.Begin (GL.QUADS);
				GL.MultiTexCoord2 (0, 0.0f, 0.0f);
				GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
				GL.MultiTexCoord2 (0, 1.0f, 0.0f);
				GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
				GL.MultiTexCoord2 (0, 1.0f, 1.0f);
				GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
				GL.MultiTexCoord2 (0, 0.0f, 1.0f);
				GL.Vertex3 (0.0f, 1.0f, 0.0f); // TL
				GL.End ();
				GL.PopMatrix ();
			}
		}


	}
}