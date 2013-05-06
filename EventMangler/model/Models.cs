using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace EventMangler.model
{
    public interface XMLable
    {
        XElement toXElement();
    }

    public interface FTLImageComposite : XMLable { }

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

    public interface FTLTextComposite : XMLable { }
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

    public enum Environment {nebula, asteroid, storm, sun, none};
    public enum Fleet { rebel, fed, battle, none};
    public static class EnumExtensions
    {
        public static Fleet toFleet(this Enum flt, string value)
        {
            switch (value) {
                case "fed":
                    return Fleet.fed;
                case "rebel":
                    return Fleet.rebel;
                case "battle":
                    return Fleet.battle;
                default: 
                    return Fleet.none;
            };
        }

        public static Environment toEnvironment(this Enum env, string value)
        {
            switch (value)
            {
                case "nebula":
                    return Environment.nebula;
                case "asteroid":
                    return Environment.asteroid;
                case "storm":
                    return Environment.storm;
                case "sun":
                    return Environment.sun;
                default:
                    return Environment.none;
            };
        }
    }

    public interface FTLEventComposite : XMLable { }
    public class FTLEvent
    {
        public string name;
        public FTLTextComposite text = null;
        public List<FTLEffect> effects = new List<FTLEffect>();
        public FTLChoice choice = null;
        public FTLShip ship = null;
        public Environment environment = Environment.none;
        public Fleet fleet = Fleet.none;
        public FTLImageComposite back = null, planet = null;
        public bool isUnique = false, isDistressBeacon = false, 
            isStore = false, isRepair = false;        

        public FTLEvent(XElement ele)
        {            
            foreach (XAttribute attribute in ele.Attributes())
            {
                switch (attribute.Name.ToString())
                {
                    case "unique":
                        this.isUnique = Boolean.Parse(attribute.Value);
                        break;
                }
            }
            foreach (XElement child in ele.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "fleet":
                        //this.fleet = 
                        this.fleet = Fleet.none.toFleet(child.Value);
                        break;
                    case "environment":
                        this.environment = Environment.none.toEnvironment(child.Value);
                        break;
                    case "img":
                        if (child.Attribute("back") != null) 
                        {
                            /// Load from image library
                            /// this.back = child.Attribute("back");
                        }
                        if (child.Attribute("planet") != null) 
                        {
                            /// Load from image library
                            /// this.planet = child.Attribute("back");
                        }
                        break;
                    case "distressBeacon":
                        this.isDistressBeacon = true;
                        break;
                    case "store":
                        this.isStore = true;
                        break;
                    case "repair":
                        this.isRepair = true;
                        break;
                    default:
                        this.effects.Add(new FTLEffect(child));
                        break;

                }
            }
            
        }

        public FTLEvent(string name, FTLTextComposite text = null, List<FTLEffect> effects = null, FTLChoice choice = null, FTLShip ship = null,
            Environment environment = Environment.none, Fleet fleet = Fleet.none,
            FTLImageComposite back = null, FTLImageComposite planet = null,
            bool isUnique = false, bool isDistressBeacon = false, bool isStore = false, bool isRepair = false)
        {
            this.name = name;
            this.text = text;
            if (effects != null) this.effects = effects;
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
            return ret;
        }
    }

    public interface FTLChoice : XMLable { }
    public interface FTLShip : XMLable { }
    public class FTLEffect : XMLable {
        public FTLEffect(XElement ele) { }
        
        public XElement toXElement() { return null; }
    }
}

