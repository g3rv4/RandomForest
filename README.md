RandomForest
===
A simple implementation of a random forest in C#. *Only supporting regression, I'll add classification features soon... or you can just send me a PR :)*

## What is this?
This is a tiny library that knows how to parse [PMML random forests](http://dmg.org/pmml/v4-3/TreeModel.html) and build predictions from them. If you want to train a model, then this library is not for you and you may be looking for something more like [Accord.NET](http://accord-framework.net/).

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

You can use that `randomForest` to make predictions that match the ones you get on R. Right now, you need to pass a `Dictionary<string, double>` to make the prediction. For variables that are integers or doubles, you just set the variable name as the key and the value as the dictionary value. When the variable is a string, you need to concatenate the variable name and the value as the dictionary key and set `1` as the value. So, if you have 2 variables, Age and Name you would pass the following dictionary to get a prediction:

```
var row = new Dictionary<string, double> {
    ["NameGervasio Marchand"] = 1,
	["Age"] = 34
}

double predictedValue = randomForest.Predict(row);
```

And that's it! I'm playing with [protobuf-net](https://github.com/mgravell/protobuf-net) to make it serializable... if I can avoid the dependency, that'd be nice.