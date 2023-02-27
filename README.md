# Bona Preview Field
![alt Preview Field](http://gamedevelopersplayground.com/files/PreviewFieldCoverImage.png)
Bona Preview Field is a Unity3D editor extension, to allow for Prefab previews when selecting models etc. 
When creating a public Prefab field in a MonoBehaviour or ScriptableObject  by adding the  ``[PreviewField]``
attribute (available in the ``Fyrvall.PreviewObjectPicker`` namespace).

## Getting started
First you need a cope of the software. 

### Unity Package
The editor extension can be added Unity's package manager from 'Add package from git URL'
* <https://github.com/bonahona/previewfield.git>

### Manual download
It can be downloaded from the following sources.
You need to put the PreviewField content inside your Unity project's Asset folder.
* <https://github.com/bonahona/previewfield.git>

## Example
Decorate a public property in a  with the attribute. By default the class's name will be displayed.
```cs
using using Fyrvall.PreviewObjectPicker;

class TestClass: ScriptableObject {
	[PreviewField]
	public GameObject SomeTestPrefab;
}
```
or
```cs
public class TestClass: ScriptableObject {
	[Fyrvall.PreviewObjectPicker.PreviewField]
	public GameObject SomeTestPrefab;
}
```
This also works for anything `Component` based scripts.
```cs
using using Fyrvall.PreviewObjectPicker;

public class TestClass: ScriptableObject {
	[PreviewField]
	public Collider ColliderPrefab;
}
```
or a custom `MonoBehaviour`
```cs
using using Fyrvall.PreviewObjectPicker;

public class TestComponent : MonoBehaviour { }

public class TestClass: ScriptableObject {
	[PreviewField]
	public TestComponent TestComponentPrefab;
}
```

## License
This project is released as Open Source under a [MIT license](https://opensource.org/licenses/MIT).