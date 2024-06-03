# Preview Field
![alt Preview Field splash](https://repository-images.githubusercontent.com/568438140/f2f76207-f9cd-42f9-9ae5-fa4684931b36)
Enhance your workflow with Preview Field, the a Unity3D editor extension for effecient Prefab selection. Perfect for developers and artists alike, Preview Field improves
the way you choose Prefabs by replacing Unity's default selection field with an interactive alternative.

Preview Field offers an intuitive interface that anyone familiar with Prefabs will instantly recognize and appreciate.

## Getting started
First you need to get your hands on a copy of the editor. We support a few options. 

### Unity Package
The editor extension can be added Unity's package manager from 'Add package from git URL'
* <https://github.com/collisionbear/previewfield.git>

### Manual download
It can be downloaded from the following sources.
* <https://github.com/CollisionBear/PreviewField/releases/download/1.3.0/PreviewField-1.3.0.unitypackage>
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
