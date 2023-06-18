# ResourceTable

The abstract class `ResourceTable` models a resource table, a mapping where
keys are strings and values are objects of any type. This class is useful as 
a base class for classes defining locale-specific resources, isolating data
objects from program code. Its static methods search the current assembly for 
classes derived from `ResourceTable` and load the ones appropriated for the 
given or the current locale.

A resource table is loaded by its owner when needed. The owner should keep a
reference to the table, in order to avoid performance penalties resulting from
repeated calls of `GetTable(...)` when searching for resources.

The search for a resource table starts in the namespace of the calling method.
For example, when searching for a `Type` named `Resources.Messages`, calling 
`GetTable(...)` from a method of an object of the namespace `Greeter.UI`, the
method searches for a `Type` with the name `Greeter.UI.Resources.Messages`. If
such a `Type` exists, an instance is created as a `ResourceTable`. Then, the
culture's name (obtained with `culture.Name`) is split by '-' then for each
part a new name is obtained by appending '_' and the part to the previous one.
For example, given the names above and the local culture `fr-FR`, the names
obtained are `Greeter.UI.Resources.Messages_fr` and 
`Greeter.UI.Resources.Messages_fr_FR`. For each of these names, the method
searches the assembly for a `Type` derived from `ResourceTable`. If such a
`Type` exists, an instance is created as a `ResourceTable` and its `Parent` is
set to the previously created `ResourceTable` object if one exists.

The search process returns the last `ResourceTable` object to the caller. The
caller must keep a reference to it, in order to avoid performance penalties
resulting from repeated searches.