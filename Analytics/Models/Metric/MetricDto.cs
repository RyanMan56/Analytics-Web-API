using System.Collections.Generic;

public class MetricDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<MetricPartDto> MetricParts { get; set; }
    public string MetricType { get; set; }
    public double Value { get; set; }
}
