namespace MeddlingIdiot.HOS.Violations;

internal interface IViolationGateway
{
    void SaveViolation(Violation violation);
    List<Violation> GetViolations();
    void Clear();
}