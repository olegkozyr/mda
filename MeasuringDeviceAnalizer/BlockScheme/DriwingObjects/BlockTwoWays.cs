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
	[Serializable()]
    class BlockTwoWays : BlockTransformation, ISerializable
    {
    	public static readonly int TransRatioCount = 2;
        //коэффицинты преобразования
        private FormattedText[] TransRatioFormText;
        //точки начала текста внутри блока
        
        private double tempWidth;
        
        public Point[] PointText;
        
        #region методы
        public BlockTwoWays() {}
        
        public BlockTwoWays(Point topLeftPoint)
        {
        	TransRatioFormText = new FormattedText[TransRatioCount];
        	TransRatioFormText[0] = new FormattedText(
   	    			"",
   			    	CultureInfo.GetCultureInfo("en-us"),
   			        FlowDirection.LeftToRight,
   			        new Typeface("TimesNewRoman"),
   			        FontSize,
   			        Brushes.Black);
        	TransRatioFormText[1] = new FormattedText(
   	    			"",
   			    	CultureInfo.GetCultureInfo("en-us"),
   			        FlowDirection.LeftToRight,
   			        new Typeface("TimesNewRoman"),
   			        FontSize,
   			        Brushes.Black);
        	
        	PointText = new Point[TransRatioCount];
        	ChangeLocation(topLeftPoint, true);        	
        }       

   		public BlockTwoWays(SerializationInfo info, StreamingContext ctxt)
   		{   			   			
      		this.PointText = (Point[])info.GetValue("PointText",typeof(Point[]));
      		this.BlockSize = (Size)info.GetValue("BlockSize", typeof(Size));
      		this.ShapePoints = (Point[])info.GetValue("ShapePoints", typeof(Point[]));
      		this.PortPoints = (List<Point[]>)info.GetValue("PortPoints", typeof(List<Point[]>));
      		this.ConnectionObj = (List<Connection>)info.GetValue("ConnectionObj", typeof(List<Connection>));
			this.ConnectionEnd = (List<int>)info.GetValue("ConnectionEnd", typeof(List<int>)); 
   			 			
        	TransRatioFormText = new FormattedText[TransRatioCount];
        	TransRatioFormText[0] = new FormattedText(
   	    			(string)info.GetValue("TransRatioText0", typeof(string)),
   			    	CultureInfo.GetCultureInfo("en-us"),
   			        FlowDirection.LeftToRight,
   			        new Typeface("TimesNewRoman"),
   			        FontSize,
   			        Brushes.Black);
        	TransRatioFormText[1] = new FormattedText(
   	    			(string)info.GetValue("TransRatioText1", typeof(string)),
   			    	CultureInfo.GetCultureInfo("en-us"),
   			        FlowDirection.LeftToRight,
   			        new Typeface("TimesNewRoman"),
   			        FontSize,
   			        Brushes.Black);	

			this.Draw(false, false);        	
   		}

   		public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
   			info.AddValue("TransRatioText0", this.TransRatioGet(0));
      		info.AddValue("TransRatioText1", this.TransRatioGet(1));
      		info.AddValue("PointText", this.PointText);
      		info.AddValue("BlockSize", this.BlockSize);
      		info.AddValue("ShapePoints", this.ShapePoints);
      		info.AddValue("PortPoints", this.PortPoints);
      		info.AddValue("ConnectionObj", this.ConnectionObj);
      		info.AddValue("ConnectionEnd", this.ConnectionEnd);
   		}
        
		public void TransRatioSet(string text, int index)
    	{
			if ((index >= 0) && (index < PortCount))
    		{
    			TransRatioFormText[index] = new FormattedText(
   	    			text,
   			    	CultureInfo.GetCultureInfo("en-us"),
   			        FlowDirection.LeftToRight,
   			        new Typeface("TimesNewRoman"),
   			        FontSize,
   			        Brushes.Black);    			    			
    		}
			
    		if (TransRatioFormText[index].Width > (BlockSize.Width - DTextWall))
   		    	BlockSize.Width = TransRatioFormText[index].Width + DTextWall;
    		else 
    		{
    			if (TransRatioFormText[0].Width > TransRatioFormText[1].Width)
    				tempWidth = TransRatioFormText[0].Width;
    			else
    				tempWidth = TransRatioFormText[1].Width;
    			
    			if (tempWidth < (BlockSize.Width - DTextWall))
    				if (tempWidth > (Block.BlockMinSize.Width - DTextWall))
    					BlockSize.Width = tempWidth + DTextWall;
    				else
    					BlockSize.Width = Block.BlockMinSize.Width;			
    		}
    		
    		ChangeLocation(ShapePoints[0], true);
    	}	
		
		public string TransRatioGet(int index)
		{
			if ((index >= 0) && (index < PortCount))
			{
				return TransRatioFormText[(int)index].Text;
			}
			else
			{
				MessageBox.Show("BlockTwoWays, TransRatioGet(), index is out of range");				
				return "";
			}
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
				dc.DrawLine(pen, PortPoints[0][0], PortPoints[1][0]);
    	        
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
    	        
    	        if (TransRatioFormText[0].Text != "")
                {
    	        	dc.DrawText(TransRatioFormText[0], PointText[0]);
                }  		
    	        if (TransRatioFormText[1].Text != "")
                {
    	        	dc.DrawText(TransRatioFormText[1], PointText[1]);
                }  	                
			}			
		}		
		
		protected override void ChangeTextLocation()
		{
			int i;
			
			for (i = 0; i < ShapePoints.Length; i++)
	    		PointText[i] = new Point(
    				ShapePoints[0].X + BlockSize.Width / 2 - TransRatioFormText[i].Width / 2,
    				ShapePoints[0].Y + BlockSize.Height / 4 * (1 + 2*i) - TransRatioFormText[i].Height / 2);
		}
		#endregion
    }    
}
