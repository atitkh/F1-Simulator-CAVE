using System;
using System.Collections.Generic;
using UnityEngine;

// Data classes
[Serializable]
public class LocationData
{
    public List<Location> data;
}

[Serializable]
public class CarData
{
    public List<Car> data;
}

[Serializable]
public class DriverList
{
    public List<Driver> data;
}

[Serializable]
public class IntervalList
{
    public List<Interval> data;
}

[Serializable]
public class Location
{
    public int meeting_key;
    public int session_key;
    public int driver_number;
    public string date;
    public int x;
    public int y;
    public int z;
}

[Serializable]
public class Car
{
    public int meeting_key;
    public int session_key;
    public int driver_number;
    public string date;
    public int rpm;
    public int speed;
    public int n_gear;
    public int throttle;
    public int drs;
    public int brake;
}

[Serializable]
public class Driver
{
    public int driver_number;
    public string broadcast_name;
    public string full_name;
    public string name_acronym;
    public string team_name;
    public string team_colour;
    public string first_name;
    public string last_name;
    public string headshot_url;
    public string country_code;
    public int session_key;
    public int meeting_key;
}

// Merged data class
[Serializable]
public class TrackData
{
    public int x;
    public int y;
    public int z;
    public string date;
    public Car carData;
    public Interval intervalData;
}

[Serializable]
public class Interval
{
    public int session_key;
    public int meeting_key;
    public string date;
    public int driver_number;
    public float gap_to_leader;
    public int interval;
     public int? position;
}