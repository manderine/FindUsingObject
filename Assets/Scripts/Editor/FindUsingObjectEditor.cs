//#define NGUI_USED

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class FindUsingObjectEditor : EditorWindow {
	public enum PROC_TYPE {
		NONE = 0,

		LOAD_START,
		LOAD_READY,
		LOAD_FILE_LIST,
		LOAD_DATA_LIST,
		LOAD_COMPLETE,

		SEARCH_START,
		SEARCH_READY,
		SEARCH_LIST,
		SEARCH_COMPLETE,
	}

	public enum PROC_KIND {
		BASIC = 0,
		NOT_GAMEOBJECT
	}

	Object _SelectObject = null;
    Object _FindObject = null;
	string _SelectName = "";
	int _is_select_lock = 0;
	int _is_sprite_lock = 0;

    Vector2 _Scroll = Vector2.zero;
	List<Object> _Oris = new List<Object>();
    List<Object> _Objs = new List<Object>();
    List<string> _PathList = new List<string>();

	List<Component> _Components = new List<Component>();

	PROC_TYPE _Type = PROC_TYPE.NONE;
	PROC_KIND _Kind = PROC_KIND.BASIC;
	int _Progress = 0;

	[MenuItem("jTools/Find Using Object Editor.", priority = 202)]
	static void Init() {
		FindUsingObjectEditor window = GetWindow<FindUsingObjectEditor>( "Find Using Object Editor." );
        window.Show();
	}

	void OnEnable() {
    }

	void OnDisable() {
    }

    void OnInspectorUpdate() {
        Repaint();
    }

	string _DirectoryReplace( string path ) {
		path = path.Replace( "\\", "/" );
		int find = path.LastIndexOf( "Assets/" );
        if( find > 0 ) {
		    path = path.Substring( find );
        }
		return path;
	}

    void _DirectoryFiles( List<string> lst, DirectoryInfo dir, string ext ) {
		FileInfo [] fis = dir.GetFiles( ext );
		if( fis != null ) {
			for( int i=0; i<fis.Length; i++ ) {
				FileInfo fi = fis[i];
				if( fi.Extension.CompareTo( ".meta" ) == 0 ) {
					continue;
				}

				string notextension = Path.GetFileNameWithoutExtension( fi.Name );
				if( notextension == this.GetType().Name ) {
					continue;
				}

				string str = _DirectoryReplace( fi.DirectoryName + "/" ) + notextension + fi.Extension;
				lst.Add( str );
			}
			fis = null;
		}

		DirectoryInfo [] dis = dir.GetDirectories();
		if( dis != null ) {
			for( int i=0; i<dis.Length; i++ ) {
				DirectoryInfo di = dis[i];
				_DirectoryFiles( lst, di, ext );
			}
			dis = null;
		}
    }

    void GetFiles( List<string> lst, string path, string ext ) {
        DirectoryInfo dir = new DirectoryInfo( path );
        _DirectoryFiles( lst, dir, ext );
        dir = null;
    }

	void GetComponentsInChildren( List<Component> comps, Transform form, System.Type type ) {
		Component comp = form.GetComponent( type );
		if( comp != null ) {
			comps.Add( comp );
		}

		for( int i=0; i<form.childCount; i++ ) {
			GetComponentsInChildren( comps, form.GetChild( i ), type );
		}
	}

	void OnGUI() {
		if( _is_select_lock == 0 ) {
			_SelectObject = Selection.activeObject;
		}

		float x = 0, y = 0, w = Screen.width, h = Screen.height - 23;
		DrawWindow( x, y, w, h );

		switch( _Type ) {
		case PROC_TYPE.LOAD_START :
			_Type = PROC_TYPE.LOAD_READY;
			break;
		case PROC_TYPE.LOAD_READY :
			EditorUtility.DisplayProgressBar( "Find Using Object", "파일 리스트 정리", 0.0f );

			_Type = PROC_TYPE.LOAD_FILE_LIST;
			break;
		case PROC_TYPE.LOAD_FILE_LIST :
			_PathList.Clear();
			GetFiles( _PathList, "Assets/", "*.*" );

			EditorUtility.DisplayProgressBar( "Find Using Object", "데이터 로딩 시작", 0.1f );

			_Type = PROC_TYPE.LOAD_DATA_LIST;
			_Progress = 0;

			_Oris.Clear();
			break;
		case PROC_TYPE.LOAD_DATA_LIST :
			int count = Mathf.Max( 1, _PathList.Count / 9 );
			int next = Mathf.Min( _PathList.Count, _Progress + count );

			//GameObject ori;
			for( ; _Progress<next; _Progress++ ) {
				DirectoryInfo dir = new DirectoryInfo( _PathList[ _Progress ] );
				switch( dir.Extension.ToLower() ) {
				case ".prefab":
					{
						GameObject ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(GameObject) ) as GameObject;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".shader":
					{
						Shader ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(Shader) ) as Shader;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".cs":
					{
						MonoScript ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(MonoScript) ) as MonoScript;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".mat":
					{
						Material ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(Material) ) as Material;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".controller":
					{
						RuntimeAnimatorController ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(RuntimeAnimatorController) ) as RuntimeAnimatorController;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".anim":
					{
						AnimationClip ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(AnimationClip) ) as AnimationClip;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".psd":
				case ".png":
				case ".jpg":
				case ".tga":
					{
						Texture2D ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(Texture2D) ) as Texture2D;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".fbx":
					break;
				case ".wav":
				case ".mp3":
				case ".ogg":
					{
						AudioClip ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(AudioClip) ) as AudioClip;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".xml":
				case ".txt":
					{
						TextAsset ori = AssetDatabase.LoadAssetAtPath( _PathList[ _Progress ], typeof(TextAsset) ) as TextAsset;
						if( ori != null ) {
							_Oris.Add( ori );
						} else {
							Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
						}
					}
					break;
				case ".unity":
				case ".pdf":
				case ".guiskin":
				case ".dll":
				case ".dylib":
					break;
				default:
					Debug.Log( dir.Extension.ToString() + ":" + dir.Name );
					break;
				}
			}

			if( _Progress < _PathList.Count ) {
				float per = 0.1f + 0.9f * (float)_Progress / _PathList.Count;
				EditorUtility.DisplayProgressBar( "Find Using Object", "데이터 로딩중 - " + ((int)(per * 100)).ToString() + "%", per );
			} else {
				EditorUtility.DisplayProgressBar( "Find Using Object", "데이터 로딩중 - 100%", 1.0f );

				_Type = PROC_TYPE.LOAD_COMPLETE;
			}
			break;
		case PROC_TYPE.LOAD_COMPLETE :
			EditorUtility.ClearProgressBar();

			_Type = PROC_TYPE.SEARCH_START;
			break;
		case PROC_TYPE.SEARCH_START :
			_Type = PROC_TYPE.SEARCH_READY;
			break;
		case PROC_TYPE.SEARCH_READY :
			_Type = PROC_TYPE.SEARCH_LIST;

			_Objs.Clear();
			break;
		case PROC_TYPE.SEARCH_LIST:
			EditorUtility.DisplayProgressBar( "Find Using Object", "파일 검색중", 1.0f );

			FindResources( _FindObject, ref _Objs );

			_Type = PROC_TYPE.SEARCH_COMPLETE;
			break;
		case PROC_TYPE.SEARCH_COMPLETE:
			EditorUtility.ClearProgressBar();

			_Type = PROC_TYPE.NONE;
			break;
		}
	}

	void DrawWindow( float x, float y, float w, float h ) {
        float hh = 60;
        DrawBasicInfo( x, y, w, hh );
        y += hh;
        h -= hh;

        DrawDetailInfo( x, y, w, h );
    }

    void GUI_Box( Rect rt, string label ) {
		GUIStyle style = new GUIStyle (GUI.skin.box);
		style.normal = GUI.skin.button.active;
		GUI.Box (rt, label, style);
	}

	void GUI_LabelField( Rect rt, string label ) {
		GUIStyle style = new GUIStyle (GUI.skin.label);
		style.alignment = TextAnchor.MiddleLeft;
		EditorGUI.LabelField (rt, label, style);
	}

	string GUI_TextField( Rect rt, string label, string data, float labelSize ) {
		float w = rt.width;
		if( labelSize > 0 ) {
			rt.width = labelSize;
			GUI_LabelField( rt, label );
		}
		
		rt.x += labelSize;
		rt.width = w - labelSize;
		return EditorGUI.TextField( rt, data );
	}

	Object GUI_ObjectField( Rect rt, string label, Object data, System.Type type, float labelSize ) {
		float w = rt.width;
		if( labelSize > 0 ) {
			rt.width = labelSize;
			GUI_LabelField( rt, label );
		}
		
		rt.x += labelSize;
		rt.width = w - labelSize;
		return EditorGUI.ObjectField( rt, data, type, false );
	}
	
	int GUI_BoolField( Rect rt, string label, int data ) {
		bool bData = (data != 0)?true:false;
		if( EditorGUI.ToggleLeft( rt, label, bData ) == true ) {
			return 1;
		}
		return 0;
	}

    void DrawBasicInfo( float x, float y, float w, float h ) {
        Rect rt = new Rect( x, y, w, h );
        GUI_Box( rt, "" );

        float xx = x + 5;
        float yy = y + 5;
        float ww = 300;
        float hh = 20;
        float wwSize = 10, hhSize = 5;
        float labelSize = 120;

		rt.Set( xx, yy, ww, hh );
        if( _SelectObject != null ) {
		    GUI_ObjectField( rt, "Select Object : ", _SelectObject, _SelectObject.GetType(), labelSize );
        } else {
            GUI_LabelField( rt, "Not Found Object !!" );
        }

		if( (_SelectObject != null) && (_SelectObject is GameObject) ) {
#if NGUI_USED
			UIAtlas atlas = (_SelectObject as GameObject).GetComponent<UIAtlas>();
			if( atlas != null ) {
				if( _is_sprite_lock == 1 ) {
					if( Selection.activeObject != null ) {
						_SelectName = Selection.activeObject.name;
					}
				}

				rt.Set( xx, yy + hh + hhSize, ww, hh );
				_SelectName = GUI_TextField( rt, "Sprite Name : ",   _SelectName, labelSize );

				if( _is_select_lock == 1 ) {
					Rect rt2 = new Rect( xx + rt.width + wwSize, rt.y, 100, hh );
					_is_sprite_lock = GUI_BoolField( rt2, "is Sprite Lock", _is_sprite_lock );
				} else {
					_is_sprite_lock = 0;
				}
			} else {
				_is_sprite_lock = 0;
				_SelectName = "";
			}
#else
			_is_sprite_lock = 0;
			_SelectName = "";
#endif
		} else {
			_is_sprite_lock = 0;
			_SelectName = "";
		}
		xx += (rt.width + wwSize);

		rt.Set( xx, yy, 100, hh );
		_is_select_lock = GUI_BoolField( rt, "is Select Lock", _is_select_lock );
		xx += (rt.width + wwSize);

		rt.Set( xx, yy, ww, hh );
        if( _FindObject != null ) {
		    GUI_ObjectField( rt, "Find Object : ", _FindObject, _FindObject.GetType(), labelSize );
        } else {
            GUI_LabelField( rt, "Not Found Object !!" );
        }
		xx += (rt.width + wwSize);

		xx = x + w - 450;
		ww = 150;

		rt.Set( xx, yy + hh, ww, hh );
        if( GUI.Button( rt, "Find" ) ) {
            GUI.FocusControl( "Find" );

			if( _PathList.Count == 0 ) {
				_Type = PROC_TYPE.LOAD_START;
			} else {
				_Type = PROC_TYPE.SEARCH_START;
			}
			_Kind = PROC_KIND.BASIC;
			_FindObject = _SelectObject;
			_Objs.Clear();
        }
		xx += ww;

		rt.Set( xx, yy + hh, ww, hh );
        if( GUI.Button( rt, "Find(not prefab)" ) ) {
            GUI.FocusControl( "Find(not prefab)" );

			if( _PathList.Count == 0 ) {
				_Type = PROC_TYPE.LOAD_START;
			} else {
				_Type = PROC_TYPE.SEARCH_START;
			}
			_Kind = PROC_KIND.NOT_GAMEOBJECT;
			_FindObject = _SelectObject;
			_Objs.Clear();
        }
		xx += ww;

		rt.Set( xx, yy + hh, ww, hh );
        if( GUI.Button( rt, "Select All" ) ) {
            GUI.FocusControl( "Select All" );

			if( _Objs.Count > 0 ) {
				Selection.objects = _Objs.ToArray();
			}
        }
    }

    void DrawDetailInfo( float x, float y, float w, float h ) {
        Rect rt = new Rect( x, y, w, h );
        GUI_Box( rt, "" );

        if( _Objs.Count == 0 ) {
            return;
        }

        float labelSize = 200;
        float hhSize = 20;
        float hhGap = 2;

        rt.Set( x, y, w, h );

        Rect rt2 = new Rect();
        float height = rt.height;
        float length = _Objs.Count * (hhGap + hhSize + hhGap);
        if( height < length ) {
            rt2.Set( x, y, w - 20, length );
        } else {
            rt2.Set( x, y, w, height );
        }

        _Scroll = GUI.BeginScrollView( rt, _Scroll, rt2 );

		string type;
        for( int i=0; i<_Objs.Count; i++ ) {
		    rt.Set( x, y + hhGap, w, hhSize );

			if( _Objs[i] is MonoScript ) {
				type = "MonoBehaviour";
			} else if( _Objs[i] is RuntimeAnimatorController ) {
				type = "RuntimeAnimatorController";
			} else {
				type = _Objs[i].GetType().ToString().Replace( "UnityEngine.", "" ).Replace( "UnityEditor.", "" );
			}

            GUI_ObjectField( rt, type, _Objs[i], _Objs[i].GetType(), labelSize );
            y += (hhGap + hhSize + hhGap);
        }

        GUI.EndScrollView();
    }

    void FindResources( Object obj, ref List<Object> objs ) {
        if( obj == null ) {
            return;
        }

		List<System.Type> find_type_list = new List<System.Type>();
		List<System.Type> search_type_list = new List<System.Type>();

		if( obj is GameObject ) {
			find_type_list.Add( typeof(MonoScript) );
			find_type_list.Add( typeof(TextAsset) );
#if NGUI_USED
			UIAtlas atlas = (obj as GameObject).GetComponent<UIAtlas>();
			if( atlas != null ) {
				search_type_list.Add( typeof(UISprite) );
			}
			UIFont font = (obj as GameObject).GetComponent<UIFont>();
            if( font != null ) {
                search_type_list.Add( typeof(UILabel) );
			}
#endif
		} else if( obj is Shader ) {
			find_type_list.Add( typeof(Material) );
			search_type_list.Add( typeof(Renderer) );
		} else if( obj is MonoScript ) {
			find_type_list.Add( typeof(MonoScript) );
			search_type_list.Add( typeof(MonoBehaviour) );
		} else if( obj is Material ) {
			search_type_list.Add( typeof(Renderer) );
#if NGUI_USED
			search_type_list.Add( typeof(UIWidget) );
#endif
		} else if( obj is AnimationClip ) {
			find_type_list.Add( typeof(UnityEditor.Animations.AnimatorController) );
			search_type_list.Add( typeof(Animation) );
			search_type_list.Add( typeof(Animator) );
		} else if( obj is RuntimeAnimatorController ) {
			search_type_list.Add( typeof(Animator) );
		} else if( obj is Font ) {
#if NGUI_USED
			search_type_list.Add( typeof(UILabel) );
#endif
		} else if( obj is Texture2D ) {
			find_type_list.Add( typeof(Material) );
			search_type_list.Add( typeof(Renderer) );
		} else if( obj is AudioClip ) {
			find_type_list.Add( typeof(MonoScript) );
			find_type_list.Add( typeof(TextAsset) );
			search_type_list.Add( typeof(AudioSource) );
		} else if( obj is TextAsset ) {
		}

        for( int i=0; i<_Oris.Count; i++ ) {
			Object ori;
            if( (ori = _Oris[i]) == null ) {
                continue;
            }

			System.Type ori_type = ori.GetType();
			if( find_type_list.Contains( ori_type ) == true ) {
				if( ori is Material ) {
					if( obj is Shader ) {
						if( obj == (ori as Material).shader ) {
							objs.Add( ori );
							continue;
						}
					} else if( obj is Texture2D ) {
						if( obj == (ori as Material).mainTexture ) {
							objs.Add( ori );
							continue;
						}
					}
				} else if( ori is RuntimeAnimatorController ) {
					AnimationClip [] clips;
					RuntimeAnimatorController rac = ori as RuntimeAnimatorController;
					if( (rac != null) && ((clips = rac.animationClips) != null) && (clips.Length > 0) ) {
						for( int l=0; l<clips.Length; l++ ) {
							if( obj == clips[l] ) {
								objs.Add( ori );
								break;
							}
						}
					}
				} else if( (ori is MonoScript) || (ori is TextAsset) ) {
					string path = AssetDatabase.GetAssetPath( obj );
					if( obj is MonoScript ) {
						path = Path.GetFileNameWithoutExtension( path );
					} else {
						DirectoryInfo dir = new DirectoryInfo( path );

						path = path.Remove( path.Length - dir.Extension.Length );

						string finder = "Assets/Resources/";
						if( path.StartsWith( finder ) == true ) {
							path = path.Substring( finder.Length );
						}

						finder = "Units/";
						if( path.StartsWith( finder ) == true ) {
							path = path.Substring( finder.Length );
						}
					}

					if( ori.name.CompareTo( path ) == 0 ) {
						objs.Add( ori );
						continue;
					}

					string text = "";
					if( ori is MonoScript ) {
						text = (ori as MonoScript).text;
					} else if( ori is TextAsset ) {
						text = (ori as TextAsset).text;
					}

					if( text.Contains( path ) == true ) {
						objs.Add( ori );
						continue;
					}
				}
			}

			if( (ori_type != typeof(GameObject)) || (_Kind == PROC_KIND.NOT_GAMEOBJECT) ) {
				continue;
			}

			for( int j=0; j<search_type_list.Count; j++ ) {
				if( objs.Contains( ori ) == true ) {
					break;
				}

				System.Type search_type;
				if( (search_type = search_type_list[j]) == null ) {
					continue;
				}

				_Components.Clear();
				GetComponentsInChildren( _Components, (ori as GameObject).transform, search_type );
				for( int k=0; k<_Components.Count; k++ ) {
					if( objs.Contains( ori ) == true ) {
						break;
					}

					Component comp;
					if( (comp = _Components[k]) == null ) {
						continue;
					}

#if NGUI_USED
					if( search_type == typeof(UISprite) ) {
						UIAtlas atlas = (obj as GameObject).GetComponent<UIAtlas>();
						if( atlas == (comp as UISprite).atlas ) {
							if( _SelectName.Length > 0 ) {
								if(  _SelectName.CompareTo( (comp as UISprite).spriteName ) == 0 ) {
									objs.Add( ori );
									break;
								}
							} else {
								objs.Add( ori );
								break;
							}
						}
					} else if( search_type == typeof(UILabel) ) {
						UILabel label = comp as UILabel;
						if( obj is Font ) {
							if( obj == label.trueTypeFont ) {
								objs.Add( ori );
							}
						} else {
							UIFont font = (obj as GameObject).GetComponent<UIFont>();
							if( (font != null) && (font == label.bitmapFont) ) {
								objs.Add( ori );
							}
						}
					} else if( search_type == typeof(UIWidget) ) {
						UIWidget widget;
						if( ((widget = (comp as UIWidget)) != null) && (obj == widget.material) ) {
							objs.Add( ori );
							break;
						}
					}
#endif

					if( search_type == typeof(Renderer) ) {
						Renderer render = comp as Renderer;

						Material mat;
						Material [] mats;

						if( obj is Shader ) {
							if( ((mats = render.sharedMaterials) != null) && (mats.Length > 0) ) {
								for( int l=0; l<mats.Length; l++ ) {
									if( ((mat = mats[l]) != null) && (obj == mat.shader) ) {
										objs.Add( ori );
										break;
									}
								}
							} else if( ((mat = render.sharedMaterial) != null) && (obj == mat.shader) ) {
								objs.Add( ori );
							}
						} else if( obj is Material ) {
							if( ((mats = render.sharedMaterials) != null) && (mats.Length > 0) ) {
								for( int l=0; l<mats.Length; l++ ) {
									if( ((mat = mats[l]) != null) && (obj == mat) ) {
										objs.Add( ori );
										break;
									}
								}
							} else if( ((mat = render.sharedMaterial) != null) && (obj == mat) ) {
								objs.Add( ori );
							}
						} else if( obj is Texture2D ) {
							if( ((mats = render.sharedMaterials) != null) && (mats.Length > 0) ) {
								for( int l=0; l<mats.Length; l++ ) {
									if( ((mat = mats[l]) != null) && (obj == mat.mainTexture) ) {
										objs.Add( ori );
										break;
									}
								}
							} else if( ((mat = render.sharedMaterial) != null) && (obj == mat.mainTexture) ) {
								objs.Add( ori );
							}
						}
					} else if( search_type == typeof(Animation) ) {
						if( obj is AnimationClip ) {
							foreach( AnimationState state in (Animation)comp ) {
								if( (state != null) && (obj == state.clip) ) {
									objs.Add( ori );
									break;
								}
							}
						}
					} else if( search_type == typeof(Animator) ) {
						if( obj is AnimationClip ) {
							AnimationClip [] clips;
							RuntimeAnimatorController rac = (comp as Animator).runtimeAnimatorController as RuntimeAnimatorController;
							if( (rac != null) && ((clips = rac.animationClips) != null) && (clips.Length > 0) ) {
								for( int l=0; l<clips.Length; l++ ) {
									if( obj == clips[l] ) {
										objs.Add( ori );
										break;
									}
								}
							}
						} else if( obj is RuntimeAnimatorController ) {
							if( obj == (comp as Animator).runtimeAnimatorController ) {
								objs.Add( ori );
							}
						}
					} else if( search_type == typeof(AudioSource) ) {
						AudioSource audio = comp as AudioSource;
						if( obj == audio.clip ) {
							objs.Add( ori );
						}
					} else if( comp is MonoBehaviour ) {
						if( obj.name.CompareTo( comp.GetType().Name ) == 0 ) {
							objs.Add( ori );
						}
					}
				}
			}
		}
    }
}
