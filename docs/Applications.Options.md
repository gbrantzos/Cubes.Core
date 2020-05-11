# Applications Options
Cubes applications can have options based on the 
[Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1), 
which are supported by the Cubes Management UI. This documents describes how we can achieve this functionality.

#### Options class
Application Options are described by simple classes, for example:
```
public class SampleApplicationOptions
{
    public string ConnectionString { get; set; }
    public string Endpoint { get; set; }
    public bool CheckExistence { get; set; }
    public List<string> CheckExistenceExceptions { get; set; }
}
```

To support persistance of options class instance, we must add an attribute describing the path of the file containing 
the serialized instance:

```
[ConfigurationStore("< path of the file, must be a constant value >")]
```

Currently YAML and JSON files are supported. Note that all parameters of an attribute must be known at compile time, 
for example string should be const values.

Usually the options object is not suitable for UI representation. To enable conversion to and from an appropriate view 
model needed by the Cubes Management, a ViewModelConverter class should be provided:

```
public class SampleApplicationOptionsViewModelConverter : ViewModelConverter
{
    public override object FromViewModel(object viewModel)
    {
        var toReturn = new SampleApplicationOptions();
        dynamic temp = viewModel;

        toReturn.ConnectionString = temp.Basic.ConnectionString;
        toReturn.Endpoint         = temp.Basic.Endpoint
        toReturn.CheckExistence   = temp.Basic.CheckExistence;

        string tempValue = temp.Basic.CheckExistenceExceptions;
        toReturn.CheckExistenceExceptions = tempValue
            .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.StartsWith("#"))
            .SelectMany(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(i => i.Trim())
            .Where(i => !String.IsNullOrEmpty(i))
            .ToList();

        // Other conversions take place
        // ...

        return toReturn;
    }

    public override object ToViewModel(object configurationInstance)
    {
        var config = configurationInstance as SampleApplicationOptions;
        if (config is null)
            throw new ArgumentException($"Could not cast to {nameof(SampleApplicationOptions)}");

        return new
        {
            Basic = new
            {
                config.ConnectionString,
                config.Endpoint,
                config.CheckExistence,
                CheckExistenceExceptions = String.Join(", ", config.CheckEofExistenceExceptions.ToArray())
            },
            config.Users
        };
    }
}
```

A converter for an option class is defined using the `ViewModelConverter` attribute:
```
[ViewModelConverter(typeof(SampleApplicationOptionsViewModelConverter))]
```

The complete options class should look like:
```
[ConfigurationStore(SampleApplication.OptionsFile)]
[ViewModelConverter(typeof(SampleApplicationOptionsViewModelConverter))]
public class SampleApplicationOptions
{
    public string ConnectionString { get; set; }
    public string Endpoint { get; set; }
    public bool CheckExistence { get; set; }
    public List<string> CheckExistenceExceptions { get; set; }
}
``` 

Example classes can be found on `Cubes.Core` project, inside `Base\Samples` folder.

#### View model converter
A view model converter provides conversion from Options class to a UI view model object and backwards. Converter 
classes derive from `ViewModelConverter` class and implement methods From and To:
```
public abstract class ViewModelConverter
{
    public abstract object ToViewModel(object configurationInstance);
    public abstract object FromViewModel(object viewModel);
}
```
Example class can be found on `Cubes.Core` project, inside `Base\Samples` folder.

#### Cubes management UI Config
To 'glue' all together, an application class must override method `GetUISettings` providing details about what to show 
and how data should be communicated:

```
public class SampleApplication : Application
{
    // Code snipped ...

    public override IEnumerable<ApplicationOptionsUIConfig> GetUISettings()
    {
        return base
            .GetUISettings()
            .Append(new ApplicationOptionsUIConfig
            {
                DisplayName     = "Sample Application",
                OptionsTypeName = "Cubes.Core.Base.Samples.SampleApplicationOptions",
                UISchema        = SampleApplicationOptionsSchema.GetSchema(),
                AssemblyName    = this.GetType().Assembly.GetName().Name,
                AssemblyPath    = this.GetType().Assembly.Location
            });
    }

    // Code snipped ...
}
```

A basic element of an `ApplicationOptionsUIConfig` object, is the schema that should be used to display options. The 
property `UISchema` is a `ComplexSchema` instance that can define multiple sections (converted to tabs on the UI) 
of information.

Example class can be found on `Cubes.Core` project, inside `Base\Samples` folder.
