using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

public class MetricsRoot
{
    [JsonPropertyName("resourceMetrics")]
    public List<ResourceMetric> ResourceMetrics { get; set; }
}

public class DataPoint
{
    [JsonPropertyName("startTimeUnixNano")]
    public string StartTimeUnixNano { get; set; }

    [JsonPropertyName("timeUnixNano")]
    public string TimeUnixNano { get; set; }

    [JsonPropertyName("asInt")]
    public string AsInt { get; set; }

    [JsonPropertyName("attributes")]
    public List<Attribute> Attributes { get; set; }

    [JsonPropertyName("count")]
    public string Count { get; set; }

    [JsonPropertyName("sum")]
    public double Sum { get; set; }

    [JsonPropertyName("bucketCounts")]
    public List<string> BucketCounts { get; set; }

    [JsonPropertyName("explicitBounds")]
    public List<double> ExplicitBounds { get; set; }

    [JsonPropertyName("min")]
    public double Min { get; set; }

    [JsonPropertyName("max")]
    public double Max { get; set; }
}

public class Histogram
{
    [JsonPropertyName("dataPoints")]
    public List<DataPoint> DataPoints { get; set; }

    [JsonPropertyName("aggregationTemporality")]
    public string AggregationTemporality { get; set; }
}

public class Metric
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    [JsonPropertyName("sum")]
    public Sum Sum { get; set; }

    [JsonPropertyName("histogram")]
    public Histogram Histogram { get; set; }
}

public class ResourceMetric
{
    [JsonPropertyName("resource")]
    public Resource Resource { get; set; }

    [JsonPropertyName("scopeMetrics")]
    public List<ScopeMetric> ScopeMetrics { get; set; }
}

public class ScopeMetric
{
    [JsonPropertyName("scope")]
    public Scope Scope { get; set; }

    [JsonPropertyName("metrics")]
    public List<Metric> Metrics { get; set; }
}

public class Sum
{
    [JsonPropertyName("dataPoints")]
    public List<DataPoint> DataPoints { get; set; }

    [JsonPropertyName("aggregationTemporality")]
    public string AggregationTemporality { get; set; }
}

public static class MetricsExtensions
{
    public static List<Resource> GetResources(this List<MetricsRoot> metrics)
        => metrics.SelectMany(mr => mr.ResourceMetrics)
                  .Select(rm => rm.Resource)
                  .ToList();

    public static string GetResourceName(this Resource resource)
        => resource.Attributes?
            .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue ?? string.Empty;

    public static Resource GetResource(this MetricsRoot metricsRoot)
        => metricsRoot.ResourceMetrics
                      .Select(rm => rm.Resource)
                      .FirstOrDefault();

    public static List<ScopeMetric> GetMetricsScopes(this List<MetricsRoot> metricsRoots, string resource)
        => metricsRoots.SelectMany(mr => mr.ResourceMetrics)
                       .Where(rm => rm.Resource.Attributes
                           .Any(a => a.Key == "service.name" && a.Value.StringValue == resource))
                       .SelectMany(rm => rm.ScopeMetrics)
                       .Distinct()
                       .ToList();

    public static List<string> GetInstruments(this MetricsRoot metricsRoot, string metricScope)
        => metricsRoot.ResourceMetrics
                      .SelectMany(rs => rs.ScopeMetrics)
                      .Where(s => s.Scope?.Name == metricScope)
                      .SelectMany(s => s.Metrics)
                      .Select(m => m.Name)
                      .Where(metricName => !string.IsNullOrEmpty(metricName))
                      .Distinct()
                      .ToList();

    public static List<Metric> GetMetrics(this MetricsRoot metricsRoot, string metricName)
        => metricsRoot.ResourceMetrics
                      .SelectMany(rs => rs.ScopeMetrics)
                      .SelectMany(s => s.Metrics)
                      .Where(m => m.Name == metricName)
                      .ToList();

    public static List<Metric> GetMetricsWithRoute(this List<Metric> metrics)
        => metrics.Where(m => m.Histogram.DataPoints
                     .Any(dp => dp.Attributes
                        .Any(a => a.Key == "http.route")))
                  .ToList();

    public static List<DataPoint> GetDataPointsWithRoute(this List<Metric> metrics)
        => metrics.SelectMany(m => m.Histogram.DataPoints)
                  .Where(dp => dp.Attributes
                     .Any(a => a.Key == "http.route"))
                  .ToList();
}
