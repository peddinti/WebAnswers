using System;
using System.Drawing;
namespace Netron.Lithium
{
	/// <summary>
	/// Represents an endpoint of a connection or a location of a shape to which
	/// a connection can be attached
	/// </summary>
	public class Connector : Entity
	{

		#region Fields
		/// <summary>
		/// the location of this connector
		/// </summary>
		protected Point point;
		/// <summary>
		/// the connectors attached to this connector
		/// </summary>
		protected ConnectorCollection attachedConnectors;
		/// <summary>
		/// the connector, if any, to which this connector is attached to
		/// </summary>
		protected Connector attachedTo;
		/// <summary>
		/// the name of this connector
		/// </summary>
		protected string name;
		#endregion

		#region Properties

		/// <summary>
		/// The name of this connector
		/// </summary>
		public string Name
		{
			get{return name;}
			set{name = value;}
		}

		/// <summary>
		/// If the connector is attached to another connector
		/// </summary>
		public Connector AttachedTo
		{		
			get{return attachedTo;}
			set{attachedTo = value;}
		}

		/// <summary>
		/// The location of this connector
		/// </summary>
		public Point Point
		{
			get{return point;}
			set{point = value;}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default connector
		/// </summary>
		public Connector()
		{
			attachedConnectors = new ConnectorCollection();
		}

		/// <summary>
		/// Constructs a connector, passing its location
		/// </summary>
		/// <param name="p"></param>
		public Connector(Point p)
		{
			attachedConnectors = new ConnectorCollection();
			point = p;
		}

		#endregion

		#region Methods
		/// <summary>
		/// Paints the connector on the canvas
		/// </summary>
		/// <param name="g"></param>
		public override void Paint(Graphics g)
		{
			if(hovered)
				g.FillRectangle(Brushes.Red,point.X-5,point.Y-5,10,10);
			else
				g.FillRectangle(Brushes.WhiteSmoke,point.X-2,point.Y-2,4,4);

//			this is useful when debugging, but annoying otherwise (though you might like it)			
//			if(name!=string.Empty)
//				g.DrawString(name,new Font("verdana",10),Brushes.Black,point.X+7,point.Y+4);
		}

		/// <summary>
		/// Tests if the mouse hits this connector
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public override bool Hit(Point p)
		{
			Point a = p;
			Point b  = point;
			b.Offset(-7,-7);
			//a.Offset(-1,-1);
			Rectangle r = new Rectangle(a,new Size(0,0));
			Rectangle d = new Rectangle(b, new Size(15,15));
			return d.Contains(r);
		}

		/// <summary>
		/// Invalidates the connector
		/// </summary>
		public override void Invalidate()
		{
			Point p = point;
			p.Offset(-5,-5);
			site.Invalidate(new Rectangle(p,new Size(10,10)));
		}

		/// <summary>
		/// Moves the connector with the given shift-vector
		/// </summary>
		/// <param name="p"></param>
		public override void Move(Point p)
		{
			this.point.X += p.X;
			this.point.Y += p.Y;
			for(int k=0; k<attachedConnectors.Count;k++)
				attachedConnectors[k].Move(p);
		}

		/// <summary>
		/// Attaches the given connector to this connector
		/// </summary>
		/// <param name="c"></param>
		public void AttachConnector(Connector c)
		{
			//remove from the previous, if any
			if(c.attachedTo!=null)
			{
				c.attachedTo.attachedConnectors.Remove(c);
			}
			attachedConnectors.Add(c);
			c.attachedTo=this;

		}

		/// <summary>
		/// Detaches the given connector from this connector
		/// </summary>
		/// <param name="c"></param>
		public void DetachConnector(Connector c)
		{
			attachedConnectors.Remove(c);
		}
		
		/// <summary>
		/// Releases this connector from any other
		/// </summary>
		public void Release()
		{
			if(this.attachedTo!=null)
			{
				this.attachedTo.attachedConnectors.Remove(this);
				this.attachedTo = null;
			}

		}

		#endregion
	}
}
