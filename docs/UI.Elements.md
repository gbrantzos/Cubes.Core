# Commands
Cubes defines a number of UI concepts to provide easy and declarative design of UI forms.

#### Lookup
A lookup is a display/value pair used to describe lists of values. The building blocks of this concepts are:
- `Lookup` is a class that holds the actual data. A cacheable Lookup does not change during Cubes server lifetime.
- `LookupProvider` is a class that creates a `Lookup`. Lookup providers are registered on the DI. Providers should 
define a unique, descriptive name.
- `LookupManager` is a helper class (singleton) that can be used to retrieve a `LookupProvider` registered on the 
DI. The provider is located by name
Lookups and providers can be used independently of the `LookupManager`. Basic Cubes providers names can be found on 
the static class `CoreProviders`.

#### Schema
Schema elements help us declare a UI form. We can use Schema functionality using the following classes:
- `Schema` class holds the definition of a form. A schema contains items that correspond to input elements.
- `SchemaProvider` are used to produce instances of schemas. Schema providers are registered on the DI. Providers should 
define a unique, descriptive name.
- `SchemaManager` is a helper class (singleton) that can be used to retrieve a named `SchemaProvider`
Schemas and providers can be used independently of the `LookupManager`.

#### Complex Schema
When a simple form is not enough we can use a `ComplexSchema` instance. The complex schema is a collection of 
sections, each one being a simple `Schema` object or a list of items used to provide UI to a collection property.