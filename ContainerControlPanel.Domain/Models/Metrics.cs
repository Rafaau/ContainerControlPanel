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

    public static List<string> GetResourceNames(this List<Resource> resources)
        => resources.Select(r => r.Attributes?
                    .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue ?? string.Empty)
                    .Distinct()
                    .ToList();

    public static string GetResourceName(this Resource resource)
        => resource.Attributes?
            .FirstOrDefault(a => a.Key == "service.name")?.Value?.StringValue ?? string.Empty;

    public static Resource GetResource(this MetricsRoot metricsRoot)
        => metricsRoot.ResourceMetrics
                      .Select(rm => rm.Resource)
                      .FirstOrDefault();

    public static List<ScopeMetric> GetScopeMetricsByResource(this List<MetricsRoot> metricsRoots, string resource)
        => metricsRoots.SelectMany(mr => mr.ResourceMetrics)
                       .Where(rm => rm.Resource.Attributes
                           .Any(a => a.Key == "service.name" && a.Value.StringValue == resource))
                       .SelectMany(rm => rm.ScopeMetrics)
                       .Distinct()
                       .ToList();

    public static string? GetRouteName(this MetricsRoot metricsRoot)
        => metricsRoot?.ResourceMetrics?
                       .SelectMany(rm => rm.ScopeMetrics ?? Enumerable.Empty<ScopeMetric>())
                       .SelectMany(s => s.Metrics ?? Enumerable.Empty<Metric>())
                       .SelectMany(m => m.Histogram?.DataPoints ?? Enumerable.Empty<DataPoint>())
                       .SelectMany(dp => dp.Attributes ?? Enumerable.Empty<Attribute>())
                       .FirstOrDefault(a => a.Key == "http.route")?.Value?.StringValue;

    public static List<Metric> GetMetrics(this MetricsRoot metricsRoot, string metricName)
        => metricsRoot?.ResourceMetrics?
                       .SelectMany(rm => rm.ScopeMetrics ?? Enumerable.Empty<ScopeMetric>())
                       .SelectMany(s => s.Metrics ?? Enumerable.Empty<Metric>())
                       .Where(m => m.Name == metricName)
                       .ToList();

    public static string GetRouteName(this DataPoint dataPoint)
        => dataPoint.Attributes?.FirstOrDefault(a => a.Key == "http.route")?.Value?.StringValue ?? string.Empty;

    public static string CalculateP50Seconds(this DataPoint dataPoint)
        => CalculatePercentile(dataPoint, 50).ToString("0.000");

    public static string CalculateP90Seconds(this DataPoint dataPoint)
        => CalculatePercentile(dataPoint, 90).ToString("0.000");

    public static string CalculateP99Seconds(this DataPoint dataPoint)
        => CalculatePercentile(dataPoint, 99).ToString("0.000");

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

    private static double TrimToThreeDecimalPlaces(double value)
    {
        string formatted = value.ToString("0.###"); // Ucinanie do trzech miejsc dziesiętnych
        return double.Parse(formatted);
    }
}
