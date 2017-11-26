using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Media.TextFormatting;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlockScheme
{
	[Serializable()]
    public class Connection : ShapeBase, ISerializable
    {
    	#region filds
    	#region static
    	public static readonly int EndCount = 2;
    	public static readonly int ConPointCount = 4;
    	protected static readonly int LineSelPointCount = 6;
        protected static readonly double LineHalfThickness = 4;
        protected static readonly double EndCircleRadius = 5;
        protected static readonly double PenThickness = 1;
        protected static readonly Pen LineSelectedPen = new Pen() 
        {
            Brush = Brushes.DarkCyan, 
            DashStyle = DashStyles.Dash,
            Thickness = PenThickness,
        };
        protected static readonly Pen LinePen = new Pen(Brushes.Black, PenThickness);
        protected static readonly Pen LineExternalPen = new Pen(Brushes.Green, PenThickness);   
		protected static readonly Brush background = Brushes.Transparent;  
		protected static readonly int FontSize = 14;         		        		
		#endregion
		
        //shape
        protected Point[] ConnectionPoints;        
        protected Point[] LineSelectPoints;
        protected Point PointText;

        //соединение с блоком
        protected ShapeBase[] EndShape;
        protected int[] EndPortNum;	       
        //соединение с соединением
        //данные соединения к которому подсоединяют
        protected int EndConNode;
        public int EndConNodeNum;
        
        //данные соединений которые подсоединяют
        protected List<Connection>[] ConConnection;
        protected List<int>[] ConEnd;
        
        //сигнал
        protected FormattedText Signal;	
        public bool? IsSignalInput = false;
        	
        protected int currentEnd; 
        public int CurrentEnd
        {
        	set 
        	{
        		if ((value > -1) && (value < EndCount))
        			currentEnd = value;
        		else
        			currentEnd = -1;
        	}
        	
        	get {return currentEnd;}
        }        
		protected int CurrentPointNum;						
		public bool IsCreating;	

        protected int i;
        protected Pen pen;  
        protected Vector minOffset; 
		protected Vector clickOffset;		
        #endregion

		#region	constructors
        public Connection() {}
        
        public Connection(Point point, ShapeBase shape, int portNum)
        	: this(point, shape, portNum, -1, -1)
        {}

        public Connection(Point point, ShapeBase shape, int node, int nodeNum)
        	: this(point, shape, -1, node, nodeNum)
        {}        
        
        public Connection(Point point, ShapeBase shape, int portNum, int node, int nodeNum)
        {        	
            ConnectionPoints = new Point[ConPointCount];
        	LineSelectPoints = new Point[LineSelPointCount];
        	PointText = new Point();        	
        	EndShape = new ShapeBase[EndCount];        	
        	EndPortNum = new int[EndCount];   
			
        	//узлов - 2
        	ConConnection = new List<Connection>[2];
        	ConConnection[0] = new List<Connection>();
        	ConConnection[1] = new List<Connection>();
        	ConEnd = new List<int>[2];
        	ConEnd[0] = new List<int>();
        	ConEnd[1] = new List<int>();
        	
        	EndShape[0] = shape;
			EndPortNum[0] = portNum; 
			EndConNode = node;	
			EndConNodeNum = nodeNum;		
                      	
	        for (int i = 0; i < ConPointCount; i++)
    	    {
   	        	ConnectionPoints[i] = point;
   	        }

   	        for (int i = 0; i < LineSelPointCount; i++)
   	        {
   	            LineSelectPoints[i] = point;
   	        }
  	        
   	        CurrentPointNum = -1; 
            CurrentEnd = 1;  
			IsCreating = true;			
			
			if (shape is Connection)
				SetSignal(((Connection)shape).GetSignal(), false, -1);
			else
				SetSignal("", false, -1);			
        }
        
   		public Connection(SerializationInfo info, StreamingContext ctxt)
   		{
      		this.ConnectionPoints = (Point[])info.GetValue("ConnectionPoints", typeof(Point[]));
      		this.LineSelectPoints = (Point[])info.GetValue("LineSelectPoints",typeof(Point[]));
      		this.PointText = (Point)info.GetValue("PointText", typeof(Point));
      		this.EndShape = (ShapeBase[])info.GetValue("EndShape", typeof(ShapeBase[]));
      		this.EndPortNum = (int[])info.GetValue("EndPortNum", typeof(int[]));
      		this.EndConNode = (int)info.GetValue("EndConNode", typeof(int));
      		this.EndConNodeNum = (int)info.GetValue("EndConNodeNum", typeof(int));
			this.ConConnection = (List<Connection>[])info.GetValue("ConConnection", typeof(List<Connection>[]));      		      		
			this.ConEnd = (List<int>[])info.GetValue("ConEnd", typeof(List<int>[]));			
			
    		this.Signal = new FormattedText(
   	    		(string)info.GetValue("Signal", typeof(string)),
   			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);				
			
			this.IsSignalInput = (bool?)info.GetValue("IsSignalInput", typeof(bool?));
			this.Draw(false);
   		}        
        #endregion
   		
        #region методы  
   		public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   		{
      		info.AddValue("ConnectionPoints", this.ConnectionPoints);
      		info.AddValue("LineSelectPoints", this.LineSelectPoints);
      		info.AddValue("PointText", this.PointText);
      		info.AddValue("EndShape", this.EndShape);
      		info.AddValue("EndPortNum", this.EndPortNum);
      		info.AddValue("EndConNode", this.EndConNode);
      		info.AddValue("EndConNodeNum", this.EndConNodeNum);
      		info.AddValue("ConConnection", this.ConConnection);
      		info.AddValue("ConEnd", this.ConEnd);
      		info.AddValue("Signal", this.GetSignal());
      		info.AddValue("IsSignalInput", this.IsSignalInput);
   		}        

		public override bool IsConnected()
		{
			if ((EndShape[0] == null) && (EndShape[1] == null))
				return false;
			
			return true;
		}
   		
        public Part Dragging(Point point)
        {
        	if (EndShape[0] == null)
        	{
	        	clickOffset = ConnectionPoints[0] - point;
	   	        if (clickOffset.Length < EndCircleRadius)
	   	        {   	        	
	   	        	CurrentEnd = 0;   	
					return Part.End;   	        	
	   	        }
        	}

        	if (EndShape[1] == null)
        	{
	        	clickOffset = ConnectionPoints[3] - point;
	   	        if (clickOffset.Length < EndCircleRadius)
	   	        {   	        	
	   	        	CurrentEnd = 1;   	
					return Part.End;   	        	
	   	        }
        	}  
        	
        	if ((Math.Abs(ConnectionPoints[1].Y - point.Y) < Math.Abs(ConnectionPoints[1].Y - ConnectionPoints[2].Y)) &&
        	    (Math.Abs(ConnectionPoints[2].Y - point.Y) < Math.Abs(ConnectionPoints[1].Y - ConnectionPoints[2].Y)) &&
        	    (Math.Abs(ConnectionPoints[1].X - point.X) <= LineHalfThickness))
        	{        		
	   	        CurrentPointNum = 1; 
				clickOffset = ConnectionPoints[CurrentPointNum] - point;	   	        
				return Part.Middle;        		
        	}
   	        
        	return Part.None;
        }
        
        public Point GetConnectionPoints(int index)
        {
        	if ((index > -1) && (index < ConPointCount))
        		return ConnectionPoints[index];
        	
        	return new Point(-1, -1);
        }
        
        public void ChangeLocation(Point point)
        {
        	if (CurrentPointNum == 1)
        	{
        		ConnectionPoints[CurrentPointNum].X = point.X + clickOffset.X;
   	            ConnectionPoints[CurrentPointNum + 1].X = ConnectionPoints[CurrentPointNum].X;       

    	        LineSelectPoints[1] = new Point(ConnectionPoints[1].X, ConnectionPoints[1].Y + LineHalfThickness);
    	        LineSelectPoints[2] = new Point(ConnectionPoints[1].X + LineHalfThickness, ConnectionPoints[1].Y);
    	        LineSelectPoints[3] = new Point(ConnectionPoints[2].X - LineHalfThickness, ConnectionPoints[2].Y);
	            LineSelectPoints[4] = new Point(ConnectionPoints[2].X, ConnectionPoints[2].Y - LineHalfThickness);   	            
        	}     

        	ChangeTextLocation();
        		
			this.Draw(true);  

			int i, j;

        	for (j = 0; j < 2; j++)
        	{
        		for (i = 0; i < ConConnection[j].Count; i++)
        		{
        			ConConnection[j][i].CurrentEnd = ConEnd[j][i];
        			ConConnection[j][i].ChangeLocation(ConnectionPoints[j + 1], false);
        		}
        	}  						
        }
        
        public void ChangeLocation(Point point, bool isSelected)
        {    	        
        	int node;
        	if (CurrentEnd == 0)
        	{
				ConnectionPoints[0] = point;
				node = 0;
        	}
        	else
        	{
        		ConnectionPoints[3] = point;
        		node = 1;
        	}
			
            if (IsCreating)
            {
            	ConnectionPoints[1] = new Point(ConnectionPoints[0].X + ((ConnectionPoints[3].X - 
         			ConnectionPoints[0].X) / 2), ConnectionPoints[0].Y);
            }
            else
            {
                ConnectionPoints[1].Y = ConnectionPoints[0].Y;
            }
            
           	ConnectionPoints[2] = new Point(ConnectionPoints[1].X, ConnectionPoints[3].Y);			
			
       		LineSelectPoints[0] = new Point(ConnectionPoints[0].X, ConnectionPoints[0].Y - LineHalfThickness);
            LineSelectPoints[1] = new Point(ConnectionPoints[1].X, ConnectionPoints[1].Y + LineHalfThickness);
            LineSelectPoints[2] = new Point(ConnectionPoints[1].X + LineHalfThickness, ConnectionPoints[1].Y);
            LineSelectPoints[3] = new Point(ConnectionPoints[2].X - LineHalfThickness, ConnectionPoints[2].Y);
            LineSelectPoints[4] = new Point(ConnectionPoints[2].X, ConnectionPoints[2].Y - LineHalfThickness);
            LineSelectPoints[5] = new Point(ConnectionPoints[3].X, ConnectionPoints[3].Y + LineHalfThickness);           	
            
            ChangeTextLocation();
            
            this.Draw(isSelected);
            
        	for (i = 0; i < ConConnection[node].Count; i++)
        	{
        		ConConnection[node][i].CurrentEnd = ConEnd[node][i];
        		ConConnection[node][i].ChangeLocation(ConnectionPoints[node + 1], false);
        	}            
        }
        
       	protected void ChangeTextLocation()
		{       		
       		PointText = new Point(ConnectionPoints[1].X + 3,  ConnectionPoints[1].Y + 
       			(ConnectionPoints[2].Y - ConnectionPoints[1].Y) / 2 - Signal.Height);
		}	
        
        public void Draw(bool isSelected)
        {
            using (DrawingContext dc = this.RenderOpen())
            {
            	pen = LinePen;
            	if (isSelected) pen = LineSelectedPen;
            	
            	//body
            	dc.DrawRectangle(background, null,
            		new Rect(LineSelectPoints[0], LineSelectPoints[1]));
				dc.DrawRectangle(background, null,
            	    new Rect(LineSelectPoints[2], LineSelectPoints[3]));
				dc.DrawRectangle(background, null,
            	    new Rect(LineSelectPoints[4], LineSelectPoints[5]));
            	
            	dc.DrawLine(pen, ConnectionPoints[0], ConnectionPoints[1]);
                dc.DrawLine(pen, ConnectionPoints[1], ConnectionPoints[2]);
                dc.DrawLine(pen, ConnectionPoints[2], ConnectionPoints[3]);
				
                if (EndShape[0] == null)
                {	               
                	dc.DrawEllipse(Brushes.Yellow, null, ConnectionPoints[0],
                		EndCircleRadius, EndCircleRadius);
                	
                	dc.DrawEllipse(Brushes.Transparent, LinePen, ConnectionPoints[0], 
                		EndCircleRadius, EndCircleRadius);
                }
                
                if ((EndShape[1] == null) && (!IsCreating))
                {	               
                	dc.DrawEllipse(Brushes.Yellow, null, 
                	    ConnectionPoints[ConnectionPoints.Length - 1],
                		EndCircleRadius, EndCircleRadius);
                	
                	dc.DrawEllipse(Brushes.Transparent, LinePen,
   	                	ConnectionPoints[ConnectionPoints.Length - 1], 
   	                	EndCircleRadius, EndCircleRadius);
                } 

                if (GetSignal() != "")
                {  
                	dc.DrawText(Signal, PointText);
                }
            }       	
        }
        
		public void SetSignal(string text, Connection connection)
    	{
			IsSignalInput = connection.IsSignalInput;
			if ((IsSignalInput == true) && !text.EndsWith(">"))
			{
				text += ">";
			}			
    		Signal = new FormattedText(
    			text,
  			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);			
    		
			ChangeTextLocation();
			Draw(false);
			
			if ((EndShape[0] is Connection) && (connection != EndShape[0]))
			{
				((Connection)EndShape[0]).SetSignal(Signal.Text, this);
			}
			else if ((EndShape[1] is Connection) && (connection != EndShape[1]))
			{
				((Connection)EndShape[1]).SetSignal(Signal.Text, this);
			}
			
			for (int j = 0; j < 2; j++)
			{
				for (int i = 0; i < ConConnection[j].Count; i++)
				{
					ConConnection[j][i].SetSignal(Signal.Text, this);
				}
			}
    	}		
		
		public void SetSignal(string text, bool? isSignIn)
		{
			IsSignalInput = isSignIn;
			if ((isSignIn == true) && !text.EndsWith(">"))
			{
				text += ">";
			}			
    		Signal = new FormattedText(
    			text,
  			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);    		
			
			ChangeTextLocation();
			Draw(true);
			
			if (EndShape[0] is Connection)
			{
				((Connection)EndShape[0]).SetSignal(Signal.Text, this);
			}
			else if (EndShape[1] is Connection)
			{
				((Connection)EndShape[1]).SetSignal(Signal.Text, this);
			}	

			for (int j = 0; j < 2; j++)
			{
				for (int i = 0; i < ConConnection[j].Count; i++)
				{
					ConConnection[j][i].SetSignal(Signal.Text, this);
				}
			}			
		}
		
		public void SetSignal(string text, bool? isSignIn, int minusOne)
		{
			IsSignalInput = isSignIn;
			if ((isSignIn == true) && !text.EndsWith(">"))
			{
				text += ">";
			}
    		Signal = new FormattedText(
    			text,
  			    CultureInfo.GetCultureInfo("en-us"),
   			    FlowDirection.LeftToRight,
   			    new Typeface("TimesNewRoman"),
   			    FontSize,
   			    Brushes.Black);
    		
			ChangeTextLocation();
			Draw(true);									
		}		
		
		public string GetSignal()
		{
    		return Signal.Text;
    	}	        
        
        public Point GetCurrentEndPnt(int index)
        {
        	return ConnectionPoints[index];
        }
     
        public int GetNodeNum(Point point)
        {
        	Vector v1, v2;
        	
        	v1 = ConnectionPoints[1] - point;
        	v2 = ConnectionPoints[2] - point;
        	
        	if (v1.Length < v2.Length)
        		return 0;
        	else
        		return 1;
        }

		public void SetConnection(Block block, int portNum)
		{
			EndShape[CurrentEnd] = block;
			EndPortNum[CurrentEnd] = portNum;
		}

		public void SetConnection(Connection connection, int node)
		{
			int end;
			
			if (CurrentEnd == 0)
				end = 1;
			else 
				end = 0;
			
			if (!(EndShape[end] is Connection))
			{				
        		EndShape[CurrentEnd] = connection;
   	    		EndConNode = node;				
   	    		EndConNodeNum = connection.SetConnection(this, CurrentEnd, node);
   	    		
				ChangeLocation(connection.GetConnectionPoints(node + 1), true);
				
				if ((ConConnection[0].Count == 0) && (ConConnection[1].Count == 0))
					SetSignal(connection.GetSignal(), connection.IsSignalInput, -1);
				else
					SetSignal(connection.GetSignal(), connection.IsSignalInput);
			}			
		}
		
		public int SetConnection(Connection connection, int end, int node)
		{
			ConConnection[node].Add(connection);
			ConEnd[node].Add(end);
			
			return ConConnection[node].Count - 1;
		}
        
		public override void SelfDisposal()
		{
			int i, j;
			
			for (i = 0; i < EndCount; i++)
			{
				if (EndShape[i] != null)
				{
					if (EndShape[i] is Block)
					{
						EndShape[i].ClearConnection(EndPortNum[i]);
					}
					else
						((Connection)EndShape[i]).ClearConnection(EndConNode, EndConNodeNum);
					
					ClearConnection(i);
				}				
			}				

			for (j = 0; j < 2; j++)
			{
				for (i = 0; i < ConConnection[j].Count; i++)
				{
					ConConnection[j][i].ClearConnection(ConEnd[j][i]);
					ClearConnection(j, i);
				}								
			}
			
			ConConnection = null;
		}
		
        public override void ClearConnection(int clearingEnd)
        {	
        	if (EndShape[clearingEnd] is Connection)
        	{
        		EndConNode = -1;    
				EndConNodeNum = -1;         		
        	}
        	else
        		EndPortNum[clearingEnd] = -1;
        	
        	EndShape[clearingEnd] = null;
        	Draw(false);       	
        }        
        
        public void ClearConnection(int clearingNode, int num)
        {
        	if ((ConConnection != null) && (clearingNode < 2) && 
        	    (num < ConConnection[clearingNode].Count))
        	{
        		ConConnection[clearingNode].RemoveAt(num);
        	}
        	if ((ConEnd != null) && (clearingNode < 2) && (num < ConEnd.GetLength(0)))
        	{
        		ConEnd[clearingNode].RemoveAt(num);
        	}
        }
        #endregion
    } 
    
    public enum Part
    {
    	None,
    	End,
    	Middle,
    }
}
