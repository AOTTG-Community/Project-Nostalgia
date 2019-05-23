using ExitGames.Client.Photon;

public class RoomInfo
{
    private Hashtable customPropertiesField = new Hashtable();

    protected bool autoCleanUpField = PhotonNetwork.autoCleanUpPlayerObjects;

    protected byte maxPlayersField;

    protected string nameField;

    protected bool openField = true;

    protected bool visibleField = true;

    protected internal RoomInfo(string roomName, Hashtable properties)
    {
        this.CacheProperties(properties);
        this.nameField = roomName;
    }

    public Hashtable customProperties
    {
        get
        {
            return this.customPropertiesField;
        }
    }

    public bool isLocalClientInside { get; set; }

    public byte maxPlayers
    {
        get
        {
            return this.maxPlayersField;
        }
    }

    public string name
    {
        get
        {
            return this.nameField;
        }
    }

    public bool open
    {
        get
        {
            return this.openField;
        }
    }

    public int playerCount { get; private set; }
    public bool removedFromList { get; internal set; }

    public bool visible
    {
        get
        {
            return this.visibleField;
        }
    }

    protected internal void CacheProperties(Hashtable propertiesToCache)
    {
        if (((propertiesToCache != null) && (propertiesToCache.Count != 0)) && !this.customPropertiesField.Equals(propertiesToCache))
        {
            if (propertiesToCache.ContainsKey((byte)0xfb) && propertiesToCache[(byte)0xfb] is bool)
            {
                this.removedFromList = (bool)propertiesToCache[(byte)0xfb];
            }
            else if (propertiesToCache.ContainsKey((byte)0xfb))
            {
                propertiesToCache.Remove((byte)0xfb);
            }
            if (propertiesToCache.ContainsKey((byte)0xff) && propertiesToCache[(byte)0xff] is byte)
            {
                this.maxPlayersField = (byte)propertiesToCache[(byte)0xff];
            }
            else if (propertiesToCache.ContainsKey((byte)0xff))
            {
                propertiesToCache.Remove((byte)0xff);
            }
            if (propertiesToCache.ContainsKey((byte)0xfd) && propertiesToCache[(byte)0xfd] is bool)
            {
                this.openField = (bool)propertiesToCache[(byte)0xfd];
            }
            else if (propertiesToCache.ContainsKey((byte)0xfd))
            {
                propertiesToCache.Remove((byte)0xfd);
            }
            if (propertiesToCache.ContainsKey((byte)0xfe) && propertiesToCache[(byte)0xfe] is bool)
            {
                this.visibleField = (bool)propertiesToCache[(byte)0xfe];
            }
            else if (propertiesToCache.ContainsKey((byte)0xfe))
            {
                propertiesToCache.Remove((byte)0xfe);
            }
            if (propertiesToCache.ContainsKey((byte)0xfc) && propertiesToCache[(byte)0xfc] is byte)
            {
                this.playerCount = (byte)propertiesToCache[(byte)0xfc];
            }
            else if (propertiesToCache.ContainsKey((byte)0xfc))
            {
                propertiesToCache.Remove((byte)0xfc);
            }
            if (propertiesToCache.ContainsKey((byte)0xf9) && propertiesToCache[(byte)0xf9] is bool)
            {
                this.autoCleanUpField = (bool)propertiesToCache[(byte)0xf9];
            }
            else if (propertiesToCache.ContainsKey((byte)0xf9))
            {
                propertiesToCache.Remove((byte)0xf9);
            }
            this.customPropertiesField.MergeStringKeys(propertiesToCache);
        }
    }

    public override bool Equals(object p)
    {
        Room room = p as Room;
        return ((room != null) && this.nameField.Equals(room.nameField));
    }

    public override int GetHashCode()
    {
        return this.nameField.GetHashCode();
    }

    public override string ToString()
    {
        object[] args = new object[] { this.nameField, !this.visibleField ? "hidden" : "visible", !this.openField ? "closed" : "open", this.maxPlayersField, this.playerCount };
        return string.Format("Room: '{0}' {1},{2} {4}/{3} players.", args);
    }

    public string ToStringFull()
    {
        object[] args = new object[] { this.nameField, !this.visibleField ? "hidden" : "visible", !this.openField ? "closed" : "open", this.maxPlayersField, this.playerCount, this.customPropertiesField.ToStringFull() };
        return string.Format("Room: '{0}' {1},{2} {4}/{3} players.\ncustomProps: {5}", args);
    }
}