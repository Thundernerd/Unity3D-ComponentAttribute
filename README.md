# Unity3D-ComponentAttribute
An attribute that let's you auto-load components

* Works on fields and properties
* Load components on awake, start, or enable
* Disable component on error (optional)

*Credits go to [@ChevyRay](https://twitter.com/ChevyRay) for inspiring me to make this by this [tweet](https://twitter.com/ChevyRay/status/665673463856664576)*


The way of this.LoadComponents()
--------------------------------
Call *this.LoadComponents()* In the Start() or Awake() (or wherever you want to put it, really).

The only thing left to do is add the component attribute to fields or properties and you are ready to go.

![Extension](http://puu.sh/lmyB4/2b3e79b708.png)

The downsides of using this way are (in my opinion)

1. You have to call this.LoadComponents, which you might forget.
2. I forget things fast, which makes point one worse.