//Samolyotlar (Airplane) bir-biri bilan emas, ATC (Air Traffic Control) bilan muloqot qiladi. 
//Har bir samolyot uchishdan oldin ATC’dan ruxsat so‘raydi, ATC esa holatga qarab ruxsat beradi yoki rad etadi.

public class Aircraft
{
    public string CallSign { get; }
    private IAirTrafficControl _atc;

    public Aircraft(string callSign, IAirTrafficControl atc)
    {
        CallSign = callSign;
        _atc = atc;
        _atc.RegisterAircraft(this);
    }

    public void RequestTakeOff()
    {
        _atc.RequestTakeOff(this);
    }

    public void TakeOff()
    {
        Console.WriteLine($"{CallSign} is taking off.");
        (_atc as AirTrafficControlTower)?.NotifyAirbone(this);
    }

    public void Land()
    {
        Console.WriteLine($"{CallSign} is landing.");
        (_atc as AirTrafficControlTower)?.NotifyLanding(this);
    }
}

public interface IAirTrafficControl
{
    void RegisterAircraft(Aircraft aircraft);
    void RequestTakeOff(Aircraft aircraft);
}

public class AirTrafficControlTower : IAirTrafficControl
{
    private List<Aircraft> _aircraftsInAir = new();

    public void RegisterAircraft(Aircraft aircraft)
    {
        Console.WriteLine($"{aircraft.CallSign} registered to Tower");
    }

    public void RequestTakeOff(Aircraft aircraft)
    {
        if (_aircraftsInAir.Count == 0)
        {
            Console.WriteLine($"{aircraft.CallSign} is cleared for takeoff.");
            _aircraftsInAir.Add(aircraft);
        }
        else
        {
            Console.WriteLine($"{aircraft.CallSign} is not cleared for takeoff. Another aircraft is already in the air.");
        }
    }

    public void NotifyAirbone(Aircraft aircraft)
    {
        Console.WriteLine($"{aircraft.CallSign} is now airborne.");
    }

    public void NotifyLanding(Aircraft aircraft)
    {
        Console.WriteLine($"{aircraft.CallSign} is landing.");
        _aircraftsInAir.Remove(aircraft);
    }
}

class Program
{
    static void Main()
    {
        var airTower = new AirTrafficControlTower();

        var aircraft1 = new Aircraft("Flight 101", airTower);
        var aircraft2 = new Aircraft("Flight 202", airTower);

        aircraft1.RequestTakeOff();
        aircraft1.TakeOff();

        aircraft2.RequestTakeOff();

        aircraft1.Land();

        aircraft2.RequestTakeOff();
        aircraft2.TakeOff();

        Console.ReadKey();
    }
}