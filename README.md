# Bona Preview Field
Bona Preview Field is a Unity3D editor extension, to allow for Prefab previews when selecting models etc. 
When creating a public Prefab field in a MonoBehaviour or ScriptableObject  by adding the  ``[PreviewField]``
attribute (available in the ``Fyrvall.PreviewObjectPicker`` namespace).

## Getting started
First you need a cope of the software. It can be downloaded from the following sources.
* <https://github.com/bonahona/previewfield.git>

If grabbed from Unity's Asset Store, everything should be set up already.
If you grabbed from any other source, you need to put the PreviewField inside your Unity project's Asset folder.

## Example
Decorate a public property in a  with the attribute. By default the class's name will be displayed.
```cs
using using Fyrvall.PreviewObjectPicker;

class TestClass: ScriptableObject {
	[PreviewField(typeof(GameObject))]
	public GameObject SomeTestPrefab;
}
```
or
```cs
  class TestClass: ScriptableObject {
	[Fyrvall.PreviewObjectPicker.PreviewField(typeof(GameObject))]
	public GameObject SomeTestPrefab;
}
  ```

## License
This project is released as Open Source under a [MIT license](https://opensource.org/licenses/MIT).