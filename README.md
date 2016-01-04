# Unity3D-ComponentAttribute
An attribute that let's you auto-load components

* Works on fields and properties
* Load components on awake, start, or enable, or whenever else you feel like it
* Get component from other GameObjects (optional)
* Add component if missing (optional)
* Disable component on error (optional)

*Credits go to [@ChevyRay](https://twitter.com/ChevyRay) for inspiring me to make this by this [tweet](https://twitter.com/ChevyRay/status/665673463856664576)*

The way of this.LoadComponents()
-------------------------------
The only thing you have to do is add the component attribute to fields and/or properties that you want and call *this.LoadComponents();* in the awake, start, on enable, or any other place for that matter and you are good to go.

Pros:
* You don't have to do all those GetComponent calls to get the components you need
* The more components you want, the less you have to do

Cons:
* You have to write *[Component]* on top the of the Components you want this to work on
* You also have to call *this.LoadComponents();*

### Fields Example
![Imgur](http://i.imgur.com/BSnZNWt.png)

### Properties Example
![Imgur](http://i.imgur.com/P3HrXcB.png)
