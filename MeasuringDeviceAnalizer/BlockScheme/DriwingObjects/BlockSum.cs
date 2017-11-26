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
	/// Description of BlockSum.
	/// </summary>
	[Serializable()]
	public class BlockSum : BlockMultiConnection, ISerializable
	{		
		protected static readonly int MinLeftPortCount = 2;
		protected static readonly Size BlockSumMinSize = new Size(45, 45);		
		protected static readonly double SignDelta = 2.5; //отступ знаков от левого края		
		
		//знаки сигналов
		//к-во знаков на 1-цу меньше к-ва портов	
		protected List<FormattedText> InPortSigns; // форматированный текст				

        //temp
        protected int intTemp;
        protected int textLength;
        protected string tempText;
        protected double doubleTemp;
        
        #region методы
        public BlockSum() {}
        
        public BlockSum(Point leftTopPoint)
		{
        	ShapePoints[0] = leftTopPoint;
        	
        	BlockSize = BlockSumMinSize;       	
        	InPortSigns = new List<FormattedText>();
				
        	PortPoints.RemoveAt(1);
        	ConnectionObj.RemoveAt(1);
        	ConnectionEnd.RemoveAt(1);        		
        	
        	PortDeltaMax = BlockSumMinSize.Height / 3;
        	PortDelta = FontSize; //отступ знаков от верхнего края
        	
			SetInPortSigns("++");			
		}		

   		public BlockSum(SerializationInfo info, StreamingContext ctxt)
   		{        	
      		this.BlockSize = (Size)info.GetValue("BlockSize", typeof(Size));
      		this.ShapePoints = (Point[])info.GetValue("ShapePoints", typeof(Point[]));
      		this.PortPoints = (List<Point[]>)info.GetValue("PortPoints", typeof(List<Point[]>));
      		this.ConnectionObj = (List<Connection>)info.GetValue("ConnectionObj", typeof(List<Connection>));
			this.ConnectionEnd = (List<int>)info.GetValue("ConnectionEnd", typeof(List<int>));   
			this.PortDeltaMax = (double)info.GetValue("PortDeltaMax", typeof(double)); 
			this.PortDelta = (double)info.GetValue("PortDelta", typeof(double)); 		

   			InPortSigns = new List<FormattedText>();
        	string text = (string)info.GetValue("InPortSigns", typeof(string));
        	for (int i = 0; i < text.Length; i++ )
			{
				InPortSigns.Add(new FormattedText(
					text[i].ToString(),
			   	    CultureInfo.GetCultureInfo("en-us"),
			   		FlowDirection.LeftToRight,
			   		new Typeface("TimesNewRoman"),
	   			    FontSize,
	   			    Brushes.Black));
        	}
			
        	ReSerialization(info, ctxt);
        	
			this.Draw(false, false);
   		}
   		
		protected virtual void ReSerialization(SerializationInfo info, StreamingContext ctxt)
		{
			
		}
   		
   		public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{   			
      		info.AddValue("InPortSigns", this.GetInPortSigns());
      		info.AddValue("BlockSize", this.BlockSize);
      		info.AddValue("ShapePoints", this.ShapePoints);
      		info.AddValue("PortPoints", this.PortPoints);
      		info.AddValue("ConnectionObj", this.ConnectionObj);
      		info.AddValue("ConnectionEnd", this.ConnectionEnd);
      		info.AddValue("PortDeltaMax", this.PortDeltaMax);
      		info.AddValue("PortDelta", this.PortDelta);
   		}        
        
		public void SetInPortSigns(string text)
		{
			int i;
			if (InPortSigns.Count != text.Length)
			{
				if (InPortSigns.Count < text.Length)
				{
					textLength = InPortSigns.Count;
					
					for (i = InPortSigns.Count; i < text.Length; i++ )
					{
						InPortSigns.Add(new FormattedText(
							text[i].ToString(),
			   			    CultureInfo.GetCultureInfo("en-us"),
			   			    FlowDirection.LeftToRight,
			   			    new Typeface("TimesNewRoman"),
			   			    FontSize,
			   			    Brushes.Black));
						PortPoints.Add(new Point[PortPointCount]);	
						ConnectionObj.Add(null);
    	    			ConnectionEnd.Add(-1);
					}	
				}
				else
				{
					textLength = text.Length;
					if (text.Length < MinLeftPortCount)
						intTemp = MinLeftPortCount;
					else
						intTemp = text.Length;
					
					for (i = InPortSigns.Count; i > intTemp; i--)
					{
						InPortSigns.RemoveAt(i - 1);	
						PortPoints.RemoveAt(i);	
						ClearConnection(i, -1);						
						ConnectionObj.RemoveAt(i);
						ConnectionEnd.RemoveAt(i);						
					}
				}					
				
				ChangeLocation(ShapePoints[0], true);
			}
			else
			{
				textLength = InPortSigns.Count;
			}			

			for (i = 0; i < textLength; i++)
			{
				if (InPortSigns[i].Text != text[i].ToString())
				{					
					InPortSigns[i] = new FormattedText(
						text[i].ToString(),
		   			    CultureInfo.GetCultureInfo("en-us"),
		   			    FlowDirection.LeftToRight,
		   			    new Typeface("TimesNewRoman"),
		   			    FontSize,
		   			    Brushes.Black);						
				}								
			}			
			
			this.Draw(true, false);			
		}
		
		public string GetInPortSigns()
		{
			int i;
			tempText = "";
			for (i = 0; i < InPortSigns.Count; i++)
				tempText += InPortSigns[i].Text;
			return tempText;
		}			

		protected void ChangePortCenterLocation()
		{
			int i;
			
			//пересчет центральных точек портов
			doubleTemp = BlockHeightChange();
				
			PortPoints[0][0] = new Point(ShapePoints[1].X, ShapePoints[1].Y + BlockSize.Height / 2);
			for (i = 1; i < PortPoints.Count; i++)
			{
				PortPoints[i][0] = new Point(ShapePoints[0].X, ShapePoints[0].Y + doubleTemp * i);
			}
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
    	        
    	        for (i = 1; i < PortPoints.Count; i++)
    	        {    	        	
    	        	dc.DrawText(InPortSigns[i - 1], new Point(PortPoints[i][0].X + SignDelta, 
    	        		PortPoints[i][0].Y - InPortSigns[i - 1].Height / 2));
    	        }
			}			
		}        		
		
		public override void ChangeLocation(Point topLeftPoint, bool isSelected)
		{
			int i;
			
        	ShapePoints[0] = topLeftPoint;
        	ShapePoints[1] = new Point(ShapePoints[0].X + BlockSize.Width, ShapePoints[0].Y);        	
        	ChangePortCenterLocation();

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
