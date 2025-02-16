using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ContainerControlPanel.Domain.Models;

/// <summary>
/// Class to represent the metrics output
/// </summary>
public class MetricsRoot
{
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the resource metrics
    /// </summary>
    [JsonPropertyName("resourceMetrics")]
    public List<ResourceMetric> ResourceMetrics { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Class to represent a data point
/// </summary>
public class DataPoint
{
    /// <summary>
    /// Gets or sets the start time in Unix nanoseconds
    /// </summary>
    [JsonPropertyName("startTimeUnixNano")]
    public string StartTimeUnixNano { get; set; }

    /// <summary>
    /// Gets or sets the time in Unix nanoseconds
    /// </summary>
    [JsonPropertyName("timeUnixNano")]
    public string TimeUnixNano { get; set; }

    /// <summary>
    /// Gets or sets the value as an integer
    /// </summary>
    [JsonPropertyName("asInt")]
    public string AsInt { get; set; }

    /// <summary>
    /// Gets or sets the attributes
    /// </summary>
    [JsonPropertyName("attributes")]
    public List<Attribute> Attributes { get; set; }

    /// <summary>
    /// Gets or sets the count
    /// </summary>
    [JsonPropertyName("count")]
    public string Count { get; set; }

    /// <summary>
    /// Gets or sets the sum
    /// </summary>
    [JsonPropertyName("sum")]
    public double Sum { get; set; }

    /// <summary>
    /// Gets or sets the bucket counts
    /// </summary>
    [JsonPropertyName("bucketCounts")]
    public List<string> BucketCounts { get; set; }

    /// <summary>
    /// Gets or sets the explicit bounds
    /// </summary>
    [JsonPropertyName("explicitBounds")]
    public List<double> ExplicitBounds { get; set; }

    /// <summary>
    /// Gets or sets the minimum value
    /// </summary>
    [JsonPropertyName("min")]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value
    /// </summary>
    [JsonPropertyName("max")]
    public double Max { get; set; }
}

/// <summary>
/// Class to represent a histogram
/// </summary>
public class Histogram
{
    /// <summary>
    /// Gets or sets the data points
    /// </summary>
    [JsonPropertyName("dataPoints")]
    public List<DataPoint> DataPoints { get; set; }

    /// <summary>
    /// Gets or sets the aggregation temporality
    /// </summary>
    [JsonPropertyName("aggregationTemporality")]
    public string AggregationTemporality { get; set; }
}

/// <summary>
/// Class to represent a metric
/// </summary>
public class Metric
{
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the unit
    /// </summary>
    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    /// <summary>
    /// Gets or sets the sum
    /// </summary>
    [JsonPropertyName("sum")]
    public Sum Sum { get; set; }

    /// <summary>
    /// Gets or sets the histogram
    /// </summary>
    [JsonPropertyName("histogram")]
    public Histogram Histogram { get; set; }
}

/// <summary>
/// Class to represent a resource metric
/// </summary>
public class ResourceMetric
{
    /// <summary>
    /// Gets or sets the resource
    /// </summary>
    [JsonPropertyName("resource")]
    public Resource Resource { get; set; }

    /// <summary>
    /// Gets or sets the scope metrics
    /// </summary>
    [JsonPropertyName("scopeMetrics")]
    public List<ScopeMetric> ScopeMetrics { get; set; }
}

/// <summary>
/// Class to represent a scope metric
/// </summary>
public class ScopeMetric
{
    /// <summary>
    /// Gets or sets the scope
    /// </summary>
    [JsonPropertyName("scope")]
    public Scope Scope { get; set; }

    /// <summary>
    /// Gets or sets the metrics
    /// </summary>
    [JsonPropertyName("metrics")]
    public List<Metric> Metrics { get; set; }
}

/// <summary>
/// Class to represent a sum
/// </summary>
public class Sum
{
    /// <summary>
    /// Gets or sets the data points
    /// </summary>
    [JsonPropertyName("dataPoints")]
    public List<DataPoint> DataPoints { get; set; }

    /// <summary>
    /// Gets or sets the aggregation temporality
    /// </summary>
    [JsonPropertyName("aggregationTemporality")]
    public string AggregationTemporality { get; set; }
}

/// <summary>
/// Extension methods for the <see cref="MetricsRoot"/> class
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Gets the resources from the metrics
    /// </summary>
    /// <param name="metrics">List of metrics</param>
    /// <returns>Returns the resources</returns>
    public static List<Resource> GetResources(this List<MetricsRoot> metrics)
        => metrics.SelectMany(mr => mr.ResourceMetrics)
                  .Select(rm => rm.Resource)
                  .ToList();

    /// <summary>
    /// Gets the resource names from the resources
    /// </summary>
    /// <param name="resources">List of resources</param>
    /// <returns>Returns the resource names</returns>
    public static List<string> GetResourceNames(this List<Resource> resources)
        => resources.Select(r => r.Attributes?
                    .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue ?? string.Empty)
                    .Distinct()
                    .ToList();

    /// <summary>
    /// Gets the resource name from the resource
    /// </summary>
    /// <param name="resource">Resource object</param>
    /// <returns>Returns the resource name</returns>
    public static string GetResourceName(this Resource resource)
        => resource.Attributes?
            .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue ?? string.Empty;

    /// <summary>
    /// Gets the resource from the metrics root
    /// </summary>
    /// <param name="metricsRoot">Metrics root object</param>
    /// <returns>Returns the resource</returns>
    public static Resource GetResource(this MetricsRoot metricsRoot)
        => metricsRoot.ResourceMetrics
                      .Select(rm => rm.Resource)
                      .FirstOrDefault();

    /// <summary>
    /// Gets the scope metrics by resource
    /// </summary>
    /// <param name="metricsRoot">Metrics root object</param>
    /// <param name="resource">Resource name</param>
    /// <returns>Returns the scope metrics</returns>
    public static List<ScopeMetric> GetScopeMetricsByResource(this MetricsRoot metricsRoot, string resource)
        => metricsRoot.ResourceMetrics
                      .Where(rm => rm.Resource.GetResourceName() == resource)
                      .SelectMany(rm => rm.ScopeMetrics)
                      .ToList();

    /// <summary>
    /// Gets the route name from the metrics root
    /// </summary>
    /// <param name="metricsRoot">Metrics root object</param>
    /// <returns>Returns the route name</returns>
    public static string? GetRouteName(this MetricsRoot metricsRoot)
        => metricsRoot?.ResourceMetrics?
                       .SelectMany(rm => rm.ScopeMetrics ?? Enumerable.Empty<ScopeMetric>())
                       .SelectMany(s => s.Metrics ?? Enumerable.Empty<Metric>())
                       .SelectMany(m => m.Histogram?.DataPoints ?? Enumerable.Empty<DataPoint>())
                       .SelectMany(dp => dp.Attributes ?? Enumerable.Empty<Attribute>())
                       .FirstOrDefault(a => a.Key == "http.route")?.Value?.StringValue;

    /// <summary>
    /// Gets the metrics by name
    /// </summary>
    /// <param name="metricsRoot">Metrics root object</param>
    /// <param name="metricName">Metric name</param>
    /// <returns>Returns the metrics</returns>
    public static List<Metric> GetMetrics(this MetricsRoot metricsRoot, string metricName)
        => metricsRoot?.ResourceMetrics?
                       .SelectMany(rm => rm.ScopeMetrics ?? Enumerable.Empty<ScopeMetric>())
                       .SelectMany(s => s.Metrics ?? Enumerable.Empty<Metric>())
                       .Where(m => m.Name == metricName)
                       .ToList();

    /// <summary>
    /// Gets the data points by metric name
    /// </summary>
    /// <param name="dataPoint">Data point object</param>
    /// <returns>Returns the route name</returns>
    public static string GetRouteName(this DataPoint dataPoint)
        => dataPoint?.Attributes?.FirstOrDefault(a => a.Key == "http.route")?.Value?.StringValue ?? string.Empty;

    /// <summary>
    /// Gets the timestamp of the data point
    /// </summary>
    /// <param name="dataPoint">Data point object</param>
    /// <returns>Returns the timestamp</returns>
    public static string CalculateP50Seconds(this DataPoint dataPoint)
        => CalculatePercentile(dataPoint, 50).ToString("0.000");

    /// <summary>
    /// Gets the timestamp of the data point
    /// </summary>
    /// <param name="dataPoint">Data point object</param>
    /// <returns>Returns the timestamp</returns>
    public static string CalculateP90Seconds(this DataPoint dataPoint)
        => CalculatePercentile(dataPoint, 90).ToString("0.000");

    /// <summary>
    /// Gets the timestamp of the data point
    /// </summary>
    /// <param name="dataPoint">Data point object</param>
    /// <returns>Returns the timestamp</returns>
    public static string CalculateP99Seconds(this DataPoint dataPoint)
        => CalculatePercentile(dataPoint, 99).ToString("0.000");

    /// <summary>
    /// Calculates the percentile
    /// </summary>
    /// <param name="dataPoint">Data point object</param>
    /// <param name="percentile">Percentile value</param>
    /// <returns>Returns the calculated percentile</returns>
    /// <exception cref="ArgumentNullException">Thrown when DataPoint, BucketCounts, or ExplicitBounds is null</exception>
    /// <exception cref="ArgumentException">Thrown when BucketCounts should have one more element than ExplicitBounds</exception>
    private static double CalculatePercentile(DataPoint dataPoint, double percentile)
    {
        if (dataPoint == null || dataPoint.BucketCounts == null || dataPoint.ExplicitBounds == null)
            throw new ArgumentNullException("DataPoint, BucketCounts, or ExplicitBounds cannot be null");

        if (dataPoint.BucketCounts.Count != dataPoint.ExplicitBounds.Count + 1)
            throw new ArgumentException("BucketCounts should have one more element than ExplicitBounds");

        double targetCount = (percentile / 100.0) * double.Parse(dataPoint.Count);
        long cumulativeCount = 0;

        for (int i = 0; i < dataPoint.BucketCounts.Count; i++)
        {
            cumulativeCount += Int64.Parse(dataPoint.BucketCounts[i]);

            if (cumulativeCount >= targetCount)
            {
                if (i == 0)
                {
                    return dataPoint.ExplicitBounds[0];
                }

                double lowerBound = dataPoint.ExplicitBounds[i - 1];
                double upperBound = i < dataPoint.ExplicitBounds.Count ? dataPoint.ExplicitBounds[i] : lowerBound * 2;
                double bucketSize = upperBound - lowerBound;

                double bucketStartCount = cumulativeCount - Int64.Parse(dataPoint.BucketCounts[i]);
                double positionInBucket = targetCount - bucketStartCount;

                return lowerBound + (positionInBucket / Double.Parse(dataPoint.BucketCounts[i])) * bucketSize;
            }
        }

        return TrimToThreeDecimalPlaces(dataPoint.ExplicitBounds.LastOrDefault());
    }

    /// <summary>
    /// Trims the value to three decimal places
    /// </summary>
    /// <param name="value">Value to trim</param>
    /// <returns>Returns the trimmed value</returns>
    private static double TrimToThreeDecimalPlaces(double value)
    {
        string formatted = value.ToString("0.###"); // Ucinanie do trzech miejsc dziesiętnych
        return double.Parse(formatted);
    }
}
