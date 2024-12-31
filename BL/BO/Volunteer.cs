namespace BO;

public class Volunteer
{
    public int Id { get; init; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Jobs Jobs { get; set; }
    public bool IsActive { get; set; }
    public double? MaxDistance { get; set; }
    public DistanceType DistanceType { get; set; }
    public int SumOfCalls { get; set; }
    public int SumOfCancellation { get; set; }
    public int SumOfExpiredCalls { get; set; }
    public BO.CallInProgress? CallInProgress { get; set; }
    public override string ToString() => this.ToStringProperty();    
}

