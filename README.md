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

---------------------------
*The following is for versions pre 1.2*

The way of ExtendedBehaviour
---------------------------
You can derive your custom behaviour from the ExtendedBehaviour class.

The only thing left to do is add the component attribute to fields or properties and you are ready to go.

![ExtendedBehaviour](http://puu.sh/lmyDs/ebeb03e5ad.png)

The downsides of using this way are (in my opinion)

1. You have to make your custom behaviour inherit from ExtendedBehaviour.
2. If you want to use Awake() you have to override it instead of the standard definition.

The way of this.LoadComponents()
--------------------------------
Call *this.LoadComponents()* In the Start() or Awake() (or wherever you want to put it, really).

The only thing left to do is add the component attribute to fields or properties and you are ready to go.

![Extension](http://puu.sh/lmyB4/2b3e79b708.png)

The downsides of using this way are (in my opinion)

1. You have to call this.LoadComponents, which you might forget.
2. I forget things fast, which makes point one worse.
