using System;
using System.Windows;
using System.Windows.Media;

namespace BlockScheme
{
	/// <summary>
	/// Description of BlockTransformation.
	/// </summary>
	public abstract class BlockTransformation : Block
	{
		protected static readonly int PortCount = 2;
		protected static readonly byte DTextWall = 10;							

        #region методы
        public BlockTransformation()
		{
        	BlockSize = BlockMinSize;			         	
		}
		
        protected abstract void ChangeTextLocation();

		public override void ChangeLocation(Point topLeftPoint, bool isSelected)
		{
        	ShapePoints[0] = topLeftPoint;
        	ShapePoints[1] = new Point(ShapePoints[0].X + BlockSize.Width, ShapePoints[0].Y);
        	PortPoints[0][0] = new Point(ShapePoints[0].X, ShapePoints[0].Y + BlockSize.Height / 2);
        	PortPoints[1][0] = new Point(ShapePoints[1].X, ShapePoints[1].Y + BlockSize.Height / 2);
        	ChangeTextLocation();
        	Draw(isSelected, false);
        	
        	int i;
        	
        	for (i = 0; i < PortCount; i++)
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
