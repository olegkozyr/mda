using System;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace BlockScheme
{
	/// <summary>
	/// Description of BlockBidirectCounter.
	/// </summary>
	[Serializable()]
	public class BlockBidirectCounter : BlockSum, ISerializable
	{		
		private static readonly FormattedText BlockName = new FormattedText(
			"PC",
   			CultureInfo.GetCultureInfo("en-us"),
   			FlowDirection.LeftToRight,
   			new Typeface("TimesNewRoman"),
   			FontSize,
   			Brushes.Black);
    		
		private Point PointText;
		
		#region methods
		public BlockBidirectCounter(Point point) : base(point)
		{	
		}
		
		public BlockBidirectCounter(SerializationInfo info, StreamingContext ctxt) 
			: base(info, ctxt)
		{						
		}
		
		protected override void ReSerialization(SerializationInfo info, StreamingContext ctxt)
		{
			this.PointText = (Point)info.GetValue("PointText", typeof(Point));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("PointText", this.PointText);
			base.GetObjectData(info, ctxt);
		}
		
		public override void Draw(bool isSelected, bool isPortsVisible)
		{
			using (DrawingContext dc = this.RenderOpen())			
			{
				int i; 
					
	        	if (isSelected) pen = SelectionPen;
    	        else pen = DrawingPen;
    	        dc.DrawRectangle(
    	        	DrawingBrush,
    	            pen,
    	            new Rect(ShapePoints[0], BlockSize));	   
    	        
    	        if (isPortsVisible)
    	        {
    	        	for (i = 0; i < PortPoints.Count; i++)
    	        	{
    	        		if (ConnectionObj[i] == null)
    	        		{
    	        			PortPointsEval();
	    		            dc.DrawRectangle(
				            	DrawingBrush,
    				            DrawingPen,
    				            new Rect(PortPoints[i][1], PortPoints[i][3]));    	        			
    	        		}
    	        	}
				}				    	            	       
    	        
    	        dc.DrawText(BlockName, PointText);
    	        
    	        for (i = 1; i < PortPoints.Count; i++)
    	        {    	        	
    	        	dc.DrawText(InPortSigns[i - 1], new Point(PortPoints[i][0].X + SignDelta, 
    	        		PortPoints[i][0].Y - InPortSigns[i - 1].Height / 2));
    	        }
			}			
		}        			

		protected void ChangeTextLocation()
		{
        	PointText = new Point(
    			ShapePoints[0].X + BlockSize.Width / 2 - BlockName.Width / 2,
        		ShapePoints[0].Y + BlockSize.Height / 2 - BlockName.Height / 2);   
		}

		public override void ChangeLocation(Point topLeftPoint, bool isSelected)
		{
			int i;
			
        	ShapePoints[0] = topLeftPoint;
        	ShapePoints[1] = new Point(ShapePoints[0].X + BlockSize.Width, ShapePoints[0].Y);        	
        	ChangePortCenterLocation();
			ChangeTextLocation();
				
        	Draw(isSelected, false);
        	
        	for (i = 0; i < PortPoints.Count; i++)
        	{
        		if (ConnectionObj[i] != null)
        		{
        			ConnectionObj[i].CurrentEnd = ConnectionEnd[i];
        			ConnectionObj[i].ChangeLocation(PortPoints[i][0], false);
        		}
        	}        	        	
		}
		#endregion		
	}
}
