namespace TollFeeCalculator
{
    public class VehiclePassage
    {
        public required Vehicle Vehicle { get; set; }
        public DateTime TimeStamp { get; set; }

        //Suggested methods to implement e g IEqualityComparer<VehiclePassage>
        public bool Equals(VehiclePassage? x, VehiclePassage? y)
        {
            if (x is null || y is null) return false;
            return x.Vehicle.LicenseNumber == y.Vehicle.LicenseNumber && x.TimeStamp == y.TimeStamp;
        }
        public int GetHashCode(VehiclePassage obj)
        {
            if (obj is null) return 0;
            return obj.Vehicle.LicenseNumber.GetHashCode() ^ obj.TimeStamp.GetHashCode();
        }
        public override bool Equals(object? obj)
        {
            if (obj is null || obj.GetType() != GetType()) return false;
            return Equals(this, (VehiclePassage)obj);
        }
        public override int GetHashCode()
        {
            return Vehicle.LicenseNumber.GetHashCode() ^ TimeStamp.GetHashCode();
        }

        public static bool operator ==(VehiclePassage left, VehiclePassage right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(left, right);
        }
        public static bool operator !=(VehiclePassage left, VehiclePassage right)
        {
            return !(left == right);
        }

        public override string ToString() => $"{Vehicle.LicenseNumber} - {TimeStamp}";
    }
}