using System.Collections.Generic;
[System.Serializable]
public class LocationData
{
    public string id;
    public string name;
    public string script;
    public string end_turn;
    public int slots;
    public List<string> starting_tokens;
}

[System.Serializable]
public class LocationDataList
{
    public List<LocationData> locations;
}
