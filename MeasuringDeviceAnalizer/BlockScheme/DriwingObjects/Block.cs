using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace BlockScheme
{
    public abstract class Block : ShapeBase
    {   	    
		#region filds    	
    	// Block shape
    	protected static readonly Size BlockMinSize = new Size(60, 40);
        protected static readonly Size BlockPortHalfSize = new Size(2.5, 2.5);  
		protected static readonly int PortPointCount = 5;  	
		protected readonly int SideCount = 2;
		
        //drawing
	        protected static readonly Brush DrawingBrush = Brushes.White;
    	    protected static readonly double BlockPenThickness = 1;
    	    protected static readonly double FontSize = 12;
    	    protected static readonly Pen DrawingPen = new Pen(Brushes.Black, BlockPenThickness);    	    
        //selection
        	protected static readonly Pen SelectionPen = new Pen(Brushes.DarkCyan, BlockPenThickness);            	        

        protected Size BlockSize;
        
        //верхнии левая и правая точки прямоугольника
        protected Point[] ShapePoints;
        public Point GetLeftTopPoint
        {
        	get {return ShapePoints[0];}
        }
        
		//точки прямоугальных портов
		//расположение точек: по часовой стрелке, 0-я точка - центр порта, 1-я - верхняя левая
        //расположение портов: 0 - справа, все остальные - слева  		
		protected List<Point[]> PortPoints;

		//объекты Connection и № концов подключаемые к портам
        protected List<Connection> ConnectionObj;

        public List<Connection> GetConnectionObj
        {
        	get{return ConnectionObj;}
        }        
        
        protected List<int> ConnectionEnd;		
        
        //temp filds
		protected Pen pen;
        #endregion
		
        public Block()
        {   
			ShapePoints = new Point[SideCount];  

        	PortPoints = new List<Point[]>();
        	PortPoints.Add(new Point[PortPointCount]);      
			PortPoints.Add(new Point[PortPointCount]);        	
			
        	ConnectionObj = new List<Connection>();
        	ConnectionObj.Add(null);
        	ConnectionObj.Add(null);
        	
        	ConnectionEnd = new List<int>();
        	ConnectionEnd.Add(-1);
        	ConnectionEnd.Add(-1);			
        }                     
		public override bool IsConnected()
		{
			for (int i = 0; i < ConnectionObj.Count; i++)
			{
				if (ConnectionObj[i] == null)
					return false;
			}
			
			return true;
		}
        
        public abstract void ChangeLocation(Point point, bool isSelected); 
        public abstract void Draw(bool isSelected, bool isPortsVisible);  

		public Point GetPortCenter(int portNum)
		{
			return PortPoints[portNum][0];
		}   
		
        public int PortHitTest(Point point)
        {
        	int i;
        	
        	for (i = 0; i < PortPoints.Count; i++)
        	{
        		if (((PortPoints[i][1].X < point.X) && (PortPoints[i][2].X > point.X) &&
        		    (PortPoints[i][1].Y < point.Y) && (PortPoints[i][4].Y > point.Y)) &&
        		    (ConnectionObj[i] == null))
        			return i;
        	}
        	
        	return -1;
        }          
		
        protected void PortPointsEval()
		{
        	for (int i = 0; i < PortPoints.Count; i++)
        	{
        		PortPoints[i][1] = new Point(PortPoints[i][0].X - BlockPortHalfSize.Width,
        		                             PortPoints[i][0].Y - BlockPortHalfSize.Height);
        		PortPoints[i][2] = new Point(PortPoints[i][0].X + BlockPortHalfSize.Width,
        		                             PortPoints[i][0].Y - BlockPortHalfSize.Height);
        		PortPoints[i][3] = new Point(PortPoints[i][0].X + BlockPortHalfSize.Width,
        		                             PortPoints[i][0].Y + BlockPortHalfSize.Height);
        		PortPoints[i][4] = new Point(PortPoints[i][0].X - BlockPortHalfSize.Width,
        		                             PortPoints[i][0].Y + BlockPortHalfSize.Height);
        	}			
		}
        
		public  void SetConnection(int portNum, Connection connection, int end)
		{
			if ((portNum > -1) && (portNum < PortPoints.Count))
			{
				ConnectionObj[portNum] = connection;
				ConnectionEnd[portNum] = end;
			}
		}         
        
        public override void ClearConnection( int portNum )
        {
        	ConnectionObj[portNum] = null;
        	ConnectionEnd[portNum] = -1;				       	
        }	                        
        
		public override void SelfDisposal()
		{
			int i;
			
			for (i = 0; i < PortPoints.Count; i++)
			{
				if (ConnectionObj[i] != null)
				{
					ConnectionObj[i].ClearConnection(ConnectionEnd[i]);
					ConnectionObj[i].Draw(false);				
					ClearConnection(i);
				}								
			}
		}        
    }
}
