using System;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.TextFormatting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace BlockScheme
{
	/// <summary>
	/// Description of BlockOneWay.
	/// </summary>
	[Serializable()]
	public class BlockOneWay : BlockTransformation, ISerializable
	{
		//коэффицинт преобразования
		protected FormattedText TransRatioFormText;
		protected Point PointText;		
		
		#region методы	
		public BlockOneWay() {}
		
    	public BlockOneWay(Point topLeftPoint)
		{  		
    		TransRatioFormText = new FormattedText(
   	    		"",
   			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);
    		PointText = new Point();
    		
    		ChangeLocation(topLeftPoint, true);
		}	
 
   		public BlockOneWay(SerializationInfo info, StreamingContext ctxt)
   		{   			
      		this.PointText = (Point)info.GetValue("PointText",typeof(Point));
      		this.BlockSize = (Size)info.GetValue("BlockSize", typeof(Size));
      		this.ShapePoints = (Point[])info.GetValue("ShapePoints", typeof(Point[]));
      		this.PortPoints = (List<Point[]>)info.GetValue("PortPoints", typeof(List<Point[]>));
      		this.ConnectionObj = (List<Connection>)info.GetValue("ConnectionObj", typeof(List<Connection>));
			this.ConnectionEnd = (List<int>)info.GetValue("ConnectionEnd", typeof(List<int>));   

    		TransRatioFormText = new FormattedText(
   	    		(string)info.GetValue("TransRatioText", typeof(string)),
   			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);			
			
			this.Draw(false, false);			
   		}

   		public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("TransRatioText", this.TransRatioGet());
      		info.AddValue("PointText", this.PointText);
      		info.AddValue("BlockSize", this.BlockSize);
      		info.AddValue("ShapePoints", this.ShapePoints);
      		info.AddValue("PortPoints", this.PortPoints);
      		info.AddValue("ConnectionObj", this.ConnectionObj);
      		info.AddValue("ConnectionEnd", this.ConnectionEnd);
   		}
    	
		public void TransRatioSet(string text)
    	{
    		TransRatioFormText = new FormattedText(
    			text,
  			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);
    		
    		if (TransRatioFormText.Width > (BlockSize.Width - DTextWall))
   		    	BlockSize.Width = TransRatioFormText.Width + DTextWall;
    		else if (TransRatioFormText.Width < (BlockSize.Width - DTextWall))
    			if (TransRatioFormText.Width > (Block.BlockMinSize.Width - DTextWall))
    				BlockSize.Width = TransRatioFormText.Width + DTextWall;
    			else
    				BlockSize.Width = Block.BlockMinSize.Width;
				   			
			ChangeLocation(ShapePoints[0], true);    		
    	}		
		public string TransRatioGet()
		{
    		return TransRatioFormText.Text;
    	}    	
		
		public override void Draw(bool isSelected, bool isPortsVisible)
		{				
			using (DrawingContext dc = this.RenderOpen())			
			{   		
	        	if (isSelected) pen = SelectionPen;
    	        else pen = DrawingPen;
    	        dc.DrawRectangle(
    	        	DrawingBrush,
    	            pen,
    	            new Rect(ShapePoints[0], BlockSize));	   
	
    	        if (isPortsVisible)
    	        {
    	        	PortPointsEval();
    	        	if (ConnectionObj[0] == null)
    	            	dc.DrawRectangle(
		            		DrawingBrush,
    		            	DrawingPen,
    		            	new Rect(PortPoints[0][1], PortPoints[0][3]));
    	        	if (ConnectionObj[1] == null)
    	            	dc.DrawRectangle(
		                	DrawingBrush,
    		            	DrawingPen,
    		            	new Rect(PortPoints[1][1], PortPoints[1][3]));
				}				
    	        
                if (TransRatioFormText.Text != "")
                {
                	dc.DrawText(TransRatioFormText, PointText);
                }        
			}			
		}	

		protected override void ChangeTextLocation()
		{
        	PointText = new Point(
    			ShapePoints[0].X + BlockSize.Width / 2 - TransRatioFormText.Width / 2,
        		ShapePoints[0].Y + BlockSize.Height / 2 - TransRatioFormText.Height / 2);   
		}		
		#endregion
	}
}
