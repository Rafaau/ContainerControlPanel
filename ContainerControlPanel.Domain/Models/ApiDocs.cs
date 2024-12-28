namespace ContainerControlPanel.Domain.Models;

public class EndpointInfo
{
    public string ControllerName { get; set; }
    public string ActionName { get; set; }
    public string HttpMethod { get; set; }
    public string Route { get; set; }
    public Type ReturnType { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new();
}

public class ParameterInfo
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public string Source { get; set; }
}

public class Type
{
    public string Name { get; set; }
    public Structure Structure { get; set; }
}

public class Structure
{
    public string Type { get; set; }
    public List<Property> Properties { get; set; } = new();
    public List<GenericArgument> GenericArguments { get; set; } = new();
}

public class GenericArgument
{
    public string Type { get; set; }
    public List<Property> Properties { get; set; } = new();
}

public class Property
{
    public string Name { get; set; }
    public string PropertyType { get; set; }
    public Structure Structure { get; set; }
}

public class ControllerView
{
    public string Name { get; set; }
    public List<ActionView> Actions { get; set; } = new();
}

public class ActionView
{
    public string Name { get; set; }
    public string HttpMethod { get; set; }
    public string Route { get; set; }
    public Type ReturnType { get; set; }
    public List<ParameterView> Parameters { get; set; } = new();
    public string RequestBodyFormatted { get; set; } = string.Empty;
    public string ResponseBodyFormatted { get; set; } = string.Empty;
}

public class ParameterView
{
    public string Name { get; set; }
    public Type Type { get; set; }
    public string Source { get; set; }
}

public static class ApiDocsExtensions
{
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
                    }).ToList()
                };
                controllerView.Actions.Add(actionView);
            }
            controllerViews.Add(controllerView);
        }
        return controllerViews;
    }

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

    private static string GetGenericArgumentJson(GenericArgument genericArgument)
    {
        string body = string.Empty;

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
    private static bool IsPrimitiveType(string type)
    {
        var primitiveTypes = new HashSet<string> { "String", "Int32", "Double", "Boolean", "DateTime", "DateOnly" };
        return primitiveTypes.Contains(type);
    }
}