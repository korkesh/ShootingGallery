
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;


namespace Editor
{
    //I did not add this 


    public partial class Form1 : Form
    {

        static class Constants
        {
            public const int TOP_LEFT = 0;
            public const int TOP_RIGHT = 1;
            public const int BOTTOM_RIGHT = 2;
            public const int BOTTOM_LEFT = 3;
        }

        int tabCount = 1; // Counts the number of tabs which have been created
        List<List<GameEntity>> gameEntityLists = new List<List<GameEntity>>(); // List of game entities belonging to each tab, the index in the list refers to the tab index

        //List of allowed property types used for populating the combo boxes.
        public List<String> AllowedTypes = new List<string>();
        //List that aliases the fully qualified type names to their common aliases.
        public Dictionary<String, String> AliasTypes = new Dictionary<string, string>() {
            { "System.Boolean", "bool" },
            {"System.Byte", "byte"},
            {"System.SByte", "sbyte"},
            {"System.Char", "char"},
            {"System.Decimal", "decimal"},
            {"System.Double", "double"},
            {"System.Single", "float"},
            {"System.Int32", "int"},
            {"System.UInt32", "uint"},
            {"System.Int64", "long"},
            {"System.UInt64", "ulong"},
            // skip for now. {"System.Object", "object"},
            {"System.Int16", "short"},
            {"System.UInt16", "ushort"},
            {"System.String", "string"},
            {"System.Drawing.Color", "Color"},
            { typeof(Rectangle).ToString(), "Rectangle"}
        };
        public Form1()
        {
            InitializeComponent();
            //Currently there is only a single collection for game entities, which is the 
            // collection of items in the gameEntities_lb listbox. If we are going to have 
            // multiple "levels" in each tab, we need to find a way to separate which entity
            // exists in each tab.
            gameEntityLists.Insert(0, new List<GameEntity>());

            //Add a blank type to the allowed types list, then copy over
            // all the values from the alias list.
            AllowedTypes.Add("");
            AllowedTypes.AddRange(AliasTypes.Values);

            //Bind the list to each combo box, but using separate binding contextes so they
            // can be selected independently.
            addPropType_cmb.DataSource = AllowedTypes;
            editPropType_cmb.BindingContext = new BindingContext();
            editPropType_cmb.DataSource = AllowedTypes;

            //Setup the tab page events.
            SetTabPanelEvents(tabControl1.TabPages[0]);

        }

        private void SetTabPanelEvents(TabPage tp)
        {
            //Add mouse events
            tp.MouseDown += TabPanel_MouseDown;
            tp.MouseUp += TabPanel_MouseUp;
            tp.MouseClick += TabPanel_MouseClick;
            tp.MouseDoubleClick += TabPanel_MouseDoubleClick;
            tp.MouseMove += TabPanel_MouseMove;
            //Add paint event.
            tp.Paint += TabPanel_Paint;
        }

        
        private void RemoveMouseEvents(TabPage tp)
        {
            //Remove mouse events
            tp.MouseDown -= TabPanel_MouseDown;
            tp.MouseUp -= TabPanel_MouseUp;
            tp.MouseClick -= TabPanel_MouseClick;
            tp.MouseDoubleClick -= TabPanel_MouseDoubleClick;
            tp.MouseMove -= TabPanel_MouseMove;
            //Remove paint events
            tp.Paint -= TabPanel_Paint;
        }
        //Global boolean to check the state of the left mouse button.
        bool LMBDown = false;
        bool Dragging = false;
        bool Resizing = false;

        int resizeCorner = 0;

        Rectangle prevRec = new Rectangle();
       
        Point MouseDragOffset = new Point();
        Cursor PreviousCursor = Cursors.Default;
        Point MouseDownPos = new Point();

        private void TabPanel_MouseDown(object sender, MouseEventArgs me)
        {
            MouseDownPos = me.Location;
            if (me.Button == MouseButtons.Left)
            {
                LMBDown = true;
                if (selectedObject_pg.SelectedObject == null)
                {
                    return;
                }
                if (toolsPointer_rb.Checked)
                {

                    // if any entity under this postion is already selected 
                    // dont select a new entity
                    GameEntity alreadySe = selectedObject_pg.SelectedObject as GameEntity;
                    if (alreadySe != null)
                    {
                        // check if already selected entity is at mouse down position
                        if (!alreadySe.GetBoundingBox().Contains(me.Location))
                        {
                            // if the entity is not at mouse down position
                            // check if any of the corner handle of selected entity is at mouse down position
                            bool corAtMouseDown = false;
                            for (int cor = 0; cor < 4; cor++)
                            {
                                if (alreadySe.CornerHandles[cor].Contains(me.Location))
                                {
                                    corAtMouseDown = true;
                                    break;
                                }
                            }
                            // if none of the corner is selected entity is at mouse down position
                            // trigger mouse click event 
                            if (!corAtMouseDown)
                                TabPanel_MouseClick(sender, me);
                        }
                    }
                    else
                    {
                        TabPanel_MouseClick(sender, me);
                    }

                    if ((selectedObject_pg.SelectedObject as GameEntity).GetBoundingBox().Contains(me.Location))
                    {
                        Dragging = true;
                        Point bb = (selectedObject_pg.SelectedObject as GameEntity).GetBoundingBox().Location;
                        Point m = me.Location;

                        MouseDragOffset = new Point(m.X - bb.X, m.Y - bb.Y);
                        PreviousCursor = tabControl1.Cursor;
                        tabControl1.Cursor = Cursors.SizeAll;
                    }
                    else
                    {
                        GameEntity ge = (selectedObject_pg.SelectedObject as GameEntity);
                        if (ge != null)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (ge.CornerHandles[i].Contains(me.Location))
                                {
                                    Console.WriteLine(me.Location);
                                    //MIDTERM:
                                    //We are on a corner handle and can start to resize it.

                                    Resizing = true;
                                    resizeCorner = i;

                                    prevRec = ge.GetBoundingBox();

                                    // change the cursor
                                    // depending upon the corner we are holding
                                    if (i == Constants.TOP_LEFT || i == Constants.BOTTOM_RIGHT)
                                    {
                                        PreviousCursor = tabControl1.Cursor;
                                        tabControl1.Cursor = Cursors.SizeNWSE;
                                    }
                                    else
                                    {
                                        PreviousCursor = tabControl1.Cursor;
                                        tabControl1.Cursor = Cursors.SizeNESW;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TabPanel_MouseUp(object sender, MouseEventArgs me)
        {
            if (me.Button == MouseButtons.Left)
            {
                LMBDown = false;
                if (Dragging)
                {
                    Dragging = false;
                    tabControl1.Cursor = PreviousCursor;
                }
                if (Resizing)
                {
                    Resizing = false;
                    tabControl1.Cursor = PreviousCursor;

                }
            }
        }

        private void TabPanel_MouseMove(object sender, MouseEventArgs me)
        {
            //MIDTERM: This needs to be modified to distinguish between moving an object and 
            // resizing it using the corners.
            if (LMBDown && Dragging)
            {
                GameEntity ge = selectedObject_pg.SelectedObject as GameEntity;
                Rectangle r = ge.GetBoundingBox();
                r.Location = new Point(me.Location.X - MouseDragOffset.X, me.Location.Y - MouseDragOffset.Y);
                ge.SetBoundingBox(r);
                RefreshAll();
            }
            else if (LMBDown && Resizing)
            {
                switch (resizeCorner)
                {
                    case Constants.TOP_LEFT:
                        {
                            // get the selected entity
                            GameEntity ge = (selectedObject_pg.SelectedObject as GameEntity);

                            // calculate drag of mouse
                            int changeInX = me.Location.X - MouseDownPos.X;
                            int changeInY = me.Location.Y - MouseDownPos.Y;

                            // get current bounding box
                            Rectangle bound = ge.GetBoundingBox();

                            // change X and widht of Bouding Box                             
                            bound.X = prevRec.X + changeInX;
                            bound.Width = prevRec.Width - changeInX;
                            // Change Y and Height of bounding Box
                            bound.Y = prevRec.Y + changeInY;
                            bound.Height = prevRec.Height - changeInY;

                            // make sure  the width and height stay positive
                            if (bound.Width < 1)
                                bound.Width = 1;
                            if (bound.Height < 1)
                                bound.Height = 1;

                            // make sure the position of box doesn't change even if we drag mouse away
                            bound.X = bound.X > prevRec.Right ? prevRec.Right : bound.X;
                            bound.Y = bound.Y > prevRec.Bottom ? prevRec.Bottom : bound.Y;

                            ge.SetBoundingBox(bound);

                            RefreshAll();
                        }
                        break;
                    case Constants.TOP_RIGHT:
                        {
                            // get the selected entity
                            GameEntity ge1 = (selectedObject_pg.SelectedObject as GameEntity);

                            // calculate drag of mouse
                            int changeInX1 = me.Location.X - MouseDownPos.X;
                            int changeInY1 = me.Location.Y - MouseDownPos.Y;

                            // get current bounding box
                            Rectangle bound1 = ge1.GetBoundingBox();

                            bound1.Y = prevRec.Y + changeInY1;

                            // change height and widht of Bouding Box 
                            bound1.Width = prevRec.Width + changeInX1;
                            bound1.Height = prevRec.Height - changeInY1;

                            // make sure  the width and height stay positive
                            if (bound1.Width < 1)
                                bound1.Width = 1;
                            if (bound1.Height < 1)
                                bound1.Height = 1;

                            // make sure the position of box doesn't change even if we drag mouse away
                            bound1.Y = bound1.Y > prevRec.Bottom ? prevRec.Bottom : bound1.Y;

                            ge1.SetBoundingBox(bound1);

                            RefreshAll();
                        }
                        break;
                    case Constants.BOTTOM_RIGHT:
                        {
                            // get the selected entity
                            GameEntity ge1 = (selectedObject_pg.SelectedObject as GameEntity);

                            // calculate drag of mouse
                            int changeInX1 = me.Location.X - MouseDownPos.X;
                            int changeInY1 = me.Location.Y - MouseDownPos.Y;

                            // get current bounding box
                            Rectangle bound1 = ge1.GetBoundingBox();


                            // change height and widht of Bouding Box 
                            bound1.Width = prevRec.Width + changeInX1;
                            bound1.Height = prevRec.Height + changeInY1;

                            // make sure  the width and height stay positive
                            if (bound1.Width < 1)
                                bound1.Width = 1;
                            if (bound1.Height < 1)
                                bound1.Height = 1;


                            ge1.SetBoundingBox(bound1);

                            RefreshAll();
                            break;
                        }

                    case Constants.BOTTOM_LEFT:
                        {
                            // get the selected entity
                            GameEntity ge1 = (selectedObject_pg.SelectedObject as GameEntity);

                            // calculate drag of mouse
                            int changeInX1 = me.Location.X - MouseDownPos.X;
                            int changeInY1 = me.Location.Y - MouseDownPos.Y;

                            // get current bounding box
                            Rectangle bound1 = ge1.GetBoundingBox();

                            // change X and widht of Bouding Box                             
                            bound1.X = prevRec.X + changeInX1;

                            // change height and widht of Bouding Box 
                            bound1.Width = prevRec.Width - changeInX1;
                            bound1.Height = prevRec.Height + changeInY1;

                            // make sure  the width and height stay positive
                            if (bound1.Width < 1)
                                bound1.Width = 1;
                            if (bound1.Height < 1)
                                bound1.Height = 1;

                            // make sure the position of box doesn't change even if we drag mouse away
                            bound1.X = bound1.X > prevRec.Right ? prevRec.Right : bound1.X;

                            ge1.SetBoundingBox(bound1);

                            RefreshAll();
                            break;
                        }
                    default:
                        break;

                }
            }
        }

        private void TabPanel_MouseClick(object sender, MouseEventArgs me)
        {
            //If this is part of a drag, do nothing.
            Point d = new Point(me.Location.X - MouseDownPos.X, me.Location.Y - MouseDownPos.Y);
            if (Math.Sqrt(d.X * d.X + d.Y * d.Y) > 10.0f)
            {
                RefreshAll();
                return;
            }
            //Check if we are in pointer mode.
            if (toolsPointer_rb.Checked && me.Button == MouseButtons.Left && gameEntities_lb.Items.Count > 0)
            {
                //Get the currently selected entity. This is needed for layering.
                int currIndex = gameEntities_lb.SelectedIndex;
                bool found = false;
                //Iterate through each entity below the currently selected one.
                for (int i = currIndex - 1; i >= 0; i--)
                {
                    GameEntity ge = gameEntities_lb.Items[i] as GameEntity;
                    //If the mouse is within the bounding box.
                    if (ge.GetBoundingBox().Contains(me.Location))
                    {
                        currIndex = i;
                        found = true;
                        break;
                    }


                    //Freed Ahmad//
                    // also check if the handles of this GameEntity are under this position
                    for (int corner = 0; corner < 4; corner++)
                    {
                        if (ge.CornerHandles[corner].Contains(me.Location))
                        {
                            currIndex = i;
                            found = true;
                            break;
                        }
                    }

                }
                //If we don't find anything below, check above (including the entity itself)
                if (!found)
                {
                    for (int i = gameEntities_lb.Items.Count - 1; i >= currIndex; i--)
                    {
                        GameEntity ge = gameEntities_lb.Items[i] as GameEntity;
                        //If the mouse is within the bounding box.
                        if (ge.GetBoundingBox().Contains(me.Location))
                        {
                            currIndex = i;
                            found = true;
                            break;
                        }
                    }
                }

                //Set the new index (if one was found) and refresh.
                gameEntities_lb.SelectedIndex = currIndex;

                RefreshAll();
            }
        }

        private void TabPanel_MouseDoubleClick(object sender, MouseEventArgs me)
        {
            //Create a rectangle.
            GameEntity ge = null;
            if (toolsRect_rb.Checked && me.Button == MouseButtons.Left)
            {
                ge = GameEntity.CreateRectangle(me.X, me.Y, 100, 100);
                
            }

            //create sprite
            if (toolsSprite_rb.Checked && me.Button == MouseButtons.Left)
            {
                ge = GameEntity.CreateSprite(me.X, me.Y, 100, 100);
                
            }

            // Create a circle.
            if (toolsCircle_rb.Checked && me.Button == MouseButtons.Left)
            {
                ge = GameEntity.CreateCircle(50, me.Location);

            }
            //create line track
            if (toolsLineTrack_rb.Checked && me.Button == MouseButtons.Left)
            {
                ge = GameEntity.CreateLineTrack(me.X, me.Y, 100, 20);

            }

            //create convey belt
            if (toolsConveyBelt_rb.Checked && me.Button == MouseButtons.Left)
            {
                ge = GameEntity.CreateConveyBelt(me.X, me.Y, 100, 20);

            }

            //create sinline
            if (toolsLineSin_rb.Checked && me.Button == MouseButtons.Left)
            {
                ge = GameEntity.CreateLineSin(me.X, me.Y, 100, 20);

            }

            if (ge != null)
            {
                gameEntities_lb.Items.Add(ge);
                gameEntities_lb.SelectedIndex = gameEntities_lb.Items.Count - 1;
                selectedObject_pg.SelectedObject = ge;
                RefreshAll();
            }
            
        }
        //Custom paint method for displaying the objects. 
        private void TabPanel_Paint(object sender, PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            for (int i = 0; i <= gameEntities_lb.Items.Count - 1; i++)
            {
                GameEntity ge = gameEntities_lb.Items[i] as GameEntity;
                switch (ge.Type)
                {
                    case EntityType.RECT: {
                        Rectangle bb = ge.GetBoundingBox();
                        Color c = (Color)ge.Props["FillColor"];
                        using (SolidBrush sb = new SolidBrush((Color)ge.Props["FillColor"]))
                        {
                            g.FillRectangle(sb, bb);
                        }
                        using (Pen p = new Pen((Color)ge.Props["OutlineColor"]))
                        {
                            g.DrawRectangle(p, bb);
                        }
                        break; }
                    case EntityType.CIRCLE:
                        {
                            Rectangle bb = ge.GetBoundingBox();
                            Color c = (Color)ge.Props["FillColor"];
                            using (SolidBrush sb = new SolidBrush((Color)ge.Props["FillColor"]))
                            {
                                g.FillEllipse(sb, bb);
                            }
                            using (Pen p = new Pen((Color)ge.Props["OutlineColor"]))
                            {
                                g.DrawEllipse(p, bb);
                            }
                            break;
                        }
                    case EntityType.SPRITE:
                        {
                            //creating sprite manually give path where image should be
                            string s = "..\\..\\..\\Resources\\Assets\\" + (string)ge.Props["SpriteName"];
                            Image im = Image.FromFile(s);
                            Rectangle bb = ge.GetBoundingBox();
                            using (Pen p = new Pen((Color)ge.Props["OutlineColor"]))
                            {
                                g.DrawRectangle(p, bb);
                                g.DrawImage(im, bb);
                            }
                            break;
                        }
                    case EntityType.LINETRACK:
                        {
                            //creating linetracksprite manually give path where image should be
                            string s = "..\\..\\..\\Resources\\Assets\\" + (string)ge.Props["SpriteName"];
                            Image im = Image.FromFile(s);
                            Rectangle bb = ge.GetBoundingBox();
                            Pen sb = new Pen(Color.Gray);
                            sb.Width = 3;
                            //get x pos at end of line
                            int nx = bb.X + bb.Width;
                            int ny = bb.Y + (bb.Height / 2);
                            //drawingline
                            g.DrawLine(sb, bb.X, ny, nx, ny);
                            //calculate how many pictures can fit
                            int howmany = bb.Width / (int)ge.Props["PicWidth"];
                            //go through how many and alternate height for everyother one
                            for (int iter = 0; iter < howmany; iter++)
                            {
                                if (iter % 2 == 0)
                                {
                                    g.DrawImage(im, new Rectangle((bb.X + iter * (int)ge.Props["PicWidth"]), (ny - (int)ge.Props["PicHeight"]), (int)ge.Props["PicWidth"], (int)ge.Props["PicHeight"]));
                                }
                                else
                                {
                                    g.DrawImage(im, new Rectangle((bb.X + iter * (int)ge.Props["PicWidth"]), ((ny - (int)ge.Props["PicHeight"]) + (int)ge.Props["DispHeight"]), (int)ge.Props["PicWidth"], (int)ge.Props["PicHeight"]));
                                }

                            }
                            break;
                        }
                    case EntityType.CONVEYBELT:
                        {
                            //get pic
                            string s = "..\\..\\..\\Resources\\Assets\\" + (string)ge.Props["SpriteName"];
                            Image im = Image.FromFile(s);
                            Rectangle bb = ge.GetBoundingBox();
                            Pen sb = new Pen(Color.Black);
                            sb.Width = 3;
                            //arc will be drawn in a rectangle of 1/8 the width of the track
                            int arcBwidth = bb.Width / 8;
                            //arc will only take up 1/16th of box so line must start there
                            int lineStart = arcBwidth / 2;
                            //draw both lines
                            g.DrawLine(sb, bb.X+lineStart-1, bb.Y, bb.X+bb.Width-lineStart+1, bb.Y);
                            g.DrawLine(sb, bb.X + lineStart - 1, bb.Y + bb.Height, bb.X + bb.Width - lineStart + 1, bb.Y + bb.Height);
                            //draw both arcs
                            g.DrawArc(sb, new Rectangle(bb.X,bb.Y,arcBwidth,bb.Height), 90.0f, 180.0f);
                            g.DrawArc(sb, new Rectangle((bb.X + (bb.Width-arcBwidth)), bb.Y, arcBwidth, bb.Height), -90.0f, 180.0f);
                            //how amny can fit (didnt do height for editor)
                            int howmanyW = bb.Width / (int)ge.Props["PicWidth"];
                            int howmanyH = bb.Height/ (int)ge.Props["PicWidth"];
                            //draws pisc above
                            for (int iter = 0; iter < howmanyW; iter++)
                            {
                                g.DrawImage(im, new Rectangle((bb.X + iter * (int)ge.Props["PicWidth"]), bb.Y - (int)ge.Props["PicHeight"], (int)ge.Props["PicWidth"], (int)ge.Props["PicHeight"]));
                            }
                            //flips for below
                            im.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            for (int iter = 0; iter < howmanyW; iter++)
                            {
                                
                                g.DrawImage(im, new Rectangle((bb.X + iter * (int)ge.Props["PicWidth"]), bb.Y + bb.Height, (int)ge.Props["PicWidth"], (int)ge.Props["PicHeight"]));
                            }
                            im.RotateFlip(RotateFlipType.Rotate180FlipNone);


                            break;
                        }
                    case EntityType.LINESIN:
                        {
                            //get pic
                            string s = "..\\..\\..\\Resources\\Assets\\" + (string)ge.Props["SpriteName"];
                            Image im = Image.FromFile(s);
                            Rectangle bb = ge.GetBoundingBox();
                            Pen sb = new Pen(Color.Gray);
                            sb.Width = 3;
                            //stuff to draw line
                            int nx = bb.X + bb.Width;
                            int ny = bb.Y;
                            g.DrawLine(sb, bb.X, ny, nx, ny);
                            int howmany = bb.Width / (int)ge.Props["PicWidth"];
                            int myX;
                            int myY;
                            //start calculations for sin wave
                            double crazyAmp = (2 * Math.PI / (bb.Width * 1 / (int)(int)ge.Props["b"]));

                            for (int iter = 0; iter < howmany; iter++)
                            {
                                //draw pic in right location based on formula
                                myX = (int)ge.Props["PicWidth"] * iter;
                                double sin = Math.Sin(crazyAmp * (double)myX);
                                double complete = (int)ge.Props["a"] * sin * (2);
                                myY = (int)(complete + (int)ge.Props["c"]);
                                g.DrawImage(im, new Rectangle((bb.X + myX), (bb.Y - myY), (int)ge.Props["PicWidth"], (int)ge.Props["PicHeight"]));
                            }
                            break;
                        }
                    default: {
                        Rectangle bb = ge.GetBoundingBox();
                        using (Pen p = new Pen(Color.Black))
                        {
                            g.DrawRectangle(p, bb);
                        }
                    break; }
                }
                //bounding box stuff
                if (gameEntities_lb.SelectedItem == ge)
                {
                    Rectangle outline = ge.OutlineBox;
                    using (Pen p = new Pen(Color.Red))
                    {
                        p.DashPattern = new float[]{4.0f, 2.0f};
                        g.DrawRectangle(p, outline );
                    }
                    using (SolidBrush sb = new SolidBrush(Color.Red))
                    {
                        foreach (Rectangle r in ge.CornerHandles)
                        {
                            g.FillRectangle(sb, r);
                        }
                    }
                }
            }
        }

        //Refresh everything so that the interface stays up to date. The flickering is
        // difficult to fix - you would need to get into double buffering to clean things
        // up. Since this is a tool we don't need to care all that much about the flickering.
        //It is expensive to blindly refresh everything, but again we need to consider that
        // this is a tool - having a big complex algorithm to only refresh the required
        // components isn't worth the time or effort.
        private void RefreshAll()
        {
            gameEntities_lb.Refresh();
            GameEntity ge = gameEntities_lb.SelectedItem as GameEntity;
            selectedObject_pg.SelectedObject = ge;
            selectedObject_pg.Refresh();
            selectedObject_pg.ExpandAllGridItems();
            tabControl1.SelectedTab.Refresh();
        }

        //Trigger when the player changes a grid item (i.e. a property of the selected item).
        private void selectedObject_pg_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            PropertyGrid pg = sender as PropertyGrid;
            if (pg == null)
            {
                RefreshAll();
                return;
            }
            if (pg.SelectedObject == null)
            {
                RefreshAll();
                return;
            }
            Type t = pg.SelectedGridItem.Value.GetType();
            if (t != typeof(CustomPropertyDictionary))
            {
                editPropName_txt.Text = pg.SelectedGridItem.Label;
                String type = pg.SelectedGridItem.Value.GetType().ToString();
                String tout = "";
                editPropType_cmb.Text = AliasTypes.TryGetValue(type, out tout) ? tout : type;
                
            }
            else
            {
                editPropName_txt.Text = "";
                editPropType_cmb.Text = "";
            }
            RefreshAll();
        }

        
        //Use at your own risk to rename a property.
        private void editProperty_btn_Click(object sender, EventArgs e)
        {
            PropertyGrid pg = selectedObject_pg;
            if (pg.SelectedObject == null)
            {
                return;
            }
            Type t = pg.SelectedGridItem.Value.GetType();
            GameEntity ge = pg.SelectedObject as GameEntity;
            
            if (ge != null && t != typeof(CustomPropertyDictionary))
            {
                if (editPropName_txt.Text != null && editPropName_txt.Text.Trim().Length > 0)
                {
                    ge.Props.TryRename(pg.SelectedGridItem.Label, editPropName_txt.Text);
                    RefreshAll();
                }
            }
        }

        //Use at your own risk to delete a property!
        private void deleteProp_btn_Click(object sender, EventArgs e)
        {
            PropertyGrid pg = selectedObject_pg;
            if (pg.SelectedObject == null)
            {
                return;
            }
            Type t = pg.SelectedGridItem.Value.GetType();
            GameEntity ge = pg.SelectedObject as GameEntity;

            if (ge != null && t != typeof(CustomPropertyDictionary))
            {
                ge.Props.TryDelete(pg.SelectedGridItem.Label);
                RefreshAll();
            }
        }

        //Use at your own risk to add a property. 
        private void addNewProperty_btn_Click(object sender, EventArgs e)
        {
            PropertyGrid pg = selectedObject_pg;
            if (pg.SelectedObject == null)
            {
                return;
            }
            GameEntity ge = pg.SelectedObject as GameEntity;

            if ( ge != null && addPropName_txt.Text != null && addPropName_txt.Text.Trim().Length > 0 )
            {
                Type newType = typeof(System.String);
                if(addPropType_cmb.Text != null && addPropType_cmb.Text.Trim().Length > 0) {
                    try {
                        String t = "System.String";
                        if (AliasTypes.ContainsValue(addPropType_cmb.Text))
                        {
                            foreach (KeyValuePair<string, string> kvp in AliasTypes)
                            {
                                if (kvp.Value == addPropType_cmb.Text)
                                {
                                    t = kvp.Key;
                                    break;
                                }
                            }
                        }
                        newType = Type.GetType(t);
                    } catch {
                        //Do nothing.
                    }
                }
                ge.Props.TryAdd(new CustomProperty { Name = addPropName_txt.Text, Desc = addPropName_txt.Text, Type = newType });
                RefreshAll();
            }
        }

        //Sets the cursor. You can add additional statefulness here or you can
        // use the PreviousCursor and manually set it yourself in the
        // mouse handling methods above.
        private void SetDefaultTabControlCursor()
        {
            if (toolsPointer_rb.Checked)
            {
                tabControl1.Cursor = Cursors.Arrow;
            }
            else if (toolsRect_rb.Checked)
            {
                tabControl1.Cursor = Cursors.Cross;
            }
        }

        //Whenever the user changes the tool, change the cursor to the appropriate type.
        private void ToolRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            SetDefaultTabControlCursor();
        }

        private void gameEntities_lb_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void selectedObject_pg_Click(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void selectedObject_pg_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            RefreshAll();
        }

        // Save Level on current Tab to XML
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("Entities");

                foreach (GameEntity ge in gameEntities_lb.Items)
                {
                    XmlElement el = ge.GenerateXML(doc);
                    root.AppendChild(el);
                }
                doc.AppendChild(root);
                doc.Save(saveFileDialog1.FileName);
            }
        }

        // Load Level from XML
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = "";

            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                fileName = openFileDialog1.FileName;

                gameEntities_lb.Items.Clear();

                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                XmlNode root = doc.FirstChild;

                // Iterate through each child element (which is hopefully a GameEntity
                // element) and attempt to create a game entity. If the entity is 
                // created add it to the collection of entities.
                XmlNode entityEl = root.FirstChild;
                while (entityEl != null)
                {
                    GameEntity ge = loadFromXml(entityEl);
                    if (ge != null)
                    {
                        gameEntities_lb.Items.Add(ge);
                        gameEntities_lb.SelectedIndex = gameEntities_lb.Items.Count - 1;
                        selectedObject_pg.SelectedObject = ge;
                    }

                    //Get next element
                    entityEl = entityEl.NextSibling;
                }
            }

            RefreshAll();

        }

        // Parse XML for each object type; creates new GameEntity object if possible and returns it
        GameEntity loadFromXml(XmlNode entity)
        {
            GameEntity ge = null;

            string type = entity.Attributes["TypeStr"].Value;

            if (type == "RECT")
            {
                XmlNode properties = entity.FirstChild;
                string[] outlineColour = properties.InnerText.Split(' ');

                properties = properties.NextSibling;
                string[] fillcolour = properties.InnerText.Split(' ');

                properties = properties.NextSibling;
                string[] dimension = properties.InnerText.Split(' ');

                ge = GameEntity.CreateRectangle(Int32.Parse(dimension[0]), Int32.Parse(dimension[1]), Int32.Parse(dimension[2]), Int32.Parse(dimension[3]));
                ge.Props["OutlineColor"] =  Color.FromArgb(Int32.Parse(outlineColour[3]), Int32.Parse(outlineColour[0]), Int32.Parse(outlineColour[1]), Int32.Parse(outlineColour[2]));
                ge.Props["FillColor"] = Color.FromArgb(Int32.Parse(fillcolour[3]), Int32.Parse(fillcolour[0]), Int32.Parse(fillcolour[1]), Int32.Parse(fillcolour[2]));

            }
            else if (type == "CIRCLE")
            {
                XmlNode properties = entity.FirstChild;
                string[] outlineColour = properties.InnerText.Split(' ');

                properties = properties.NextSibling;
                string[] fillcolour = properties.InnerText.Split(' ');

                properties = properties.NextSibling;
                string[] radius = properties.InnerText.Split(' ');

                properties = properties.NextSibling;
                string[] position = properties.InnerText.Split(',');

                position[0] = position[0].Substring(3);
                position[1] = position[1].Substring(2, position[1].Length - 3);

                Point pt = new Point(Int32.Parse(position[0]), Int32.Parse(position[1]));

                ge = GameEntity.CreateCircle(Int32.Parse(radius[0]), pt);
                ge.Props["OutlineColor"] = Color.FromArgb(Int32.Parse(outlineColour[3]), Int32.Parse(outlineColour[0]), Int32.Parse(outlineColour[1]), Int32.Parse(outlineColour[2]));
                ge.Props["FillColor"] = Color.FromArgb(Int32.Parse(fillcolour[3]), Int32.Parse(fillcolour[0]), Int32.Parse(fillcolour[1]), Int32.Parse(fillcolour[2]));
            }
            else if (type == "SPRITE")
            {
                XmlNode properties = entity.FirstChild;
                string spriteName = properties.InnerText;

                properties = properties.NextSibling;
                string[] outlineColour = properties.InnerText.Split(' ');

                properties = properties.NextSibling;
                string[] dimension = properties.InnerText.Split(' ');

                ge = GameEntity.CreateSprite(Int32.Parse(dimension[0]), Int32.Parse(dimension[1]), Int32.Parse(dimension[2]), Int32.Parse(dimension[3]));
                ge.Props["SpriteName"] = spriteName;
                ge.Props["OutlineColor"] = Color.FromArgb(Int32.Parse(outlineColour[3]), Int32.Parse(outlineColour[0]), Int32.Parse(outlineColour[1]), Int32.Parse(outlineColour[2]));

            }
            else if (type == "LINETRACK")
            {
                XmlNode properties = entity.FirstChild;
                int dispHeight = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int picWidth = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int picHeight = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int speed = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                string spriteName = properties.InnerText;

                properties = properties.NextSibling;
                string[] dimension = properties.InnerText.Split(' ');

                ge = GameEntity.CreateLineTrack(Int32.Parse(dimension[0]), Int32.Parse(dimension[1]), Int32.Parse(dimension[2]), Int32.Parse(dimension[3]));
                ge.Props["DispHeight"] = dispHeight;
                ge.Props["PicWidth"] = picWidth;
                ge.Props["PicHeight"] = picHeight;
                ge.Props["Speed"] = speed;
                ge.Props["SpriteName"] = spriteName;
            }
            else if (type == "CONVEYBELT")
            {
                XmlNode properties = entity.FirstChild;
                int dispHeight = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int picWidth = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int picHeight = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int speed = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                bool clockwise = Boolean.Parse(properties.InnerText);

                properties = properties.NextSibling;
                string spriteName = properties.InnerText;

                properties = properties.NextSibling;
                string[] dimension = properties.InnerText.Split(' ');

                ge = GameEntity.CreateConveyBelt(Int32.Parse(dimension[0]), Int32.Parse(dimension[1]), Int32.Parse(dimension[2]), Int32.Parse(dimension[3]));
                ge.Props["DispHeight"] = dispHeight;
                ge.Props["PicWidth"] = picWidth;
                ge.Props["PicHeight"] = picHeight;
                ge.Props["Speed"] = speed;
                ge.Props["Clockwise"] = clockwise;
                ge.Props["SpriteName"] = spriteName;
            }
            else if (type == "LINESIN")
            {
                XmlNode properties = entity.FirstChild;
                int a = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int b = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int c = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int picWidth = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int picHeight = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                int speed = Int32.Parse(properties.InnerText);

                properties = properties.NextSibling;
                string spriteName = properties.InnerText;

                properties = properties.NextSibling;
                string[] dimension = properties.InnerText.Split(' ');

                ge = GameEntity.CreateLineSin(Int32.Parse(dimension[0]), Int32.Parse(dimension[1]), Int32.Parse(dimension[2]), Int32.Parse(dimension[3]));
                ge.Props["a"] = a;
                ge.Props["b"] = b;
                ge.Props["c"] = c;
                ge.Props["PicWidth"] = picWidth;
                ge.Props["PicHeight"] = picHeight;
                ge.Props["Speed"] = speed;
                ge.Props["SpriteName"] = spriteName;
            }

            return ge;
        }

        // Add new tab page
        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gameEntityLists.Insert(tabControl1.TabCount, new List<GameEntity>());

            string title = "TabPage " + (++tabCount).ToString();
            TabPage myTabPage = new TabPage(title);
            tabControl1.TabPages.Add(myTabPage);

        }

        // Close current tab page
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveMouseEvents(tabControl1.TabPages[tabControl1.SelectedIndex]);
            gameEntityLists.RemoveAt(tabControl1.SelectedIndex);

            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        // Delete the selected GameEntity
        private void delete_btn_Click(object sender, EventArgs e)
        {
            GameEntity ge = selectedObject_pg.SelectedObject as GameEntity;

            gameEntities_lb.Items.Remove(ge);
            gameEntities_lb.SelectedIndex = gameEntities_lb.Items.Count - 1;
            selectedObject_pg.SelectedObject = gameEntities_lb.SelectedItem;

            ge = null;

            RefreshAll();
        }

        // Performs a deep copy the selected GameEntity
        private void clone_btn_Click(object sender, EventArgs e)
        {
            GameEntity ge = selectedObject_pg.SelectedObject as GameEntity;
            EntityType entity = ge.Type;
            Rectangle bb = ge.GetBoundingBox();


            GameEntity geCopy = null;
            // Create new GameEntity
            if (entity == EntityType.RECT) {
                geCopy = GameEntity.CreateRectangle(bb.Location.Y , bb.Location.Y, bb.Width, bb.Height);
            }

            if (entity == EntityType.CIRCLE)
            {
                geCopy = GameEntity.CreateCircle(bb.Width, bb.Location);
            }

            if (entity == EntityType.SPRITE)
            {
                geCopy = GameEntity.CreateSprite(bb.Location.Y, bb.Location.Y, bb.Width, bb.Height);
            }

            if (entity == EntityType.LINETRACK)
            {
                geCopy = GameEntity.CreateLineTrack(bb.Location.Y, bb.Location.Y, bb.Width, bb.Height);
            }

            if (entity == EntityType.CONVEYBELT)
            {
                geCopy = GameEntity.CreateConveyBelt(bb.Location.Y, bb.Location.Y, bb.Width, bb.Height);
            }

            if (entity == EntityType.LINESIN)
            {
                geCopy = GameEntity.CreateLineSin(bb.Location.Y, bb.Location.Y, bb.Width, bb.Height);
            }

            geCopy.SetBoundingBox(bb); // Sets bounding box
            geCopy.Props = ge.Props; // Deep copy of properties

            // Adds the gameentity to the list of entities if not null
            if (geCopy != null)
            {
                gameEntities_lb.Items.Add(geCopy);
                gameEntities_lb.SelectedIndex = gameEntities_lb.Items.Count - 1;
                selectedObject_pg.SelectedObject = geCopy;
                RefreshAll();
            }
        }

        // Shift the gameentity forward in the list
        private void forward_btn_Click(object sender, EventArgs e)
        {
            if (gameEntities_lb.SelectedIndex > 0)
            {
                GameEntity ge = selectedObject_pg.SelectedObject as GameEntity;

                gameEntities_lb.SelectedIndex = gameEntities_lb.SelectedIndex - 1;
                GameEntity geSwap = selectedObject_pg.SelectedObject as GameEntity;

                gameEntities_lb.Items[gameEntities_lb.SelectedIndex] = ge;
                gameEntities_lb.SelectedIndex = gameEntities_lb.SelectedIndex + 1;

                gameEntities_lb.Items[gameEntities_lb.SelectedIndex] = geSwap;
                gameEntities_lb.SelectedIndex = gameEntities_lb.SelectedIndex - 1;

                selectedObject_pg.SelectedObject = gameEntities_lb.SelectedItem;

                RefreshAll();
            }
        }

        // Shift the gameentity backward in the list
        private void back_btn_Click(object sender, EventArgs e)
        {
            if (gameEntities_lb.SelectedIndex < gameEntities_lb.Items.Count - 1)
            {
                GameEntity ge = selectedObject_pg.SelectedObject as GameEntity;

                gameEntities_lb.SelectedIndex = gameEntities_lb.SelectedIndex + 1;
                GameEntity geSwap = selectedObject_pg.SelectedObject as GameEntity;

                gameEntities_lb.Items[gameEntities_lb.SelectedIndex] = ge;
                gameEntities_lb.SelectedIndex = gameEntities_lb.SelectedIndex - 1;

                gameEntities_lb.Items[gameEntities_lb.SelectedIndex] = geSwap;
                gameEntities_lb.SelectedIndex = gameEntities_lb.SelectedIndex + 1;

                selectedObject_pg.SelectedObject = gameEntities_lb.SelectedItem;

                RefreshAll();
            }
        }

        // Update Entity list on Tab change
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTabPanelEvents(tabControl1.TabPages[tabControl1.SelectedIndex]);

            for (int i = 0; i < gameEntityLists[tabControl1.SelectedIndex].Count; i++)
            {
                gameEntities_lb.Items.Add(gameEntityLists[tabControl1.SelectedIndex][i]);
            }

            gameEntityLists[tabControl1.SelectedIndex].Clear();
        }

        // Save Entity list on Tab change from the previous tab
        private void tabControl1_Deselected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedIndex >= 0)
            {
                RemoveMouseEvents(tabControl1.TabPages[tabControl1.SelectedIndex]);

                for (int i = 0; i < gameEntities_lb.Items.Count; i++)
                {
                    gameEntityLists[tabControl1.SelectedIndex].Add((GameEntity)gameEntities_lb.Items[i]);
                }

            }

            gameEntities_lb.Items.Clear();

        }
    }
}
