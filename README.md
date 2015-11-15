# Unity3D-ComponentAttribute
An attribute that let's you auto-load components

* Works on fields and properties
* Load components on awake (ExtendedBehaviour only)
* Disable component on error (optional)


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
