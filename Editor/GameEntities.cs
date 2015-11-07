using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Editor
{
    //Game Entity class. Uses a variety of dynamic techniques to avoid having to use a lot
    // of inheritance.
    //Contains a properties dictionary you can use to stick whatever you want into it.
    //If the entity is drawable, you should give it a bounding box, which is used for mouse
    // interactions and on screen drawing. Things like dragging the handles should alter the
    // bounding box, and the specific implementation of how that effects the game entity type
    // should be implemented in the delegate function for that type of bounding box.
    public class GameEntity
    {
        //Each entity should have an immutable, unique ID.
        private static int sNextID = 0;
        public int ID { get; private set; }

        //Bounding box delegates. You need to define a custom version of
        // each of these to use them. The bounding box is the smallest axis
        // aligned rectangle that the entity fits in on the screen - for a rectangle
        // this is the same size as a rectangle, but it would be different for a 
        // circle. Bounding box is used for resizing and picking the entity without
        // caring what type it is.
        public delegate void delSetBoundingBox(Rectangle r);
        public delSetBoundingBox SetBoundingBox;
        public delegate Rectangle delGetBoundingBox();
        public delGetBoundingBox GetBoundingBox;
        public delGetBoundingBox GetName;


        //The list of properties. Customizable for each entity.
        private CustomPropertyDictionary m_props = new CustomPropertyDictionary();
        public CustomPropertyDictionary Props
        {
            get { return m_props; }
            set
            {
                // Performs a deep copy of the properties
                CustomPropertyDictionary propsCopy = new CustomPropertyDictionary();
                propsCopy.Name = value.Name;

                if (Type == EntityType.RECT)
                {
                    propsCopy.TryAdd(new CustomProperty { Name = "OutlineColor", Type = typeof(Color), DefaultValue = value["OutlineColor"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "FillColor", Type = typeof(Color), DefaultValue = value["FillColor"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = value["Dimensions"] });

                }
                else if (Type == EntityType.CIRCLE)
                {
                    propsCopy.TryAdd(new CustomProperty { Name = "OutlineColor", Type = typeof(Color), DefaultValue = value["OutlineColor"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "FillColor", Type = typeof(Color), DefaultValue = value["FillColor"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Radius", Type = typeof(int), DefaultValue = value["Radius"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Position", Type = typeof(Point), DefaultValue = value["Position"] });
                }
                else if (Type == EntityType.SPRITE)
                {
                    propsCopy.TryAdd(new CustomProperty { Name = "OutlineColor", Type = typeof(Color), DefaultValue = value["OutlineColor"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Flipped", Type = typeof(bool), DefaultValue = value["Flipped"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = value["SpriteName"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = value["Dimensions"] });
                }
                else if (Type == EntityType.LINETRACK)
                {
                    propsCopy.TryAdd(new CustomProperty { Name = "DispHeight", Type = typeof(int), DefaultValue = value["DispHeight"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "PicWidth", Type = typeof(int), DefaultValue = value["PicWidth"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "PicHeight", Type = typeof(int), DefaultValue = value["PicHeight"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Speed", Type = typeof(int), DefaultValue = value["Speed"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = value["SpriteName"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = value["Dimensions"] });
                }
                else if (Type == EntityType.CONVEYBELT)
                {
                    propsCopy.TryAdd(new CustomProperty { Name = "DispHieght", Type = typeof(int), DefaultValue = value["DispHieght"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "PicWidth", Type = typeof(int), DefaultValue = value["PicWidth"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "PicHeight", Type = typeof(int), DefaultValue = value["PicHeight"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Speed", Type = typeof(int), DefaultValue = value["Speed"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Clockwise", Type = typeof(bool), DefaultValue = value["Clockwise"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = value["SpriteName"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = value["Dimensions"] });

                } 
                else if (Type == EntityType.LINESIN)
                {
                    propsCopy.TryAdd(new CustomProperty { Name = "a", Type = typeof(int), DefaultValue = value["a"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "b", Type = typeof(int), DefaultValue = value["b"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "c", Type = typeof(int), DefaultValue = value["c"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "PicWidth", Type = typeof(int), DefaultValue = value["PicWidth"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "PicHeight", Type = typeof(int), DefaultValue = value["PicHeight"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Speed", Type = typeof(int), DefaultValue = value["Speed"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Static", Type = typeof(bool), DefaultValue = value["Static"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = value["SpriteName"] });
                    propsCopy.TryAdd(new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = value["Dimensions"] });
                }
                m_props = propsCopy;
            }
        }
        //The outline box is an Axis Aligned Bounding box that is slightly
        // bigger than the bounding box. It is used when you draw a box around
        // a selected game entity in the editor.
        private const int OUTLINE_PADDING = 4;
        [Browsable(false)]
        public Rectangle OutlineBox
        {
            get
            {
                Rectangle r = GetBoundingBox();
                return new Rectangle(r.X - OUTLINE_PADDING, r.Y - OUTLINE_PADDING, 
                    r.Width + (2 * OUTLINE_PADDING), r.Height + (2 * OUTLINE_PADDING)
                );
            }

            
        }

        private const int HALFHANDLESIZE = 3;
        private const int HANDLESIZE = HALFHANDLESIZE * 2;
        [Browsable(false)]
        public Rectangle[] CornerHandles
        {
            get
            {
                Rectangle outline = OutlineBox;
                return new Rectangle[] {
                    new Rectangle(outline.X - HALFHANDLESIZE, outline.Y - HALFHANDLESIZE, HANDLESIZE, HANDLESIZE),
                    new Rectangle(outline.X + outline.Width - HALFHANDLESIZE, outline.Y - HALFHANDLESIZE, HANDLESIZE, HANDLESIZE),
                    new Rectangle(outline.X + outline.Width - HALFHANDLESIZE, outline.Y + outline.Height - HALFHANDLESIZE, HANDLESIZE, HANDLESIZE),
                    new Rectangle(outline.X - HALFHANDLESIZE, outline.Y + outline.Height - HALFHANDLESIZE, HANDLESIZE, HANDLESIZE)
                };
            }
        }

        //Type of entity. The integer value should be consist in the game world.
        public EntityType Type { get; private set; }

        //Used for the listbox that display entities.
        public override string ToString()
        {
            return this.Type.ToString() + " " + ID;
        }

        //Constructor that ensures a unique ID. Add a new constructor at your peril.
        private GameEntity()
        {
            ID = sNextID++;
        }

        //Factory method for creating a rectangular game entity. This can be chained with other
        // factory methods to create new entity types - for example a sprite game entity would
        // create a rectangle entity first, then have additional logic to add a texture, subrect, etc...
        public static GameEntity CreateRectangle(int x, int y, int w, int h)
        {
            GameEntity ge = new GameEntity();
            ge.Type = EntityType.RECT;
            ge.Props.TryAdd(new CustomProperty { Name = "OutlineColor", Type = typeof(Color), DefaultValue = Color.Black });
            ge.Props.TryAdd(new CustomProperty { Name = "FillColor", Type = typeof(Color), DefaultValue = Color.Transparent });
            CustomProperty dim = new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = new Rectangle(x,y,w,h) };
            ge.Props.TryAdd(dim);
            
            ge.SetBoundingBox = new delSetBoundingBox(delegate(Rectangle r)
            {
                ge.Props["Dimensions"] = r;
            });

            ge.GetBoundingBox = new delGetBoundingBox(delegate()
            {
                return ge.Props["Dimensions"] == null ? (Rectangle)dim.DefaultValue : (Rectangle)ge.Props["Dimensions"];
            });

               
            return ge;
        }

        // Create Circle
        public static GameEntity CreateCircle(int radius, Point position)
        {
            GameEntity ge = new GameEntity();
            ge.Type = EntityType.CIRCLE;
            ge.Props.TryAdd(new CustomProperty { Name = "OutlineColor", Type = typeof(Color), DefaultValue = Color.Magenta });
            ge.Props.TryAdd(new CustomProperty { Name = "FillColor", Type = typeof(Color), DefaultValue = Color.Transparent });
            CustomProperty rad = new CustomProperty { Name = "Radius", Type = typeof(int), DefaultValue = radius };
            CustomProperty pos = new CustomProperty { Name = "Position", Type = typeof(Point), DefaultValue = new Point(position.X, position.Y) };
            ge.Props.TryAdd(rad);
            ge.Props.TryAdd(pos);

            ge.SetBoundingBox = new delSetBoundingBox(delegate (Rectangle r)
            {
                // Must set bounding box based on the circles radius and location
                ge.Props["Radius"] = r.Width/2;
                ge.Props["Position"] = r.Location;

            });

            ge.GetBoundingBox = new delGetBoundingBox(delegate ()
            {
                Size sizeDefault = new Size((int)rad.DefaultValue * 2, (int)rad.DefaultValue * 2);
                Rectangle bb = new Rectangle ((Point)pos.DefaultValue, sizeDefault);

                if (!(ge.Props["Radius"] == null && ge.Props["Position"] == null))
                {
                    // Set bounding box to encompass circle based on diameter
                    Size size = new Size((int)ge.Props["Radius"] * 2, (int)ge.Props["Radius"] * 2);
                    bb = new Rectangle((Point)ge.Props["Position"], size);

                }

                return bb;

            });


            return ge;
        }

        //create sprite
        public static GameEntity CreateSprite(int x, int y, int w, int h)
        {
            GameEntity ge = new GameEntity();
            ge.Type = EntityType.SPRITE;
            ge.Props.TryAdd(new CustomProperty { Name = "OutlineColor", Type = typeof(Color), DefaultValue = Color.Transparent });
            ge.Props.TryAdd(new CustomProperty { Name = "Flipped", Type = typeof(bool), DefaultValue = false });
            ge.Props.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = "water1.png" });
            CustomProperty dim = new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = new Rectangle(x, y, w, h) };
            ge.Props.TryAdd(dim);


            ge.SetBoundingBox = new delSetBoundingBox(delegate (Rectangle r)
            {
                ge.Props["Dimensions"] = r;
            });

            ge.GetBoundingBox = new delGetBoundingBox(delegate ()
            {
                return ge.Props["Dimensions"] == null ? (Rectangle)dim.DefaultValue : (Rectangle)ge.Props["Dimensions"];
            });



            return ge;
        }

        //create linetrack
        public static GameEntity CreateLineTrack(int x, int y, int w, int h)
        {
            GameEntity ge = new GameEntity();
            ge.Type = EntityType.LINETRACK;
            ge.Props.TryAdd(new CustomProperty { Name = "DispHeight", Type = typeof(int), DefaultValue = 10 });
            ge.Props.TryAdd(new CustomProperty { Name = "PicWidth", Type = typeof(int), DefaultValue = 50 });
            ge.Props.TryAdd(new CustomProperty { Name = "PicHeight", Type = typeof(int), DefaultValue = 50 });
            ge.Props.TryAdd(new CustomProperty { Name = "Speed", Type = typeof(int), DefaultValue = 1 });
            ge.Props.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = "water1.png" });
            CustomProperty dim = new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = new Rectangle(x, y, w, h) };
            ge.Props.TryAdd(dim);

            ge.SetBoundingBox = new delSetBoundingBox(delegate(Rectangle r)
            {
                ge.Props["Dimensions"] = r;
            });

            ge.GetBoundingBox = new delGetBoundingBox(delegate()
            {
                return ge.Props["Dimensions"] == null ? (Rectangle)dim.DefaultValue : (Rectangle)ge.Props["Dimensions"];
            });


            return ge;
        }

        //create Conveyor belt
        public static GameEntity CreateConveyBelt(int x, int y, int w, int h)
        {
            GameEntity ge = new GameEntity();
            ge.Type = EntityType.CONVEYBELT;
            ge.Props.TryAdd(new CustomProperty { Name = "DispHieght", Type = typeof(int), DefaultValue = 1 });
            ge.Props.TryAdd(new CustomProperty { Name = "PicWidth", Type = typeof(int), DefaultValue = 50 });
            ge.Props.TryAdd(new CustomProperty { Name = "PicHeight", Type = typeof(int), DefaultValue = 50 });
            ge.Props.TryAdd(new CustomProperty { Name = "Speed", Type = typeof(int), DefaultValue = 1 });
            ge.Props.TryAdd(new CustomProperty { Name = "Clockwise", Type = typeof(bool), DefaultValue = true });
            ge.Props.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = "water1.png" });
            CustomProperty dim = new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = new Rectangle(x, y, w, h) };
            ge.Props.TryAdd(dim);

            ge.SetBoundingBox = new delSetBoundingBox(delegate(Rectangle r)
            {
                ge.Props["Dimensions"] = r;
            });

            ge.GetBoundingBox = new delGetBoundingBox(delegate()
            {
                return ge.Props["Dimensions"] == null ? (Rectangle)dim.DefaultValue : (Rectangle)ge.Props["Dimensions"];
            });


            return ge;
        }

        //create LineSin
        public static GameEntity CreateLineSin(int x, int y, int w, int h)
        {
            GameEntity ge = new GameEntity();
            ge.Type = EntityType.LINESIN;
            ge.Props.TryAdd(new CustomProperty { Name = "a", Type = typeof(int), DefaultValue = 1 });
            ge.Props.TryAdd(new CustomProperty { Name = "b", Type = typeof(int), DefaultValue = 1 });
            ge.Props.TryAdd(new CustomProperty { Name = "c", Type = typeof(int), DefaultValue = 0 });
            ge.Props.TryAdd(new CustomProperty { Name = "PicWidth", Type = typeof(int), DefaultValue = 20 });
            ge.Props.TryAdd(new CustomProperty { Name = "PicHeight", Type = typeof(int), DefaultValue = 20 });
            ge.Props.TryAdd(new CustomProperty { Name = "Speed", Type = typeof(int), DefaultValue = 1 });
            ge.Props.TryAdd(new CustomProperty { Name = "Static", Type = typeof(bool), DefaultValue = false });
            ge.Props.TryAdd(new CustomProperty { Name = "SpriteName", Type = typeof(string), DefaultValue = "water1.png" });
            CustomProperty dim = new CustomProperty { Name = "Dimensions", Type = typeof(Rectangle), DefaultValue = new Rectangle(x, y, w, h) };
            ge.Props.TryAdd(dim);

            ge.SetBoundingBox = new delSetBoundingBox(delegate(Rectangle r)
            {
                ge.Props["Dimensions"] = r;
            });

            ge.GetBoundingBox = new delGetBoundingBox(delegate()
            {
                return ge.Props["Dimensions"] == null ? (Rectangle)dim.DefaultValue : (Rectangle)ge.Props["Dimensions"];
            });


            return ge;
        }

        //Output this game entity as an XMLElement. The Entity is the element and will contain
        // each property as a sub-element. In addition, the entity will contain attributes for the
        // Type (as an int), Type (as a string), and ID.
        public XmlElement GenerateXML(XmlDocument doc)
        {
            //Output the XML. Note that this doesn't care what type of entity is being outputted.
            XmlElement el = doc.CreateElement("GameEntity");
            XmlAttribute tatt = doc.CreateAttribute("Type");
            tatt.Value = ((int)this.Type).ToString();
            //The string name is useful for debugging.
            XmlAttribute tsatt = doc.CreateAttribute("TypeStr");
            tsatt.Value = this.Type.ToString();
            XmlAttribute idatt = doc.CreateAttribute("ID");
            idatt.Value = this.ID.ToString();
            el.Attributes.Append(tatt);
            el.Attributes.Append(tsatt);
            el.Attributes.Append(idatt);
            this.Props.ToXml(el, doc);
            return el;
        }

    }
}
