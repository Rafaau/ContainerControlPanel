namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to store information about the endpoints in the API
/// </summary>
public class EndpointInfo
{
    /// <summary>
    /// Gets or sets the name of the controller
    /// </summary>
    public string ControllerName { get; set; }

    /// <summary>
    /// Gets or sets the name of the action
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the route of the action
    /// </summary>
    public string Route { get; set; }

    /// <summary>
    /// Gets or sets the return type of the action
    /// </summary>
    public Type ReturnType { get; set; }

    /// <summary>
    /// Gets or sets the parameters of the action
    /// </summary>
    public List<ParameterInfo> Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the summary of the action
    /// </summary>
    public string Summary { get; set; }
}

/// <summary>
/// Class to store information about the parameters in the API
/// </summary>
public class ParameterInfo
{
    /// <summary>
    /// Gets or sets the name of the parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the parameter
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the source of the parameter
    /// </summary>
    public string Source { get; set; }
}

/// <summary>
/// Class to store information about the types in the API
/// </summary>
public class Type
{
    /// <summary>
    /// Gets or sets the name of the type
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the structure of the type
    /// </summary>
    public Structure Structure { get; set; }
}

/// <summary>
/// Class to store information about the structure of the types in the API
/// </summary>
public class Structure
{
    /// <summary>
    /// Gets or sets the type name of the structure
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the properties of the structure
    /// </summary>
    public List<Property> Properties { get; set; } = new();

    /// <summary>
    /// Gets or sets the generic arguments of the structure
    /// </summary>
    public List<GenericArgument> GenericArguments { get; set; } = new();
}

/// <summary>
/// Class to store information about the generic arguments in the API
/// </summary>
public class GenericArgument
{
    /// <summary>
    /// Gets or sets the type name of the generic argument
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the properties of the generic argument
    /// </summary>
    public List<Property> Properties { get; set; } = new();
}

/// <summary>
/// Class to store information about the properties in the API
/// </summary>
public class Property
{
    /// <summary>
    /// Gets or sets the name of the property
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the property
    /// </summary>
    public string PropertyType { get; set; }

    /// <summary>
    /// Gets or sets the structure of the property
    /// </summary>
    public Structure Structure { get; set; }
}

/// <summary>
/// Controller view model for the API documentation
/// </summary>
public class ControllerView
{
    /// <summary>
    /// Gets or sets the name of the controller
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the actions of the controller
    /// </summary>
    public List<ActionView> Actions { get; set; } = new();
}

/// <summary>
/// Action view model for the API documentation
/// </summary>
public class ActionView
{
    /// <summary>
    /// Gets or sets the name of the action
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the HTTP method of the action
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// Gets or sets the route of the action
    /// </summary>
    public string Route { get; set; }

    /// <summary>
    /// Gets or sets the return type of the action
    /// </summary>
    public Type ReturnType { get; set; }

    /// <summary>
    /// Gets or sets the parameters of the action
    /// </summary>
    public List<ParameterView> Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the summary of the action
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Gets or sets the formatted request body
    /// </summary>
    public string RequestBodyFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted response body
    /// </summary>
    public string ResponseBodyFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the indicator if the action is to be tested
    /// </summary>
    public bool TryOut { get; set; } = false;

    /// <summary>
    /// Gets or sets the indicator if the action is during execution
    /// </summary>
    public bool Loading { get; set; } = false;

    /// <summary>
    /// Gets or sets the test request body
    /// </summary>
    public string TestRequestBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test response body
    /// </summary>
    public string TestResponseBody { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test response headers
    /// </summary>
    public string TestResponseHeaders { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test response status code
    /// </summary>
    public string TestResponseStatusCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test response status description
    /// </summary>
    public string TestResponseStatusDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the test request curl
    /// </summary>
    public string TestRequestCurl { get; set; } = string.Empty;
}

/// <summary>
/// Parameter view model for the API documentation
/// </summary>
public class ParameterView
{
    /// <summary>
    /// Gets or sets the name of the parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the parameter
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Gets or sets the source of the parameter
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the test value of the parameter
    /// </summary>
    public string TestValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the indicator if the parameter has an error
    /// </summary>
    public bool Error { get; set; } = false;
}

/// <summary>
/// Extension methods for the API documentation
/// </summary>
public static class ApiDocsExtensions
{
    /// <summary>
    /// Maps the data from the endpoints to the controller view model
    /// </summary>
    /// <param name="endpoints">List of <see cref="EndpointInfo"/> objects</param>
    /// <returns>Returns a list of <see cref="ControllerView"/> objects</returns>
    public static List<ControllerView> GetControllers(this List<EndpointInfo> endpoints)
    {
        var controllers = endpoints.GroupBy(x => x.ControllerName).Select(x => x.Key).ToList();
        var controllerViews = new List<ControllerView>();
        foreach (var controller in controllers)
        {
            var controllerView = new ControllerView
            {
                Name = controller,
                Actions = new List<ActionView>()
            };
            var actions = endpoints.Where(x => x.ControllerName == controller).ToList();
            foreach (var action in actions)
            {
                var actionView = new ActionView
                {
                    Name = action.ActionName,
                    HttpMethod = action.HttpMethod,
                    Route = action.Route,
                    ReturnType = action.ReturnType,
                    Parameters = action.Parameters.Select(x => new ParameterView
                    {
                        Name = x.Name,
                        Type = x.Type,
                        Source = x.Source
                    }).ToList(),
                    Summary = action.Summary
                };
                controllerView.Actions.Add(actionView);
            }
            controllerViews.Add(controllerView);
        }
        return controllerViews;
    }

    /// <summary>
    /// Gets the formatted request body JSON
    /// </summary>
    /// <param name="type">Type object</param>
    /// <returns>Returns the formatted request body JSON</returns>
    public static string GetRequestBodyJson(this Type type)
    {
        string body = string.Empty;

        foreach (var field in type.Structure.Properties)
        {
            if (field.Structure != null && !IsPrimitiveType(field.Structure.Type))
            {
                body += $"\"{field.Name}\": {GetRequestBodyJson(new Type { Structure = field.Structure })},";
            }
            else
            {
                body += $"\"{field.Name}\": {GetExampleValue(field.PropertyType)},";
            }
        }

        return $"{{{body.TrimEnd(',')}}}";
    }

    /// <summary>
    /// Gets the formatted response body JSON
    /// </summary>
    /// <param name="returnType">Return type object</param>
    /// <returns>Returns the formatted response body JSON</returns>
    public static string GetResponseBodyJson(this Type returnType)
    {
        if (returnType.Structure == null)
        {
            return "{}";
        }

        if (returnType.Structure.GenericArguments != null && returnType.Structure.GenericArguments.Any())
        {
            var genericArgument = returnType.Structure.GenericArguments.First();
            return $"[{GetGenericArgumentJson(genericArgument)}]";
        }

        return GetStructureJson(returnType.Structure);
    }

    /// <summary>
    /// Gets the formatted JSON for the generic argument
    /// </summary>
    /// <param name="genericArgument">Generic argument object</param>
    /// <returns>Returns the formatted JSON for the generic argument</returns>
    private static string GetGenericArgumentJson(GenericArgument genericArgument)
    {
        string body = string.Empty;

        if (genericArgument.Properties == null || !genericArgument.Properties.Any())
        {
            return $"\"{genericArgument.Type.ToLower()}\"";
        }

        foreach (var property in genericArgument.Properties)
        {
            if (property.Structure != null && !IsPrimitiveType(property.Structure.Type))
            {
                body += $"\"{property.Name}\": {GetStructureJson(property.Structure)},";
            }
            else
            {
                body += $"\"{property.Name}\": {GetExampleValue(property.PropertyType)},";
            }
        }

        return $"{{{body.TrimEnd(',')}}}";
    }

    /// <summary>
    /// Gets the formatted JSON for the structure
    /// </summary>
    /// <param name="structure">Structure object</param>
    /// <returns>Returns the formatted JSON for the structure</returns>
    private static string GetStructureJson(Structure structure)
    {
        string body = string.Empty;

        foreach (var property in structure.Properties)
        {
            if (property.Structure != null && !IsPrimitiveType(property.Structure.Type))
            {
                body += $"\"{property.Name}\": {GetStructureJson(property.Structure)},";
            }
            else
            {
                body += $"\"{property.Name}\": {GetExampleValue(property.PropertyType)},";
            }
        }

        return $"{{{body.TrimEnd(',')}}}";
    }

    /// <summary>
    /// Gets the example value for the property type
    /// </summary>
    /// <param name="propertyType">Property type</param>
    /// <returns>Returns the example value for the property type</returns>
    private static string GetExampleValue(string propertyType)
    {
        return propertyType switch
        {
            "String" => "\"string\"",
            "Int32" => "0",
            "Double" => "0.00",
            "Boolean" => "true",
            "DateTime" => $"\"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}\"",
            "DateOnly" => $"\"{DateTime.Now:yyyy-MM-dd}\"",
            _ => "null"
        };
    }

    /// <summary>
    /// Checks if the type is a primitive type
    /// </summary>
    /// <param name="type">Type name</param>
    /// <returns>Returns true if the type is a primitive type</returns>
    public static bool IsPrimitiveType(this string type)
    {
        var primitiveTypes = new HashSet<string> { "String", "Int32", "Double", "Boolean", "DateTime", "DateOnly" };
        return primitiveTypes.Contains(type);
    }
}