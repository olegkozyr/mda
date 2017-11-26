using System;

namespace BlockScheme
{
	/// <summary>
	/// Description of BlockMultiConnection.
	/// </summary>
	public abstract class BlockMultiConnection : Block
	{
		protected double PortDeltaMax;
		protected double PortDelta;
		
		public BlockMultiConnection()
		{
		}
		
		public void ClearConnection( int portNum, int minOne)
        {
			if (ConnectionObj[portNum] != null)
			{			
				ConnectionObj[portNum].ClearConnection(ConnectionEnd[portNum]);			
				ClearConnection(portNum);			       	
			}
        }
		
		protected double BlockHeightChange()
		{
			double doubleTemp = BlockSize.Height / PortPoints.Count;
			if (doubleTemp < PortDelta)
			{
				BlockSize.Height = PortDelta * PortPoints.Count;
				return PortDelta;
			}
			
			if (doubleTemp > PortDeltaMax)
			{
				BlockSize.Height = PortDeltaMax * PortPoints.Count;
				return PortDeltaMax;				
			}

			return doubleTemp;
		}
	}
}
