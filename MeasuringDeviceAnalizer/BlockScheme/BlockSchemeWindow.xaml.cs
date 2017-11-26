#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using MeasuringDeviceAnalizer;

#endregion

namespace BlockScheme
{
	/// <summary>
	/// Interaction logic for BlockSchemeWindow.xaml
	/// </summary>
	public partial class BlockSchemeWindow : Window
	{	
        #region fields        
       	//контейнеры
        	//временные
        	private DrawingVisual visualObject = null;
        	private Block tempBlock = null;
        	private ShapeBase tempShape = null;
            private int tempPortNum;        	
        	private Point pointClicked;
        	private Vector clickOffset;
        	private Point pointDragged;
        	private Part part;
        	
        	//для передачи данных       	
			public MainWindow mainWindow;        		
        	private DrawingVisual selectedVisual = null; //текущий вибрынный объект
        	private Block blockPortVisual = null; //формируемое соединение
        	private Connection connectionLine = null; //формируемое соединение
        	private Connection draggingConnection = null;
        	//ShapeBase для которого формируется соединение
        	private ConnectingShape connectingShape = new ConnectingShape(); 
        	
		//флаги	выполняемых действий		
			private bool isDragging = false; //перемещение блоков
			private bool isConnectionDragging = false; //частичное перетаскивание соединения
       		private bool isConnectionDraw = false; //рисование соединения       						

        //ContextMenu 
        private ContextMenu canvasContextMenu = new ContextMenu();   
        #endregion

        #region constructor
        public BlockSchemeWindow()
        {
            InitializeComponent();
            
            drawingSurface.MnWindow = this;
            
            //ContextMenu
            MenuItem menuItem;
            menuItem = new MenuItem();
            menuItem.Name = "changeContentMenuItem";
            menuItem.Header = "Змінити вміст";
            menuItem.Click += changeContentMenuItem_Click;
            canvasContextMenu.Items.Add(menuItem);
            menuItem = new MenuItem();
            menuItem.Name = "quitMenuItem";
            menuItem.Header = "Видалити";
            menuItem.Click += deleteMenuItem_Click;
            canvasContextMenu.Items.Add(menuItem);            
        }
        #endregion

        #region methods
		#region state          
        public void WindowDisposal()
        {
        	drawingSurface.ClearAll();
        	this.Close();
        }                   
        #endregion
        
        private void ClearConnectionMode()
        {
        	connectingShape.Clear();
			drawingSurface.Cursor = Cursors.Arrow;
        }                                            
                
        private void SelectBlockConnection(Point pointClicked)
        {
            visualObject = drawingSurface.GetVisual(pointClicked);

            if (visualObject != null)
            {
            	drawingSurface.SetVisualTopmost(visualObject);

                if (visualObject is Block)
                {
                	((Block)visualObject).Draw(true, false);
                }
                else if (visualObject is Connection)
                {
                	((Connection)visualObject).Draw(true);
                }
                
                // Change the previous selection
                if (selectedVisual != null && selectedVisual != visualObject)
                {
                    SelectionClear();
                }
                selectedVisual = visualObject;
                visualObject= null;
            }
            else if (selectedVisual != null)
            {
                SelectionClear();
            }
        }
        
        private void SelectionClear()
        {
            if (selectedVisual is Block)
            {
            	((Block)selectedVisual).Draw(false, false);
            }
            else if (selectedVisual is Connection)
            {            	
            	((Connection)selectedVisual).Draw(false);
            }
            selectedVisual = null;
        }
        #endregion				
        
        #region event handlers           
		#region block scheme events        
        private void drawingSurface_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
        	//текущая позиция миши
            pointClicked = e.GetPosition( drawingSurface );

            //перерисовка предыдущего выбранного объекта
            if (selectedVisual != null)
                SelectionClear();
		
            //проверка нажатых клавищ панели инструментов
            if ( cmdBlockOneWay.IsChecked == true )
            {//создать однонаправленный блок           	
                tempBlock = new BlockOneWay(pointClicked);
                drawingSurface.AddVisual( tempBlock );
                selectedVisual = tempBlock;
                tempBlock = null;
            }    
            else if ( cmdBlockTwoWays.IsChecked == true )
            {//создать двунаправленный блок
                tempBlock = new BlockTwoWays(pointClicked);
                drawingSurface.AddVisual( tempBlock );
                selectedVisual = tempBlock;
                tempBlock = null;
            } 
            else if ( cmdBlockSum.IsChecked == true )
            {//создать сумматор
                tempBlock = new BlockSum(pointClicked);
                drawingSurface.AddVisual( tempBlock );
                selectedVisual = tempBlock;
                tempBlock = null;
            }
            else if ( cmdBlockBidirectCounter.IsChecked == true )
            {//создать реверсивный счетчик
                tempBlock = new BlockBidirectCounter(pointClicked);
                drawingSurface.AddVisual( tempBlock );
                selectedVisual = tempBlock;
                tempBlock = null;
            }            
            else if ( cmdBlockSwitch.IsChecked == true )
            {//создать переключатель
                tempBlock = new BlockSwitch(pointClicked);
                drawingSurface.AddVisual( tempBlock );
                selectedVisual = tempBlock;
                tempBlock = null;
            }            
            else if ( cmdConnection.IsChecked == true )
            {//создание соединения
                if (connectingShape.TargetBlock != null)
                {//курсор над портом
                	connectionLine = 
                		new Connection(connectingShape.TargetBlock.GetPortCenter(connectingShape.PortNum), 
                		               connectingShape.TargetBlock, connectingShape.PortNum);                	
                	connectingShape.TargetBlock.SetConnection(connectingShape.PortNum, connectionLine, 0);
                }
                else if (connectingShape.TargetConnection != null)
                {
                	connectionLine = new Connection(connectingShape.TargetConnection.GetConnectionPoints(
                	connectingShape.Node + 1), connectingShape.TargetConnection,
	                	connectingShape.Node, -1);
                	connectionLine.EndConNodeNum = connectingShape.TargetConnection.SetConnection(
                		connectionLine, 0, connectingShape.Node);
                }
                else
                	connectionLine = new Connection(pointClicked, null, -1);
                
				drawingSurface.AddVisual(connectionLine);				
                ClearConnectionMode();
                isConnectionDraw = true;
                drawingSurface.CaptureMouse();
            }                 
            else if ( cmdDelete.IsChecked == true )
            {//удаление любого объекта класса DrawingVisual
                visualObject = drawingSurface.GetVisual(pointClicked);
                if (visualObject != null)
                {
                    drawingSurface.DeleteVisual(visualObject);
                    visualObject = null;
                }
            }
            else if (cmdSelectMove.IsChecked == true ) 
            {//выбрать, переместить объект	
				//визуализация выбранного visual
    	        SelectBlockConnection( pointClicked );
					
    	        //проверка выбран ли объект
    	        if (selectedVisual != null)
    	       	{
    	           	if (selectedVisual is Block)
    	           	{//перемещение блока
    	           		clickOffset = ((Block)selectedVisual).GetLeftTopPoint - pointClicked;
		               	//действие: разрешается перемещение блока
    	               	isDragging = true;
    	                drawingSurface.CaptureMouse();
    	            }
    	           	else if (selectedVisual is Connection)
   	                { 
   	           			draggingConnection = selectedVisual as Connection;
   	           			part = draggingConnection.Dragging(pointClicked);
   	           			if (part == Part.End)
   	           			{
   	           				connectionLine = draggingConnection;
   	           				draggingConnection = null;
   	           				connectingShape.Clear();
   	                    	isConnectionDraw = true;
   	                    	drawingSurface.CaptureMouse();
   	            		}
   	           			else if (part == Part.Middle)
   	           			{//действие: разрешается частичное перемещение соединения
   	                    	isConnectionDragging = true;
   	                    	drawingSurface.CaptureMouse();   	           				
   	           			}
   	           			else
   	           			{
   	           				draggingConnection = null;
   	           			}
    	           	}
    	    	}
            }           
        }

        private void drawingSurface_MouseMove(object sender, MouseEventArgs e)
        {//перемещение мыши
        	//текущая позиция
            pointDragged = e.GetPosition(drawingSurface);			            
            
            //флаги разрешенных действий
            if (isDragging)
            {//перемещение блока
            	((Block)selectedVisual).ChangeLocation(pointDragged + clickOffset, true);
            	drawingSurface.InvalidateMeasure();
            }
            else if (isConnectionDragging)
            {//частичное перемещение соединения
            	draggingConnection.ChangeLocation(pointDragged);
            	drawingSurface.InvalidateMeasure();
            }
            else if ((isConnectionDraw) || (cmdConnection.IsChecked == true))
            {
				//прорисовка соединения
				if (isConnectionDraw)
				{
					connectionLine.ChangeLocation(pointDragged, true);
					drawingSurface.InvalidateMeasure();
					tempShape = drawingSurface.GetVisualCptMode(pointDragged) as ShapeBase;
				}
				else               
	            	tempShape = drawingSurface.GetVisual(pointDragged) as ShapeBase;

				if (tempShape == null)
            	{
                	if (blockPortVisual != null)
                	{
                		blockPortVisual.Draw(false, false);
                		blockPortVisual = null;
                	}
                	
                	if ((connectingShape.TargetBlock != null) || (connectingShape.TargetConnection != null))
            			ClearConnectionMode();    

					return;                	
            	}
				
                if (tempShape is Block)
                {
                	if (blockPortVisual != null)
                	{
                		blockPortVisual.Draw(false, false);
                		blockPortVisual = null;
                	}
                	
                	tempBlock = tempShape as Block;
                	tempShape = null;
   	            	drawingSurface.SetVisualTopmost(tempBlock);
             	
                	blockPortVisual = tempBlock;
                    tempBlock.Draw(false, true);
                    	
                    //поиск порта
                    tempPortNum = tempBlock.PortHitTest(pointDragged);
                    if (tempPortNum != -1)
                    {//порт найден 
                        connectingShape.TargetBlock = tempBlock;
                        connectingShape.PortNum = tempPortNum;
                        drawingSurface.Cursor = Cursors.Cross; 
                    }
                    else
                    	drawingSurface.Cursor = Cursors.Arrow;                     	
                	
                    tempBlock = null;
                }
                else if (tempShape is Connection)
                {
                	connectingShape.TargetConnection = tempShape as Connection;
                	connectingShape.Node = ((Connection)tempShape).GetNodeNum(pointDragged);
                }
                
            }            
            
            e.Handled = true;
        }

        private void drawingSurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {                      
            if (isDragging == true)
            {
            	drawingSurface.ReleaseMouseCapture();
                isDragging = false;
            }
            else if (isConnectionDragging == true)
            {
            	drawingSurface.ReleaseMouseCapture();
                isConnectionDragging = false;
            }             
            else if (isConnectionDraw)
            {//задано действие прорисовки соединения
            	drawingSurface.ReleaseMouseCapture();
            	isConnectionDraw = false;
            	if (connectingShape.TargetBlock != null) 
                {//курсор находится над портом  
            		connectionLine.SetConnection(connectingShape.TargetBlock, connectingShape.PortNum);
					connectionLine.ChangeLocation(connectingShape.TargetBlock.GetPortCenter(
    	       			connectingShape.PortNum),true);
					connectingShape.TargetBlock.SetConnection(connectingShape.PortNum, connectionLine, 
                    	connectionLine.CurrentEnd);
            		connectingShape.TargetBlock.Draw(false, false);
   	                ClearConnectionMode();   
           		}
            	else if (connectingShape.TargetConnection != null)
            	{
            		connectionLine.SetConnection(connectingShape.TargetConnection, connectingShape.Node);
            		ClearConnectionMode(); 
            	}
   	            else
   	            {//курсор находится в любом месте: второе соединение не произошло       	            	
    	            drawingSurface.Cursor = Cursors.Arrow;    	            	
   	            } 
   	            
   	            if (connectionLine.IsCreating)
   	            	connectionLine.IsCreating = false;
   	           
   	            connectionLine.Draw(true);
   	            
   	            selectedVisual = connectionLine;
   	            connectionLine = null;
           	}        
        }

        private void drawingSurface_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pointClicked = e.GetPosition(drawingSurface);
			cmdSelectMove.IsChecked = true;
            SelectBlockConnection(pointClicked);

            if (selectedVisual is ShapeBase)
            {
	            drawingSurface.ContextMenu = canvasContextMenu;
            }
            else
            {
                if (drawingSurface.ContextMenu != null)
                {
            	    drawingSurface.ContextMenu = null;
                }
            }
        }
        #endregion
        
        #region block ContentMenu event handlers
        private void changeContentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Windows
            if (selectedVisual is BlockOneWay)
            {
            	BlockOneWayWindow dialogWindow = new BlockOneWayWindow();
    	        dialogWindow.Owner = this;           
    	        dialogWindow.TransformRatio.Text = ((BlockOneWay)selectedVisual).TransRatioGet();
    	        if ( dialogWindow.ShowDialog() == true )
    	        {
    	        	((BlockOneWay)selectedVisual).TransRatioSet(dialogWindow.TransformRatio.Text);
    	        	((Block)selectedVisual).Draw(true, false);
    	        }
    	        
    	        dialogWindow = null;
            }
            else if (selectedVisual is BlockTwoWays)
            {
            	BlockTwoWaysWindow dialogWindow = new BlockTwoWaysWindow();
    	        dialogWindow.Owner = this;           
    	        dialogWindow.TransformRatio0.Text = ((BlockTwoWays)selectedVisual).TransRatioGet(0);
    	        dialogWindow.TransformRatio1.Text = ((BlockTwoWays)selectedVisual).TransRatioGet(1);
    	        if ( dialogWindow.ShowDialog() == true )
    	        {
    	        	((BlockTwoWays)selectedVisual).TransRatioSet(dialogWindow.TransformRatio0.Text, 0);
    	        	((BlockTwoWays)selectedVisual).TransRatioSet(dialogWindow.TransformRatio1.Text, 1);
    	        	((Block)selectedVisual).Draw(true, false);
    	        }
    	        
    	        dialogWindow = null;
            }  
			else if (selectedVisual is BlockSum)
            {
            	BlockSumWindow dialogWindow = new BlockSumWindow();
    	        dialogWindow.Owner = this;           
    	        dialogWindow.InPortCount.Text = ((BlockSum)selectedVisual).GetInPortSigns();
    	        if ( dialogWindow.ShowDialog() == true )
    	        {
    	        	((BlockSum)selectedVisual).SetInPortSigns(dialogWindow.InPortCount.Text);
    	        }
    	        
    	        dialogWindow = null;
            }         
			else if (selectedVisual is BlockSwitch)
            {
            	BlockSwitchWindow dialogWindow = new BlockSwitchWindow();
    	        dialogWindow.Owner = this;           
    	        int[] portCount = ((BlockSwitch)selectedVisual).GetPortCount();
    	        dialogWindow.LeftPortCount.Text = portCount[0].ToString();
    	        dialogWindow.RightPortCount.Text = portCount[1].ToString();
    	        if ( dialogWindow.ShowDialog() == true )
    	        {    	        	
    	        	portCount[0] = Convert.ToInt16(dialogWindow.LeftPortCount.Text);
    	        	portCount[1] = Convert.ToInt16(dialogWindow.RightPortCount.Text);
    	        	((BlockSwitch)selectedVisual).SetPortCount(portCount);
    	        }
    	        
    	        dialogWindow = null;
            } 			
			else if (selectedVisual is Connection)
			{
            	ConnectionSignalWindow dialogWindow = new ConnectionSignalWindow();
    	        dialogWindow.Owner = this;           
    	        dialogWindow.Signal.Text = ((Connection)selectedVisual).GetSignal();
    	        dialogWindow.IsSignalInput.IsChecked = ((Connection)selectedVisual).IsSignalInput;
    	        if ( dialogWindow.ShowDialog() == true )
    	        {
    	        	((Connection)selectedVisual).SetSignal(dialogWindow.Signal.Text, 
    	        	                                       dialogWindow.IsSignalInput.IsChecked);
    	        }
    	        
    	        dialogWindow = null;				
			}
        }

        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVisual != null) drawingSurface.DeleteVisual(selectedVisual);
        }
        #endregion

        #region toolBars events
        private void cmdDelete_Checked(object sender, RoutedEventArgs e)
        {
            if (selectedVisual != null)
                SelectionClear();
        }
        
		private void CmdRun_Click(object sender, RoutedEventArgs e)
		{					
			string[,] CoefMatrix;
			string[] BMatrix;
			string erorText = drawingSurface.PreSolver(out CoefMatrix, out BMatrix);
			if (erorText != "")
			{
				MessageBox.Show(erorText);
				CoefMatrix = null;
				return;
			}	
			if ((CoefMatrix.GetLength(0) != 0) && (BMatrix.Length != 0))
			{			
	            mainWindow.SetArrays(CoefMatrix, BMatrix);			
   		    	string exception = string.Empty;
   		    	VisualToSerialize vts = new VisualToSerialize();
   		    	vts.SetShapeBase(drawingSurface.GetVisuals());
    		    	
   		       	string filePath = string.Empty;
   		       	if (OpenSaveClass.SaveCommand(vts, out filePath, out exception))
   		       	{
   		       		mainWindow.BlockSchemePath = filePath;
   		       	}
   		       	else
   		       	{
   		       		MessageBox.Show("Неможливо зберегти структурну схему внаслідок:\n" + exception);         		
	           	}  
			}	            
			
			CoefMatrix = null;
			BMatrix = null;		         
            drawingSurface.ClearAll();
            this.Close();
		}

		private void CmdClearAll_Click(object sender, RoutedEventArgs e)
		{
            //удаление всех обектов
            MessageBoxResult result = MessageBox.Show("Ви справді хочете видалити схему", "Попередження", 
                                                      MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
            	drawingSurface.ClearAll();
            }
            
            drawingSurface.MeasureCall();
		}		
        #endregion
 
        #region open / save events      
        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;            
        }

        private void SaveCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        { 
			string exception = string.Empty;
           	VisualToSerialize visualToSerialize = 
           		(VisualToSerialize)OpenSaveClass.OpenDialog(ModelType.Scheme, out exception);
            if (visualToSerialize == null)
            {
            	if (!string.IsNullOrEmpty(exception))
            	{
            		MessageBox.Show("Неможливо відкрити файл структурної схеми внаслідок:\n" + exception);
            	}       	    		
            }
            else
            {
	        	if (drawingSurface.GetVisualCount() != 0)
    	       	{
    	     		drawingSurface.ClearAll();
    	       	}
	        	drawingSurface.AddVisuals(visualToSerialize.GetShapeBases());
            }
        }
        
        private void SaveCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        	string filePath = string.Empty;
        	if((filePath = OpenSaveClass.SaveDialog(ModelType.Scheme, string.Empty)) != string.Empty)
            {         	
	        	string exception = string.Empty;
    	    	VisualToSerialize vts = new VisualToSerialize();
    	    	vts.SetShapeBase(drawingSurface.GetVisuals());
    	    	
    	       	if (OpenSaveClass.SaveCommand(filePath, vts, out exception))
    	       	{
    	       		mainWindow.BlockSchemePath = filePath;    	       		
    	       	} 
    	       	else
    	       	{
    	       		MessageBox.Show("Неможливо зберегти структурну схему внаслідок:\n" + exception);
    	       	}
        	}
        } 

		private void BSWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(mainWindow.BlockSchemePath))
			{
				string exception = string.Empty;
           		VisualToSerialize visualToSerialize = 
           			(VisualToSerialize)OpenSaveClass.OpenCommand(mainWindow.BlockSchemePath, 
           			                                             ModelType.Scheme, out exception);
    	        if (visualToSerialize == null)
    	        {
   		    		MessageBox.Show("Неможливо відкрити файл структурної схеми внаслідок:\n" + exception);
					visualToSerialize = null;  
					mainWindow.BlockSchemePath = string.Empty;					
    	        }
    	        else
    	        {
		        	if (drawingSurface.GetVisualCount() != 0)
    		       	{
    		     		drawingSurface.ClearAll();
    		       	}
		        	drawingSurface.AddVisuals(visualToSerialize.GetShapeBases());
	            }           		
			}						
		}        
        #endregion				             
        #endregion
	}
}