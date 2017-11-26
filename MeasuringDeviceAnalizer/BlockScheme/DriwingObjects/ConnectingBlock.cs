using System;

namespace BlockScheme
{
	/// <summary>
	/// Description of ConnectingShape.
	/// </summary>
	public class ConnectingShape
	{
		public Block TargetBlock = null;
        public int PortNum = -1;
        
		public Connection TargetConnection = null;
        public int Node = -1;        
		
		public ConnectingShape()
		{
		}				
		
		public void Clear()
		{
			if (TargetBlock != null)
			{
				TargetBlock = null;
    	        PortNum = -1;
			}
			else
			{
	            TargetConnection = null;
    	        Node = -1;
			}
		}
	}
}
