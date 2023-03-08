# Preview Field
![alt Preview Field splash](https://repository-images.githubusercontent.com/568438140/f2f76207-f9cd-42f9-9ae5-fa4684931b36)
When selecting a prefab, get a complete list of available prefabs with an interactive preview.
Perfect for anyone looking for a more artist-friendly workflow.

Preview Field is a Unity3D editor extension, to allow for Prefab previews when selecting prefabs, models etc. 
When creating a public object (Prefab) field in a MonoBehaviour or ScriptableObject, by adding the  ``[PreviewField]``
attribute (available in the ``CollisionBear.PreviewObjectPicker`` namespace) the property
will look slightly different. To the far right is a new Prefab picker button, opening the preview
object selector.

## Getting started
First you need to get your hands on a copy of the editor. We support a few options. 

### Unity Package
The editor extension can be added Unity's package manager from 'Add package from git URL'
* <https://github.com/collisionbear/previewfield.git>

### Manual download
It can be downloaded from the following sources.
* <https://github.com/CollisionBear/PreviewField/releases/download/1.2.0/PreviewField-1.2.0.unitypackage>
You need to put the PreviewField content inside your Unity project's Asset folder.

## Example
Decorate a public property with the attribute.
```cs
using using CollisionBear.PreviewObjectPicker;

class TestClass: ScriptableObject {
	[PreviewField]
	public GameObject SomeTestPrefab;
}
```
or
```cs
public class TestClass: ScriptableObject {
	[CollisionBear.PreviewObjectPicker.PreviewField]
	public GameObject SomeTestPrefab;
}
```
This also works for any `Component` based scripts.
```cs
using using CollisionBear.PreviewObjectPicker;

public class TestClass: ScriptableObject {
	[PreviewField]
	public Collider ColliderPrefab;
}
```
or a custom `MonoBehaviour`
```cs
using using CollisionBear.PreviewObjectPicker;

public class TestComponent : MonoBehaviour { }

public class TestClass: ScriptableObject {
	[PreviewField]
	public TestComponent TestComponentPrefab;
}
```

## License
This project is released as Open Source under a [MIT license](https://opensource.org/licenses/MIT).
