namespace MeddlingIdiot.HOS.TimelineNavigator.Moments;

[Serializable]
public sealed class JurisdictionMoment : Moment
{
    public string? JurisdictionName { get; private set; }
        
    
    public JurisdictionMoment() { }

    public JurisdictionMoment(DateTime timestamp,
        string? jurisdictionName = null,
        string? driverIdNumber = null,
        string? truckNumber = null)
    {
        Timestamp=timestamp; 
        JurisdictionName=jurisdictionName;
        DriverIdNumber=driverIdNumber;
        TruckNumber =truckNumber;
    }

    public override object Clone()
    {
        var src = this;
        JurisdictionMoment dest = new JurisdictionMoment();
        dest = (JurisdictionMoment)PopulateClone(src, dest);
        dest.JurisdictionName = src.JurisdictionName;
        dest.DriverIdNumber = src.DriverIdNumber;
        dest.TruckNumber = src.TruckNumber;
        return dest;
    }

}