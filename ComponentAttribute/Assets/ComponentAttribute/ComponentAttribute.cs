using System;

[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
sealed class ComponentAttribute : Attribute {

    public readonly bool DisableComponentOnError;

    public ComponentAttribute() {
        DisableComponentOnError = false;
    }

    public ComponentAttribute( bool disableComponentOnError ) {
        DisableComponentOnError = disableComponentOnError;
    }

}