using Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EventDto
{
    public int Id { get; set; }
    public string CreatedBy { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public List<PropertyDto> Properties { get; set; }

}
