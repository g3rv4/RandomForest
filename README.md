RandomForest
===
A simple implementation of a random forest in C#. *Only supporting regression, I'll add classification features soon... or you can just send me a PR :)*

## What is this?
This is a tiny library that knows how to parse [PMML random forests](http://dmg.org/pmml/v4-3/TreeModel.html) and build predictions from them. If you want to train a model, then this library is not for you and you may be looking for something more like [Accord.NET](http://accord-framework.net/).

## How can I install it?
On the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/tools/package-manager-console), run

    Install-Package RandomForest
    
## Can I see an example?
I'm glad you asked. I built [a sample project using it](https://github.com/g3rv4/RandomForest.Sample).

## What use case does this solve?
You use [R](https://www.r-project.org/) to build and tune your models. You may be using [caret](http://caret.r-forge.r-project.org/) to help you build and choose the one you like the most... but then, you want to productionize the model in C#.

In your R code, you can do:

```
library(r2pmml)
r2pmml(my_model, "my_model.pmml")
```

And that will generate a `my_model.pmml` that you can feed this library using:

```
Forest randomForest;
using (XmlReader reader = XmlReader.Create("my_model.pmml"))
{
    randomForest = new Forest(reader);
}
```

You can use that `randomForest` to make predictions that match the ones you get on R.

```
var row = new Dictionary<string, string> {
    ["Name"] = "Gervasio Marchand",
    ["Age"] = "34"
}

double predictedValue = randomForest.Predict(row);
```

A more efficient alternative (because it doesn't need to parse the string to double) receives a `Dictionary<string, double>`. The key of the dictionary should be the variable name and the value when the variable type is not a double. Here's an example:

```
var row = new Dictionary<string, double> {
    ["NameGervasio Marchand"] = 1,
    ["Age"] = 34
}

double predictedValue = randomForest.Predict(row);
```

And that's it! I'm playing with [protobuf-net](https://github.com/mgravell/protobuf-net) to make it serializable... if I can avoid the dependency, that'd be nice.