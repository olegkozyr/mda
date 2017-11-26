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
	/// Description of BlockSwitch.
	/// </summary>
	[Serializable()]
	public class BlockSwitch : BlockMultiConnection, ISerializable
	{		
		protected static readonly int MinPortCount = 1;				
		protected static readonly Size BlockSwitchMinSize = new Size(45, 45);				
				
		//к-во портов:
		//если слева 1 порт - справа может быть сколько угодно, и наоборот
		protected int[] PortCount; //к-во портов		 

		//внутренние точки линий изображающих переключатель; к-во = к-ву портов
		protected List<Point> InLinePoints;			

        //temp
        private int intTemp;
        private double doubleTemp;
        
        #region методы
        public BlockSwitch() {}
        
        public BlockSwitch(Point leftTopPoint)
		{
        	PortDeltaMax = BlockSwitchMinSize.Height / PortPoints.Count;
        	PortDelta = BlockPortHalfSize.Height * 2 + 5; //отступ знаков от верхнего края	       	
        	BlockSize = BlockSwitchMinSize;       	     					
        	InLinePoints = new List<Point>(); 
        	
        	PortCount = new int[SideCount];
        	PortCount[0] = 1;
        	PortCount[1] = 1;
        	
        	InLinePoints.Add(new Point());
        	InLinePoints.Add(new Point());
        	
        	ChangeLocation(leftTopPoint, true);
		}		

   		public BlockSwitch(SerializationInfo info, StreamingContext ctxt)
   		{
      		this.BlockSize = (Size)info.GetValue("BlockSize", typeof(Size));
      		this.ShapePoints = (Point[])info.GetValue("ShapePoints", typeof(Point[]));
      		this.PortCount = (int[])info.GetValue("PortCount", typeof(int[]));
      		this.PortPoints = (List<Point[]>)info.GetValue("PortPoints", typeof(List<Point[]>));
      		this.InLinePoints = (List<Point>)info.GetValue("InLinePoints",typeof(List<Point>));
      		this.ConnectionObj = (List<Connection>)info.GetValue("ConnectionObj", typeof(List<Connection>));
			this.ConnectionEnd = (List<int>)info.GetValue("ConnectionEnd", typeof(List<int>));   
			this.PortDeltaMax = (double)info.GetValue("PortDeltaMax", typeof(double)); 
			this.PortDelta = (double)info.GetValue("PortDelta", typeof(double)); 

			this.Draw(false, false);			
   		}

   		public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("BlockSize", this.BlockSize);
      		info.AddValue("ShapePoints", this.ShapePoints);
      		info.AddValue("PortCount", this.PortCount);
      		info.AddValue("PortPoints", this.PortPoints);
      		info.AddValue("InLinePoints", this.InLinePoints);
      		info.AddValue("ConnectionObj", this.ConnectionObj);
      		info.AddValue("ConnectionEnd", this.ConnectionEnd);
      		info.AddValue("PortDeltaMax", this.PortDeltaMax);
      		info.AddValue("PortDelta", this.PortDelta);
   		}
        
        public void SetPortCount(int[] newPortCount)
        {
        	int i;        	
        	int ex = 0;
        	
        	//левые порты
			if (PortCount[0] != newPortCount[0])
    		{
        		ex = 1;
   				if (PortCount[0] < newPortCount[0])
    			{
  		    		for (i = PortCount[0]; i < newPortCount[0]; i++)
		    		{
        				PortPoints.Insert(i, new Point[PortPointCount]);         	      
        				InLinePoints.Insert(i, new Point());
	    				ConnectionObj.Insert(i, null);
			    	    ConnectionEnd.Insert(i, -1);    		    				
		        	}    	    				
   	    		}
				else
				{													
					intTemp = newPortCount[0] - 1;						
	   		    	for (i = PortCount[0] - 1; i > intTemp; i--)
   			    	{
    	    			PortPoints.RemoveAt(i);         	      
    	    			InLinePoints.RemoveAt(i);
    	    			ClearConnection(i, -1);	
   		    			ConnectionObj.RemoveAt(i);
				    	ConnectionEnd.RemoveAt(i);				    	    	
		        	}  
				}
				
				PortCount[0] = newPortCount[0];
   	    	}
       		
			//правые порты			
      		if (PortCount[1] != newPortCount[1])
   	    	{
      			ex = 1;
       			if (PortCount[1] < newPortCount[1])
   	    		{
   		    		for (i = PortCount[1]; i < newPortCount[1]; i++)
		    		{
        				PortPoints.Add(new Point[PortPointCount]);         	      
        				InLinePoints.Add(new Point());
	    				ConnectionObj.Add(null);
			    	    ConnectionEnd.Add(-1);    		    				
		        	}    	    				
   	    		}
				else
				{													
					intTemp = newPortCount[1] - 1;
   		    		for (i = PortCount[1] + PortCount[0] - 1; i > intTemp + PortCount[0]; i--)
		    		{
   	    				PortPoints.RemoveAt(i);         	      
   	    				InLinePoints.RemoveAt(i);
   	    				ClearConnection(i);	
	    				ConnectionObj.RemoveAt(i);
			    	    ConnectionEnd.RemoveAt(i);				    	    	
	        		} 
				}
				
				PortCount[1] = newPortCount[1];
   	    	} 

      		if (ex == 1)
      			ChangeLocation(ShapePoints[0], true);
        }
        
        public int[] GetPortCount()
        {
        	int[] portCount = new int[2];
        	portCount[0] = PortCount[0];
        	portCount[1] = PortCount[1];
        	return portCount;
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
    	        
    	        for (i = 0; i < PortCount[0]; i++)
    	        {    	        	
    	        	dc.DrawLine(DrawingPen, PortPoints[i][0], InLinePoints[i]);
    	        }
    	        
    	        for (i = PortCount[0]; i < PortCount[0] + PortCount[1]; i++)
    	        {    	        	
    	        	dc.DrawLine(DrawingPen, PortPoints[i][0], InLinePoints[i]);
    	        }
    	        
    	        if (PortCount[0] > PortCount[1])
    	        {
    	        	dc.DrawLine(DrawingPen, InLinePoints[PortCount[0] + PortCount[1] - 1], InLinePoints[0]);
    	        }
    	        else if (PortCount[0] < PortCount[1])
    	        {
    	        	dc.DrawLine(DrawingPen, InLinePoints[0], InLinePoints[PortCount[0]]);
    	        }
    	        else
    	        {
    	        	dc.DrawLine(DrawingPen, InLinePoints[0], new Point(InLinePoints[1].X, InLinePoints[1].Y 
    	        	                                                  - BlockSize.Height / 4));
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
		
		private void ChangePortCenterLocation()
		{
			int i;
			//пересчет центральных точек портов
			doubleTemp = BlockHeightChange();
			
			if (PortCount[0] > PortCount[1])
			{
				PortPoints[PortCount[0]][0] = new Point(ShapePoints[1].X, ShapePoints[1].Y + BlockSize.Height / 2);
				InLinePoints[PortCount[0]] = new Point(PortPoints[PortCount[0]][0].X - BlockSize.Width / 3, 
				                                          PortPoints[PortCount[0]][0].Y);
				for (i = 0; i < PortCount[0]; i++)
				{
					PortPoints[i][0] = new Point(ShapePoints[0].X, ShapePoints[0].Y + doubleTemp * (i + 1));
					InLinePoints[i] = new Point(PortPoints[i][0].X + BlockSize.Width / 3, 
				                                          PortPoints[i][0].Y);
				}
			}
			else if (PortCount[0] < PortCount[1])
			{
				PortPoints[0][0] = new Point(ShapePoints[0].X, ShapePoints[0].Y + BlockSize.Height / 2);
				InLinePoints[0] = new Point(PortPoints[0][0].X + BlockSize.Width / 3, 
				                                          PortPoints[0][0].Y);
				for (i = PortCount[0]; i < PortCount[0] + PortCount[1]; i++)
				{
					PortPoints[i][0] = new Point(ShapePoints[1].X, ShapePoints[1].Y + doubleTemp * i);
					InLinePoints[i] = new Point(PortPoints[i][0].X - BlockSize.Width / 3, 
				                                          PortPoints[i][0].Y);
				}				
			}
			else
			{
				PortPoints[0][0] = new Point(ShapePoints[0].X, ShapePoints[0].Y + BlockSize.Height / 2);
				InLinePoints[0] = new Point(PortPoints[0][0].X + BlockSize.Width / 3, 
				                                          PortPoints[0][0].Y);
				PortPoints[1][0] = new Point(ShapePoints[1].X, ShapePoints[1].Y + BlockSize.Height / 2);
				InLinePoints[1] = new Point(PortPoints[1][0].X - BlockSize.Width / 3, 
				                                          PortPoints[1][0].Y); 
			}				
		}							
		#endregion 		
	}
}
