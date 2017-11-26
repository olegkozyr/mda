using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using MeasuringDeviceAnalizer;

namespace BlockScheme
{	
    public class DrawingCanvas : Canvas
    {    		    		    
		public BlockSchemeWindow MnWindow;
    	
    	private int counter = 0;
        private List<Visual> visuals = new List<Visual>();
        
        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }
        public void AddVisual(Visual visual)
        {
            visuals.Add(visual);
            base.AddVisualChild(visual);
            base.AddLogicalChild(visual);
        }       
		
        public void AddVisuals(List<ShapeBase> shapeBases)
        {
        	int count = shapeBases.Count;
        	for (int i = 0; i < count; i++)
        	{
        		visuals.Add(shapeBases[i]);
    	        base.AddVisualChild(shapeBases[i]);
	            base.AddLogicalChild(shapeBases[i]);        		
        	}
        }
        
        public List<Visual> GetVisuals()
        {
        	return visuals;
        }
        
        public int GetVisualCount()
        {
        	return VisualChildrenCount;
        }
        
        public void SetVisualTopmost(Visual visual)
        {//перемещение объекта на верх всех остальных
        	visuals.Remove(visual);
   	        base.RemoveVisualChild(visual);
   	        base.RemoveLogicalChild(visual); 
    	        
    	    visuals.Add(visual);
    	    base.AddVisualChild(visual);
    		base.AddLogicalChild(visual);
        }
        
        public void DeleteVisual(Visual visual)
        {      	
        	((ShapeBase)visual).SelfDisposal();
            visuals.Remove(visual);
            base.RemoveVisualChild(visual);          
        }

        public void ClearAll()
        { 
        	for (int i = visuals.Count - 1; i > -1; i--)
        	{
        		((ShapeBase)visuals[i]).SelfDisposal();    	        
    	        base.RemoveVisualChild(visuals[i]);
    	        visuals.Remove(visuals[i]);
        	}
        }        
        
        public DrawingVisual GetVisual(Point point)
        {
            HitTestResult hitResult = VisualTreeHelper.HitTest(this, point);
            if (hitResult != null)
                return hitResult.VisualHit as DrawingVisual;
            else
                return null;         
        }
 		
        public DrawingVisual GetVisual(int index)
        {
        	return GetVisualChild(index) as DrawingVisual;
        }
        
        private DrawingVisual hitDrawingVisual = new DrawingVisual();
        public DrawingVisual GetVisualCptMode(Point point)
        {
            VisualTreeHelper.HitTest(this, null,
        		new HitTestResultCallback(this.ResultCallback), new PointHitTestParameters(point));
        	
            return hitDrawingVisual;       
        }
        
        private HitTestResultBehavior ResultCallback(HitTestResult result)
        {
        	if (counter == 0)
        	{
        		if (result.VisualHit.GetType() == typeof(Connection))
	        	{	
        			counter++;
        			return HitTestResultBehavior.Continue;
	        	}	
        	}
        	else
        		counter = 0;

            if (result != null)
                hitDrawingVisual = result.VisualHit as DrawingVisual;
            else
                hitDrawingVisual = null;         	

        	return HitTestResultBehavior.Stop;
        }
        
        private List<DrawingVisual> hits = new List<DrawingVisual>();
        public List<DrawingVisual> GetVisuals(Geometry region)
        {
            hits.Clear();
            GeometryHitTestParameters parameters = new GeometryHitTestParameters(region);
            HitTestResultCallback callback = new HitTestResultCallback(this.HitTestCallback);
            VisualTreeHelper.HitTest(this, null, callback, parameters);
            return hits;
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            GeometryHitTestResult geometryResult = (GeometryHitTestResult)result;
            DrawingVisual visual = result.VisualHit as DrawingVisual;
            if (visual != null &&
                geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                hits.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        }

        public void MeasureCall()
        {
        	this.MeasureOverride(new Size(double.PositiveInfinity,
        	                              double.PositiveInfinity));
        }
        
        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            double left, top;
            foreach (DrawingVisual visual in visuals)
            {
            	left = visual.ContentBounds.Left;
                top = visual.ContentBounds.Top;
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                Size desiredSize = visual.ContentBounds.Size;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }

            // add some extra margin
            size.Width += 10;
            size.Height += 10;
            return size;
        }
        
        public string PreSolver(out string[,] CoefMatrix, out string[] BMatrix)
		{
			int i, j, k, t; 
			int count = VisualChildrenCount;
			List<Block> blocks = new List<Block>();
			List<Connection> connections = new List<Connection>();
			string text = "";
			string[] temp = new string[2];
			int max = 0;			
			BMatrix = null;
			CoefMatrix = null;			
			
			for ( i = 0; i < count; i++)
			{
				if (!((ShapeBase)visuals[i]).IsConnected())
				{
					blocks = null;
					return "Не всі елементи з'єднані";						
				}
					
				if (visuals[i] is Block)
				{					
					blocks.Add(visuals[i] as Block);				
				}
				else 
				{					
					text = ((Connection)visuals[i]).GetSignal();
					if (text != "")
					{		
						if (text.EndsWith(">"))
						{
							t = Convert.ToInt16(text.Substring(0, text.Length - 1));
						}
						else
						{
							t = Convert.ToInt16(text);
						}
						if (max < t)
							max = t;						
					}
					
					if (((Connection)visuals[i]).IsSignalInput == true)
					{
						if (text == "")
						{
							blocks = null;
							connections = null;
							return "Вхідний сигнал не має номеру";								
						}
						else
						{
							connections.Add(visuals[i] as Connection);
						}
					}					
				}
			}

			if (connections == null)
			{
				blocks = null;
				connections = null;
				return "Не вказано вхідні сигнали";					
			}
			
			CoefMatrix = new string[max,max];
			BMatrix = new string[max];
			
			for (j = 0; j < max; j++)
			{
				for (i = 0; i < max; i++)
				{
					CoefMatrix[i, j] = "0";
				}
				BMatrix[j] = "0";
			}			

			//Connections
			for (i = 0; i < connections.Count; i++)
			{
				text = connections[i].GetSignal();
				if (text.EndsWith(">"))
				{
					text = text.Substring(0, text.Length - 1);
					BMatrix[Convert.ToInt32(text) - 1] = "x" + text;
				}
				else
				{
					BMatrix[Convert.ToInt32(text) - 1] = "x" + text;				
				}
			}			
			
			connections.Clear();
			
			for (j = 0; j < max; j++)
			{
				CoefMatrix[j, j] = "1";
			}	

			//Blocks
			for (i = 0; i < blocks.Count; i++)
			{
				#region BlockOneWay
				if (blocks[i] is BlockOneWay)
				{
					temp[0] = "";
					temp[1] = "";
					text = ((BlockOneWay)blocks[i]).TransRatioGet();
					t = 0;
					for (j = 0; (j < text.Length) && (t < 2);)
					{
						for (; (j < text.Length) && (!Char.IsDigit(text, j)); j++) {}
						if (j <= text.Length)
						{
							temp[t] += text[j];
							for (j++; (j < text.Length) && (Char.IsDigit(text, j)); j++) 
							{
								temp[t] += text[j];
							}
							t++;							
						}						
					}
					
					if ((temp[0] != "") && (temp[1] != ""))
					{
						k = Convert.ToInt16(temp[0]) - 1;
						t = Convert.ToInt16(temp[1]) - 1;
						if ((k < CoefMatrix.GetLength(0)) && (t < CoefMatrix.GetLength(0)))
						{
							if (text[0] == '-')
								CoefMatrix[k, t] = text.Substring(1);
							else
								CoefMatrix[k, t] = "-" + text;
						}
						else
						{
							blocks = null;
							CoefMatrix = null;
							return "В однонаправленому блоці неправильно задано коефіцієнт перетворення";							
						}
					}
					else
					{
						blocks = null;
						CoefMatrix = null;
						return "В однонаправленому блоці неправильно задано коефіцієнт перетворення";						
					}
				}
				#endregion				
				#region BlockTwoWays
				else if (blocks[i] is BlockTwoWays)
				{
					max = BlockTwoWays.TransRatioCount;
					for (k = 0; k < max; k++)
					{
						temp[0] = "";
						temp[1] = "";
						text = ((BlockTwoWays)blocks[i]).TransRatioGet(k);
						t = 0;
						for (j = 0; (j < text.Length) && (t < 2);)
						{
							for (; (j < text.Length) && (!Char.IsDigit(text, j)); j++) {}
							if (j <= text.Length)
							{
								temp[t] += text[j];
								for (j++; (j < text.Length) && (Char.IsDigit(text, j)); j++) 
								{
									temp[t] += text[j];
								}
								t++;							
							}						
						}
						
						if ((temp[0] != "") && (temp[1] != ""))
						{
							j = Convert.ToInt16(temp[0]) - 1;
							t = Convert.ToInt16(temp[1]) - 1;
							if ((j < CoefMatrix.GetLength(0)) && (t < CoefMatrix.GetLength(0)))							
							{
								if (text[0] == '-')
									CoefMatrix[j, t] = text.Substring(1);
								else
									CoefMatrix[j, t] = "-" + text;
							}
							else
							{
								blocks = null;
								CoefMatrix = null;
								return "В двонаправленому блоці неправильно задано коефіцієнт перетворення";							
							}							
						}
						else
						{
							blocks = null;
							CoefMatrix = null;
							return "В двонаправленому блоці неправильно задано коефіцієнт перетворення";						
						}
					}
				}
				#endregion
				#region BlockBidirectCounter
				else if (blocks[i] is BlockBidirectCounter)
				{					
					text = ((BlockBidirectCounter)blocks[i]).GetInPortSigns();
					connections.Clear();
					connections = ((BlockBidirectCounter)blocks[i]).GetConnectionObj;
					temp[0] = connections[0].GetSignal();
					if (temp[0] != "")
					{
						if (temp[0].EndsWith(">"))
						{
							k = Convert.ToInt16(temp[0].Substring(0, temp[0].Length - 1)) - 1;
						}
						else
						{
							k = Convert.ToInt16(temp[0]) - 1;
						}						
						if (k < CoefMatrix.GetLength(0))						
						{						
							for (j = 0; j < text.Length; j++)
							{
								temp[1] = connections[j + 1].GetSignal();
								if (temp[1] != "")
								{
									if (temp[1].EndsWith(">"))
									{
										t = Convert.ToInt16(temp[1].Substring(0, temp[1].Length - 1)) - 1;
									}
									else
									{
										t = Convert.ToInt16(temp[1]) - 1;
									}
									if (t < CoefMatrix.GetLength(0))						
									{									
										if (text[j] == '-')
											CoefMatrix[k, t] = "a" + temp[0] + "," + temp[1];
										else
											CoefMatrix[k, t] = "-a" + temp[0] + "," + temp[1];
									}
									else
									{
										blocks = null;
										CoefMatrix = null;
										connections = null;
										return "В реверсивному лічильнику невказаний вхідний сигнал";						
									}									
								}
								else
								{
									blocks = null;
									CoefMatrix = null;
									connections = null;
									return "В реверсивному лічильнику невказаний вхідний сигнал";						
								}
							}
						}
						else
						{
							blocks = null;
							CoefMatrix = null;
							connections = null;
							return "В суматорі невказаний вихідний сигнал";							
						}						
					}
					else
					{
						blocks = null;
						CoefMatrix = null;
						connections = null;
						return "В реверсивному лічильнику невказаний вихідний сигнал";						
					}
				}	
				#endregion				
				#region BlockSum
				else if (blocks[i] is BlockSum)
				{					
					text = ((BlockSum)blocks[i]).GetInPortSigns();
					connections = ((BlockSum)blocks[i]).GetConnectionObj;
					temp[0] = connections[0].GetSignal();
					if (temp[0] != "")
					{
						if (temp[0].EndsWith(">"))
						{
							k = Convert.ToInt16(temp[0].Substring(0, temp[0].Length - 1)) - 1;
						}
						else
						{
							k = Convert.ToInt16(temp[0]) - 1;
						}
						if (k < CoefMatrix.GetLength(0))						
						{						
							for (j = 0; j < text.Length; j++)
							{
								temp[1] = connections[j + 1].GetSignal();
								if (temp[1] != "")
								{
									if (temp[1].EndsWith(">"))
									{
										t = Convert.ToInt16(temp[1].Substring(0, temp[1].Length - 1)) - 1;
									}
									else
									{
										t = Convert.ToInt16(temp[1]) - 1;
									}
									if (t < CoefMatrix.GetLength(0))						
									{									
										if (text[j] == '-')
											CoefMatrix[k, t] = "1";
										else
											CoefMatrix[k, t] = "-1";
									}
									else
									{
										blocks = null;
										CoefMatrix = null;
										connections = null;
										return "В суматорі невказаний вхідний сигнал";										
									}
								}
								else
								{
									blocks = null;
									CoefMatrix = null;
									connections = null;
									return "В суматорі невказаний вхідний сигнал";						
								}
							}
						}
						else
						{
							blocks = null;
							CoefMatrix = null;
							connections = null;
							return "В суматорі невказаний вихідний сигнал";							
						}
					}
					else
					{
						blocks = null;
						CoefMatrix = null;
						connections = null;
						return "В суматорі невказаний вихідний сигнал";						
					}
				}
				#endregion				
			}						
			
			blocks = null;
			return "";
		}
    }
}
