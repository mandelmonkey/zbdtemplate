using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

namespace Beamable.Installer.Editor
{

	public class VerisonNumberPrompt : EditorWindow
	{
		private string VersionNumber = "";
		private Action<string> _callback;

		/// <summary>
		/// Create a Centered screen-relative rectangle, given a parent editor window
		/// </summary>
		/// <param name="window"></param>
		public static Rect GetCenteredScreenRectForWindow(EditorWindow window, Vector2 size)
		{
			var pt = window.position.center;

			var halfSize = size * .5f;
			return new Rect(pt.x - halfSize.x, pt.y - halfSize.y, size.x, size.y);
		}

		public static void GetNumber(EditorWindow source, Action<string> cb)
		{
			var window = EditorWindow.CreateInstance<VerisonNumberPrompt>();
			window._callback = cb;
			window.ShowPopup();
			window.position = GetCenteredScreenRectForWindow(source, new Vector2(300, 200));

		}

		void OnGUI()
		{
			VersionNumber = EditorGUILayout.TextField("Beamable Version", VersionNumber);

			var wasEnabled = GUI.enabled;
			bool versionSelected = false;
			var beamVersionsCache = BeamableInstaller.BeamVersionsCache;
			int versionsToDisplay = 0;

			if (!string.IsNullOrEmpty(VersionNumber) && beamVersionsCache != null)
			{
				var versionSplited = VersionNumber.Split(new[] { ' ' });
				var fittingVersions = beamVersionsCache.Where(version => versionSplited.All(version.Contains)).Take(5);
				foreach (var fittingVersion in fittingVersions)
				{
					if (GUILayout.Button(fittingVersion))
					{
						VersionNumber = fittingVersion;
						versionSelected = true;
					}
					versionsToDisplay ++;
				}
			}

			const int buttonHeight = 21;
			GUILayout.Space((6 - versionsToDisplay) * buttonHeight);
			GUI.enabled = !string.IsNullOrEmpty(VersionNumber) && (beamVersionsCache ?? Array.Empty<string>()).Contains(VersionNumber);
			versionSelected |= GUILayout.Button("Install");
			
			if (versionSelected)
			{
				this.Close();
				_callback?.Invoke(VersionNumber);
			}

			GUI.enabled = wasEnabled;

			if (GUILayout.Button("Cancel"))
			{
				this.Close();
				_callback?.Invoke(null);
			}
		}

	}

	[CustomEditor(typeof(BeamableInstallerReadme))]
	[InitializeOnLoad]
	public class BeamableInstallerReadmeEditor : UnityEditor.Editor
	{

//		static string kShowedReadmeSessionStateName = "Beamable.Installer.ReadmeEditor.showedReadme";
		private bool _advancedFoldout;

		static float kSpace = 16f;
		private static bool hasPackage;

		static BeamableInstallerReadmeEditor()
		{
			EditorApplication.delayCall += SelectReadmeAutomatically;
		}

		static void SelectReadmeAutomatically()
		{
			BeamableInstaller.HasBeamableInstalled(installed =>
			{
				hasPackage = installed;
				if (installed) return;

//				if (!EditorPrefs.GetBool(kShowedReadmeSessionStateName, false))
				{
					var readme = SelectReadme();
//					EditorPrefs.SetBool(kShowedReadmeSessionStateName, true);

					if (readme && !readme.loadedLayout)
					{
						readme.loadedLayout = true;
					}
				}
			});
		}

		[MenuItem(BeamableInstaller.BeamableMenuPath + "Show Readme", priority = 100)]
		static BeamableInstallerReadme SelectReadme()
		{
			var ids = AssetDatabase.FindAssets($"Readme t:{nameof(BeamableInstallerReadme)}");
			if (ids.Length == 1)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));

				Selection.objects = new UnityEngine.Object[] {readmeObject};

				return (BeamableInstallerReadme) readmeObject;
			}
			else
			{
				return null;
			}
		}

		protected override void OnHeaderGUI()
		{
			var readme = (BeamableInstallerReadme) target;
			Init();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

			GUILayout.BeginHorizontal("In BigTitle");
			{
				GUILayout.Space (12);
				GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
				GUILayout.Label(readme.title, TitleStyle);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			var readme = (BeamableInstallerReadme) target;
			Init();

			foreach (var section in readme.sections)
			{
				if (!hasPackage && section.onlyShowWhenInstalled)
				{
					continue;
				}

				if (hasPackage && section.onlyShowWhenNotInstalled)
				{
					continue;
				}

				if (!string.IsNullOrEmpty(section.heading))
				{
					GUILayout.Label(section.heading, HeadingStyle);
				}

				if (!string.IsNullOrEmpty(section.text))
				{
					GUILayout.Label(section.text, BodyStyle);
				}

				if (section.Action != InstallerActionType.None)
				{
					var buttonStyle = new GUIStyle("button");
					buttonStyle.fontSize = 16;

					var enabled = GUI.enabled;
					GUI.enabled = !BeamableInstaller.IsBusy;
					if (GUILayout.Button(section.ActionText, buttonStyle, GUILayout.Height(30)))
					{
						var source = GUILayoutUtility.GetLastRect();

						Event current = Event.current;
						if (Event.current.button == 0)
						{
							BeamableInstaller.RunAction(section.Action);
						}
						else if (section.Action == InstallerActionType.Install && section.IncludeRightClickOptions && Event.current.button == 1)
						{
							GenericMenu menu = new GenericMenu();

							menu.AddItem(new GUIContent("Install Stable Build"), false, func: BeamableInstaller.InstallStable);
							menu.AddItem(new GUIContent("Install Release Candidate"), false, func: BeamableInstaller.InstallRC);
							menu.AddItem(new GUIContent("Install Nightly Build"), false,  BeamableInstaller.InstallDev);
							menu.AddItem(new GUIContent("Install Specific Version"), false, () =>
							{

								VerisonNumberPrompt.GetNumber(EditorWindow.mouseOverWindow, (version) =>
								{
									if (version != null)
									{
										BeamableInstaller.InstallSpecific(version);
									}
								});

							});
							menu.ShowAsContext();

							current.Use();
						}
					}

					if (section.Action == InstallerActionType.Install)
					{

						EditorGUI.indentLevel++;

						EditorGUILayout.Space();
						_advancedFoldout = EditorGUILayout.Foldout(_advancedFoldout, "Advanced Setup");
						if (_advancedFoldout)
						{
							EditorGUILayout.BeginVertical();

							GUI.enabled = false;
							EditorGUILayout.ToggleLeft("Install com.beamable", true); // forced true on purpose, because otherwise, whats the point of the installer??
							GUI.enabled = true;
							GUILayout.BeginHorizontal();
							GUILayout.Space (EditorGUI.indentLevel * 10 + 25);
							GUILayout.Label("The basic Beamable package. This must be installed if any other Beamable packages will be installed. This package provides Beamable frictionless authentication, content, game economy, player inventory, and more.", AdvancedStyle);
							GUILayout.EndHorizontal();

							EditorGUILayout.Space();
							BeamableInstaller.InstallServerPackage = EditorGUILayout.ToggleLeft("Install com.beamable.server", BeamableInstaller.InstallServerPackage);

							GUILayout.BeginHorizontal();
							GUILayout.Space (EditorGUI.indentLevel * 10 + 25);
							GUILayout.Label("The server package allows you to create and deploy Microservices and Microstorages for your game. If you don't install this now, you can install it later from the Beamable Toolbox.", AdvancedStyle);
							GUILayout.EndHorizontal();
							
							
							EditorGUILayout.Space();
							BeamableInstaller.InstallDependencies = EditorGUILayout.ToggleLeft("Install Unity packages dependencies", BeamableInstaller.InstallDependencies);

							GUILayout.BeginHorizontal();
							GUILayout.Space (EditorGUI.indentLevel * 10 + 25);
							GUILayout.Label("TMPro and Addressable Asset packages are used heavily in Beamable. If you don't install this now, you can install it later from the Beamable Toolbox or Unity Packages window.", AdvancedStyle);
							GUILayout.EndHorizontal();

							EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel--;
					}

					GUI.enabled = enabled;
				}

				if (!string.IsNullOrEmpty(section.linkText))
				{
					if (LinkLabel(new GUIContent(section.linkText)))
					{
						Application.OpenURL(section.url);
					}
				}

				GUILayout.Space(kSpace);
			}
		}


		bool m_Initialized;

		GUIStyle LinkStyle
		{
			get { return m_LinkStyle; }
		}

		[SerializeField] GUIStyle m_LinkStyle;

		GUIStyle TitleStyle
		{
			get { return m_TitleStyle; }
		}

		[SerializeField] GUIStyle m_TitleStyle;

		GUIStyle HeadingStyle
		{
			get { return m_HeadingStyle; }
		}

		[SerializeField] GUIStyle m_HeadingStyle;

		GUIStyle BodyStyle
		{
			get { return m_BodyStyle; }
		}

		[SerializeField] GUIStyle m_BodyStyle;

		GUIStyle AdvancedStyle
		{
			get { return m_AdvancedStyle; }
		}

		[SerializeField] GUIStyle m_AdvancedStyle;

		void Init()
		{
			if (m_Initialized)
				return;
			m_BodyStyle = new GUIStyle(EditorStyles.label);
			m_BodyStyle.wordWrap = true;
			m_BodyStyle.fontSize = 14;

			m_AdvancedStyle = new GUIStyle(EditorStyles.label);
			m_AdvancedStyle.wordWrap = true;
			m_AdvancedStyle.fontSize = 10;

			m_TitleStyle = new GUIStyle(m_BodyStyle);
			m_TitleStyle.fontSize = 26;

			m_HeadingStyle = new GUIStyle(m_BodyStyle);
			m_HeadingStyle.fontSize = 18;

			m_LinkStyle = new GUIStyle(m_BodyStyle);
			m_LinkStyle.wordWrap = false;
			// Match selection color which works nicely for both light and dark skins
			m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
			m_LinkStyle.stretchWidth = false;

			m_Initialized = true;
		}

		bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkStyle);
		}
	}

}