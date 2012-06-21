using System;
using System.Drawing;
namespace Netron.Lithium
{
	/// <summary>
	/// A simple rectangular shape
	/// </summary>
	public class SimpleRectangle : ShapeBase
	{
		#region Fields
		string plus = "";
				
		#endregion

		#region Constructor
		/// <summary>
		/// Default ctor
		/// </summary>
		/// <param name="s"></param>
		public SimpleRectangle(LithiumControl s) : base(s)
		{
			
		}
		#endregion

		#region Methods
		/// <summary>
		/// Tests whether the mouse hits this shape
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public override bool Hit(System.Drawing.Point p)
		{
			Rectangle r= new Rectangle(p, new Size(5,5));
			return rectangle.Contains(r);			
		}



		/// <summary>
		/// Paints the shape on the canvas
		/// </summary>
		/// <param name="g"></param>
		public override void Paint(System.Drawing.Graphics g)
		{
			SetBrush();				
//			//the shadow effect
//			Rectangle shadowRec = rectangle;
//			shadowRec.Offset(5,5);
//			g.FillRectangle(Brushes.Gainsboro,shadowRec);

			g.FillRectangle(shapeBrush,rectangle);
			
			if(hovered || isSelected)
				g.DrawRectangle(new Pen(Color.Red,2F),rectangle);
			else
				g.DrawRectangle(blackPen,rectangle);
			//add the amount of children
			if(childNodes.Count>0) plus = " [" + childNodes.Count + "]";
			else plus = "";
		
			if(text !=string.Empty)
				g.DrawString(text + plus,font,Brushes.Black, rectangle.X+5,rectangle.Y+5);
		}

		/// <summary>
		/// Invalidates the shape
		/// </summary>
		public override void Invalidate()
		{
			Rectangle r = rectangle;
			r.Offset(-5,-5);
			r.Inflate(40,40);
			site.Invalidate(r);
		}

		public override void Resize(int width, int height)
		{
			base.Resize(width,height);		
			Invalidate();
		}

		#endregion	
	}
}
