using System.Xml.Linq;

namespace EventMangler.model
{
    public interface FTLImageComposite
    {
        public XElement toXElement();
    }
    public struct FTLImage : FTLImageComposite
    {
        public readonly int w, h;
        public readonly string path;

        public FTLImage(string path, int w, int h)
        {
            this.path = path;
            this.w = w;
            this.h = h;
        }

        public XElement toXElement()
        {
            return new XElement("img", new XAttribute("w", w), new XAttribute("h", h), path);
        }
    }

    public interface FTLTextComposite
    {
        public XElement toXElement();
    }
    public struct FTLText : FTLTextComposite
    {
        public readonly string back, planet;
        public readonly string text;

        public FTLText(XElement ele)
            : this(ele.Value, ele.Attribute("back").Value, ele.Attribute("planet").Value)
        { }

        public FTLText(string text, string back, string planet)
        {
            this.text = text;
            this.back = back;
            this.planet = planet;
        }

        public XElement toXElement()
        {
            XElement ret = new XElement("text", text);
            if (back != null && !back.Trim().Equals("")) ret.SetAttributeValue("back", back);
            if (planet != null && !planet.Trim().Equals("")) ret.SetAttributeValue("planet", planet);
            return ret;
        }
    }

    enum Environment {nebula, astroid, storm, sun, none};
    enum Fleet {rebel, fed, both, none};

    public interface FTLEventComposite
    {
        public XElement toXElement();
    }
    public struct FTLEvent
    {        
        public FTLTextComposite text;
        public FTLEffect effect;
        public FTLChoice choice;
        public FTLShip ship;
        public Environment environment;
        public Fleet fleet;
        public string back, planet;
        public bool isUnique, isDistressBeacon, isStore, isRepair;        

        public FTLEvent(XElement ele)
        { 
            
        }

        public FTLEvent(FTLTextComposite text, FTLEffect effect = null, FTLChoice choice = null, FTLShip ship = null,
            Environment environment = Environment.none, Fleet fleet = Fleet.none,
            string back = null, string planet = null,
            bool isUnique = false, bool isDistressBeacon = false, bool isStore = false, bool isRepair = false)
        {
            this.text = text;
            this.effect = effect;
            this.choice = choice;
            this.ship = ship;
            this.environment = environment;
            this.fleet = fleet;
            this.back = back;
            this.planet = planet;
            this.isUnique = isUnique;
            this.isDistressBeacon = isDistressBeacon;
            this.isStore = isStore;
            this.isRepair = isRepair;
        }

        public XElement toXElement()
        {
            XElement ret = new XElement("text", text);
            if (back != null && !back.Trim().Equals("")) ret.SetAttributeValue("back", back);
            if (planet != null && !planet.Trim().Equals("")) ret.SetAttributeValue("planet", planet);
            return ret;
        }
    }

    public interface FTLChoice 
    {
        public XElement toXElement();
    }

    public interface FTLShip
    {
        public XElement toXElement();
    }
    
    public interface FTLEffect 
    {
        public XElement toXElement();
    }
}

