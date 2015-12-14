# Unity3D-ComponentAttribute
An attribute that let's you auto-load components

* Works on (public and non-public) fields and properties
* Disable component on error (optional)

*Credits go to [@ChevyRay](https://twitter.com/ChevyRay) for inspiring me to make this by this [tweet](https://twitter.com/ChevyRay/status/665673463856664576)*

How-to
---------------------------
It's simple, really. All you need to do is add the [Component] attribute to any field or property in your class and it is good to go.

You have three/five options on how to use the component attribute:
* [Component] which tries to find the component and then set the field/property
* [Component(false,false)] which does the same as the one above
* [Component(true, false)] which adds the component to the GameObject if it is missing and then set the field/property
* [Component(true, true)] which does the same as the one above
* [Component(false,true)] which, if the component is not on the GameObject, will disable that component 

![ComponentAttribute](http://puu.sh/lVE7W/88eeca4d60.png)
